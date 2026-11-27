using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Cosmetic, seed-stable wildlife. Each animal belongs to one loaded chunk.
/// No save-format changes or world mutations are required.</summary>
public sealed class WildlifePopulation : IDisposable {
    const float SpawnChance = 0.42f;
    const int MaxAnimals = 32;
    const byte FireTileId = 22;
    const float BurnDuration = 3.2f;
    static readonly Vector2Int[] Directions = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    sealed class Animal {
        public ChunkGameObject home;
        public GameObject root;
        public SpriteRenderer renderer, shadow;
        public int species;
        public bool burning;
        public float burnElapsed, nextSmoke;
        public SpriteRenderer flames;
        public float idleClock, blinkAfter;
        public System.Random random;
        public Vector2Int cell, target;
        public float speed, pause, phase;
    }

    readonly Dictionary<Vector3Int, Animal> animals = new();
    readonly Sprite[][] sprites = new Sprite[3][];
    readonly Sprite shadowSprite;
    readonly Sprite[] fireFrames;
    readonly List<Vector3Int> expired = new();
    readonly int seed;
    readonly Transform player;
    readonly GlobalTileInfo tiles;

    public WildlifePopulation(int seed, Transform player, GlobalTileInfo tiles) {
        this.seed = seed;
        this.player = player;
        this.tiles = tiles;
        sprites[0] = MakeFrames("Bunny", new[] {
                "................",
                ".......oo..oo...",
                "......ohpoohpo..",
                "......ohpoohpo..",
                "......ohpoohpo..",
                ".......ohhoho...",
                "......ohhhhho...",
                "..oo.ohhhhhhhho.",
                ".ohhohhhhhkhhpo.",
                ".ohhhhhhhhhhhso.",
                "..ohhhhsshhhso..",
                "...ohhssssssso..",
                "...ohho..ohho...",
                "....oo....oo....",
                "................",
                "................"
            }, new Dictionary<char, Color32> {
            ['o'] = new Color32(137, 108, 118, 255),
            ['h'] = new Color32(255, 244, 228, 255),
            ['s'] = new Color32(212, 184, 183, 255),
            ['p'] = new Color32(233, 162, 179, 255),
            ['k'] = new Color32(56, 43, 59, 255)
        }, new (int, string)[] {(12, "..ohho....ohho.."), (13, "...oo......oo...")}, new (int, string)[] {(12, "....ohho.ohho..."), (13, ".....oo...oo....")}, new (int, string)[] {(1, "......oo...oo..."), (8, ".ohhohhhhhohhpo.")});
        sprites[1] = MakeFrames("Chick", new[] {
                "................",
                "................",
                ".........oo.....",
                "........ohho....",
                ".......ohhhho...",
                ".......ohhkhho..",
                "..oo..ohhhhhbbo.",
                ".ohhoohhhhhhbo..",
                ".ohhhhhhhhho....",
                "..ohhhsshhho....",
                "...ohssssho.....",
                "....osssso......",
                ".....o..o.......",
                ".....b..b.......",
                "................",
                "................"
            }, new Dictionary<char, Color32> {
            ['o'] = new Color32(189, 133, 69, 255),
            ['h'] = new Color32(255, 229, 139, 255),
            ['s'] = new Color32(244, 189, 86, 255),
            ['b'] = new Color32(237, 146, 80, 255),
            ['k'] = new Color32(76, 52, 52, 255)
        }, new (int, string)[] {(12, "....o....o......"), (13, "....b....b......")}, new (int, string)[] {(12, "......oo........"), (13, "......bb........")}, new (int, string)[] {(5, ".......ohhohho..")});
        sprites[2] = MakeFrames("Fox", new[] {
                "................",
                ".........o..o...",
                "........oho.oho.",
                "........ohpoho..",
                "........ohhhho..",
                "...oo..ohhhhhho.",
                "..ohhoohhhkhhho.",
                ".owwhhhhhhhhwoo.",
                ".owwhhhhsswwko..",
                "..owhhhhsswwo...",
                "...osssssssso...",
                "....osso.osso...",
                "....okko.okko...",
                ".....oo...oo....",
                "................",
                "................"
            }, new Dictionary<char, Color32> {
            ['o'] = new Color32(133, 77, 67, 255),
            ['h'] = new Color32(245, 162, 97, 255),
            ['s'] = new Color32(206, 111, 71, 255),
            ['p'] = new Color32(248, 182, 160, 255),
            ['k'] = new Color32(64, 47, 57, 255),
            ['w'] = new Color32(255, 240, 214, 255)
        }, new (int, string)[] {(11, "...osso...osso.."), (12, "...okko...okko.."), (13, "....oo.....oo...")}, new (int, string)[] {(11, ".....ossoosso..."), (12, ".....okkookko..."), (13, "......oo..oo....")}, new (int, string)[] {(6, "..ohhoohhhohhho.")});
        shadowSprite = MakeShadowSprite();
        fireFrames = MakeFireFrames();
        goreSprites = MakeGoreSprites();
        ashSprites = MakeAshSprites();
    }

    // Four deliberately drawn poses share a single palette and foot anchor.
    static Sprite[] MakeFrames(string name, string[] idle, Dictionary<char, Color32> palette,
        (int, string)[] stepA, (int, string)[] stepB, (int, string)[] blink) {
        var frames = new Sprite[4];
        frames[0] = MakeSprite(name, idle, palette);
        var patches = new[] { stepA, stepB, blink };
        for (int i = 0; i < patches.Length; i++) {
            var rows = (string[])idle.Clone();
            foreach (var patch in patches[i]) rows[patch.Item1] = patch.Item2;
            frames[i + 1] = MakeSprite(name + " " + (i + 1), rows, palette);
        }
        return frames;
    }

    static Sprite MakeSprite(string name, string[] rows, Dictionary<char, Color32> palette) {
        const int size = 16;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = name + " (wildlife)";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (palette.TryGetValue(rows[y][x], out var color))
                    pixels[(size - 1 - y) * size + x] = color;
        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 3f / size), size);
        sprite.name = name;
        return sprite;
    }

    static Sprite MakeShadowSprite() {
        // Draw a two-tone translucent footprint, aligned below the foot anchor.
        var rows = new string[16];
        for (int i = 0; i < rows.Length; i++) rows[i] = "................";
        rows[12] = ".....aaaaaa.....";
        rows[13] = "...aabbbbbbaa...";
        rows[14] = "....aaaaaaaa....";
        return MakeSprite("Wildlife shadow", rows, new Dictionary<char, Color32> {
            ['a'] = new Color32(35, 27, 36, 34),
            ['b'] = new Color32(35, 27, 36, 67)
        });
    }

    static Sprite[] MakeFireFrames() {
        var palette = new Dictionary<char, Color32> {
            ['r'] = new Color32(175, 58, 37, 225),
            ['o'] = new Color32(242, 111, 38, 245),
            ['y'] = new Color32(255, 195, 58, 245),
            ['w'] = new Color32(255, 242, 161, 255)
        };
        return new[] {
            MakeSprite("Wildlife flame A", new[] { "................", ".........r......", "........ror.....", "....r...roor....", "...ror.rooyr....", "...roorrooyor...", "..rooyrooyyyr...", "..royyrooyyyr...", "..royyyoyywyr...", "..royyoyywwyor..", "..royyoywwwyor..", "...royyywwyor...", "....rooooyor....", ".....rrrrrr.....", "................", "................" }, palette),
            MakeSprite("Wildlife flame B", new[] { "................", ".....r..........", "....ror....r....", "....roor..ror...", "...rooyr.roor...", "...royyrrroor...", "..royyyrooyyr...", "..royyoyoyyyr...", "..royyoyyywyr...", "..royyoyywwyor..", "..royyyoywwyor..", "...royyywwyor...", "....roooyyor....", ".....rrrrrr.....", "................", "................" }, palette)
        };
    }

    // Use the cell actually occupied by the animal, including halfway through a step.
    bool IsOnFire(Animal animal) {
        var current = animal.root.transform.position;
        var cell = animal.cell;
        if (animal.cell != animal.target &&
            (current - Center(animal.home, animal.target)).sqrMagnitude <
            (current - Center(animal.home, cell)).sqrMagnitude)
            cell = animal.target;
        return animal.home.chunk.GetTile(new Chunk.TileChunkPos(cell.x, cell.y), false) == FireTileId;
    }

    void BeginBurning(Animal animal) {
        animal.burning = true;
        animal.burnElapsed = 0f;
        animal.renderer.enabled = true;
        animal.shadow.enabled = true;
        var effect = new GameObject("Wildlife flames");
        effect.transform.SetParent(animal.root.transform, false);
        effect.transform.localScale = new Vector3(1.1f, 1.2f, 1f);
        animal.flames = effect.AddComponent<SpriteRenderer>();
        animal.flames.sortingLayerName = "Player";
        animal.flames.sortingOrder = 1;
        animal.flames.sprite = fireFrames[0];
    }

    readonly Sprite[] ashSprites;
    const float SmokeDuration = 4.5f;

    static Sprite[] MakeAshSprites() {
        var palette = new Dictionary<char, Color32> {
            ['d'] = new Color32(43, 35, 35, 180),
            ['k'] = new Color32(41, 39, 41, 255),
            ['g'] = new Color32(79, 75, 73, 255),
            ['a'] = new Color32(137, 129, 119, 255),
            ['r'] = new Color32(79, 27, 30, 255),
            ['m'] = new Color32(113, 49, 40, 255),
            ['w'] = new Color32(177, 157, 126, 255),
            ['s'] = new Color32(61, 59, 63, 100),
            ['c'] = new Color32(100, 95, 94, 155),
            ['e'] = new Color32(217, 82, 30, 255),
            ['h'] = new Color32(255, 198, 76, 255)
        };
        var art = new[] {
            new[] { "................", "................", "................", "................", "................", "................", "................", ".....d.d........", "...d.dkkdd......", "..dkkgkagkdd....", ".ddkagkkgkadd...", ".dkkgkagkkgkdd..", "..dkkgkkagkdd...", "...ddddkddddd...", ".....d...d......", "................" },
            new[] { "................", "................", "................", "................", "................", ".......k........", "......kgk.......", "....kkggakk.....", "...kgkggkggk....", "..kggagkgagkk...", ".kkggkgagkgkkk..", ".kggagkkgkgggk..", "..kkgkgagkggkk..", "...kkkkgkkkkk...", ".....kk..k......", "................" },
            new[] { "................", "................", "................", "................", "................", "........d.......", "......ddkd......", "....ddkgkkd.....", "...dkggkagkd....", "..dkgrgkkgmkd...", ".dkkgrkkwkgkkd..", ".dkgkmgkgkkgkd..", "..dkkgkakkgkd...", "...ddkddkkddd...", "......dd........", "................" },
            new[] { "................", "................", "................", "................", "................", "......ss........", ".....sccs.......", "....scccccs.....", "...ssccccss.....", "...sccgcccs.....", "....sccccs......", ".....ssss.......", "................", "................", "................", "................" },
            new[] { "................", "................", "................", "................", "................", "................", ".......e........", "......ehe.......", ".......e........", "................", "................", "................", "................", "................", "................", "................" }
        };
        var result = new Sprite[art.Length];
        for (int i=0;i<art.Length;i++) result[i]=MakeSprite("Wildlife ash "+i,art[i],palette);
        return result;
    }

    void EmitAshParticle(Vector3Int chunk, ChunkGameObject home, Vector3 origin,
        System.Random random, bool smoke) {
        if (goreParticles.Count >= MaxGoreParticles) return;
        float drift = ((float)random.NextDouble()-0.5f)*0.8f;
        var root = MakeGoreObject(home, smoke ? "Wildlife smoke" : "Wildlife ember",
            ashSprites[smoke ? 3 : 4], origin, smoke ? 0.6f : 0.45f, 3);
        goreParticles.Add(new GoreParticle {
            root=root, chunk=chunk, origin=origin, smoke=smoke, atmospheric=true,
            velocity=new Vector3(drift, smoke ? 0.75f : 1.3f, 0),
            duration=smoke ? 1.6f+(float)random.NextDouble()*0.8f : 0.55f+(float)random.NextDouble()*0.6f
        });
    }

    bool TickBurning(Vector3Int chunk, Animal animal, float deltaTime) {
        animal.burnElapsed += deltaTime;
        float t = animal.burnElapsed;
        float progress = Math.Min(1f,t/BurnDuration);
        animal.flames.sprite = fireFrames[(int)(t * 12f) & 1];
        float flameScale = progress < 0.65f ? 1.2f : 1.2f-(progress-0.65f)*2.5f;
        animal.flames.transform.localScale = new Vector3(flameScale,flameScale,1f);
        animal.renderer.sprite = progress >= 0.86f ? ashSprites[2] : sprites[animal.species][3];
        byte tint = (byte)(255-185*Math.Min(1f,progress/0.85f));
        animal.renderer.color = progress >= 0.86f ? new Color32(255,255,255,255) : new Color32(tint,tint,tint,255);
        float baseScale = animal.species == 1 ? 0.82f : animal.species == 2 ? 1.08f : 0.96f;
        float collapse = 1f-0.68f*Math.Max(0f,(progress-0.45f)/0.55f);
        animal.renderer.transform.localScale = progress >= 0.86f
            ? new Vector3(baseScale,baseScale,1f)
            : new Vector3(baseScale*(1f+0.2f*progress),baseScale*collapse,1f);
        animal.renderer.transform.localPosition = new Vector3(
            Mathf.Sin(t*23f)*0.025f*(1f-progress),0,0);
        if (t >= animal.nextSmoke) {
            animal.nextSmoke = t+0.15f;
            EmitAshParticle(chunk,animal.home,animal.root.transform.position+new Vector3(0,0.3f,0),animal.random,true);
            if (progress < 0.85f) EmitAshParticle(chunk,animal.home,animal.root.transform.position,animal.random,false);
        }
        return t >= BurnDuration;
    }

    sealed class GoreParticle {
        public GameObject root;
        public Vector3 origin, velocity;
        public float elapsed, duration;
        public bool smoke, atmospheric;
        public Vector3Int chunk;
    }

    sealed class GoreDecal {
        public GameObject root;
        public Vector3Int chunk;
        public ChunkGameObject home;
        public SpriteRenderer renderer;
        public float age, nextSmoke, scale;
        public bool ash;
        public System.Random random;
    }

    readonly List<GoreParticle> goreParticles = new();
    readonly List<GoreDecal> goreDecals = new();
    readonly Sprite[] goreSprites;
    const int MaxGoreParticles = 180;
    const int MaxGoreDecals = 96;
    const int GoreBurstCount = 10;

    static Sprite[] MakeGoreSprites() {
        var palette = new Dictionary<char, Color32> {
            ['d'] = new Color32(82, 14, 26, 255),
            ['r'] = new Color32(151, 24, 38, 255),
            ['b'] = new Color32(224, 39, 51, 255),
            ['h'] = new Color32(255, 98, 87, 255),
            ['m'] = new Color32(112, 39, 45, 255),
            ['s'] = new Color32(53, 32, 38, 255),
            ['w'] = new Color32(242, 219, 183, 255)
        };
        var art = new[] {
            new[] {"................","................","................","................","................","................",".......db.......","......dbrd......","......rbd.......",".......d........","................","................","................","................","................","................"},
            new[] {"................","................","................","................","................","......dd........",".....drbbd......","....dbrhrmd.....",".....drbmd......","......ddd.......","................","................","................","................","................","................"},
            new[] {"................","................","................","................","................",".....ww.........","....wwd.........",".....www........","......wwww......","........wdw.....",".........ww.....","................","................","................","................","................"},
            new[] {"................","................",".....d..........","...........r....","....r..dd.......",".....ddrrdd.....","..d.drrbrrrd.d..","...drrbbrrrdd...","..ddrbbrbrrrdd..","....drrrrrdd..r.","...r.dddddd.....",".......d........","............d...",".....r..........","................","................"},
            new[] {"................","................","................","................","................",".....dd...d.....","...ddmmdddd.....","..drmssrrmmdd...","..drsswrrsmmrd..","...dmswbrswwrd..","..ddrrmmsmwwd...","....ddrrrddd....","......ddd.......","................","................","................"}
        };
        var result = new Sprite[art.Length];
        for (int i=0;i<art.Length;i++)
            result[i]=MakeSprite("Wildlife gore "+i, art[i], palette);
        return result;
    }

    GameObject MakeGoreObject(ChunkGameObject home, string name, Sprite sprite,
        Vector3 worldPosition, float scale, int order) {
        var obj = new GameObject(name);
        obj.transform.SetParent(home.transform, false);
        obj.transform.position = worldPosition;
        obj.transform.localScale = new Vector3(scale, scale, 1f);
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerName = "Player";
        renderer.sortingOrder = order;
        return obj;
    }

    void AddGoreDecal(Vector3Int chunk, ChunkGameObject home, Vector3 position,
        int spriteIndex, float scale) {
        if (goreDecals.Count >= MaxGoreDecals) {
            UnityEngine.Object.Destroy(goreDecals[0].root);
            goreDecals.RemoveAt(0);
        }
        var obj = MakeGoreObject(home, "Wildlife remains", goreSprites[spriteIndex],
            position, scale, -4);
        goreDecals.Add(new GoreDecal { root = obj, chunk = chunk });
    }

    void SpawnGore(Vector3Int chunk, Animal animal) {
        Vector3 center = animal.root.transform.position;
        AddGoreDecal(chunk, animal.home, center, 3, 1.2f);
        // Ash covers the scorched patch; a small amount of dark red debris remains beneath.
        var ash = MakeGoreObject(animal.home,"Wildlife ash pile",ashSprites[2],center,1.25f,-3);
        if (goreDecals.Count >= MaxGoreDecals) {
            UnityEngine.Object.Destroy(goreDecals[0].root);
            goreDecals.RemoveAt(0);
        }
        goreDecals.Add(new GoreDecal {
            root=ash, chunk=chunk, home=animal.home, renderer=ash.GetComponent<SpriteRenderer>(),
            ash=true, random=animal.random, scale=1.25f
        });
        for (int i=0;i<GoreBurstCount;i++) {
            if (goreParticles.Count >= MaxGoreParticles) break;
            double angle = animal.random.NextDouble() * Math.PI * 2;
            float speed = 0.25f + (float)animal.random.NextDouble() * 0.95f;
            int type = i % 5 == 0 ? 2 : i % 3 == 0 ? 1 : 0;
            var root = MakeGoreObject(animal.home, "Wildlife debris", goreSprites[type],
                center, 0.45f + (float)animal.random.NextDouble()*0.35f, 2);
            goreParticles.Add(new GoreParticle {
                root = root, chunk = chunk,
                origin = center, velocity = new Vector3((float)Math.Cos(angle)*speed,
                    (float)Math.Sin(angle)*speed, 0),
                duration = 0.35f + (float)animal.random.NextDouble()*0.45f
            });
        }
    }

    void TickGore(float deltaTime) {
        foreach (var decal in goreDecals) {
            if (!decal.ash || decal.root == null || decal.home == null) continue;
            decal.age += deltaTime;
            if (decal.age >= SmokeDuration) {
                decal.renderer.sprite = ashSprites[1];
                continue;
            }
            if (decal.age >= decal.nextSmoke) {
                decal.nextSmoke = decal.age+0.38f;
                EmitAshParticle(decal.chunk,decal.home,decal.root.transform.position,decal.random,true);
                if (decal.age < 1.6f) EmitAshParticle(decal.chunk,decal.home,decal.root.transform.position,decal.random,false);
            }
            decal.renderer.sprite = decal.age > 2.2f ? ashSprites[0] : ashSprites[2];
        }
        for (int i=goreParticles.Count-1;i>=0;i--) {
            var p=goreParticles[i];
            if (p.root == null) { goreParticles.RemoveAt(i); continue; }
            p.elapsed += deltaTime;
            float t = Math.Min(p.elapsed,p.duration);
            float progress = t/p.duration;
            float distance = p.atmospheric ? t : t*(1f-0.45f*progress);
            float hop = p.atmospheric ? 0f : (float)Math.Sin(progress*Math.PI)*0.12f;
            if (p.atmospheric) {
                float scale = p.smoke ? 0.6f+progress*0.85f : 0.45f*(1f-progress*0.65f);
                p.root.transform.localScale = new Vector3(scale,scale,1f);
                p.root.GetComponent<SpriteRenderer>().color = new Color32(255,255,255,(byte)(255*(1f-progress)));
            }
            p.root.transform.position = p.origin + new Vector3(
                p.velocity.x*distance, p.velocity.y*distance+hop, 0f);
            if (p.elapsed < p.duration) continue;
            UnityEngine.Object.Destroy(p.root);
            goreParticles.RemoveAt(i);
        }
    }

    void ClearChunkGore(Vector3Int chunk) {
        for (int i=goreParticles.Count-1;i>=0;i--)
            if (goreParticles[i].chunk.Equals(chunk)) {
                UnityEngine.Object.Destroy(goreParticles[i].root);
                goreParticles.RemoveAt(i);
            }
        for (int i=goreDecals.Count-1;i>=0;i--)
            if (goreDecals[i].chunk.Equals(chunk)) {
                UnityEngine.Object.Destroy(goreDecals[i].root);
                goreDecals.RemoveAt(i);
            }
    }

    static int ChunkSeed(int seed, Vector3Int position) {
        unchecked { return ((seed * 397) ^ position.x * 73856093) ^ position.y * 19349663; }
    }

    // Natural ground only; a foreground object (including fire) always blocks movement.
    bool IsWalkable(ChunkGameObject home, Vector2Int cell) {
        if (home == null || home.chunk == null || tiles == null ||
            cell.x < 0 || cell.y < 0 || cell.x >= Chunk.CHUNK_SIZE || cell.y >= Chunk.CHUNK_SIZE)
            return false;
        var pos = new Chunk.TileChunkPos(cell.x, cell.y);
        byte ground = home.chunk.GetTile(pos, true);
        byte foreground = home.chunk.GetTile(pos, false);
        return foreground == tiles.foregroundEmptyID && ground != tiles.backgroundEmptyID &&
            (ground == 4 || ground == 5 || ground == 6);
    }

    static Vector3 Center(ChunkGameObject home, Vector2Int cell) =>
        home.ChunkPosToWorld(new Chunk.TileChunkPos(cell.x, cell.y)) + new Vector3(0.5f, 0.5f, 0);

    public void OnChunkLoaded(Vector3Int position, ChunkGameObject home) {
        if (animals.Count >= MaxAnimals || animals.ContainsKey(position)) return;
        var random = new System.Random(ChunkSeed(seed, position));
        if (random.NextDouble() >= SpawnChance) return;

        var candidates = new List<Vector2Int>();
        for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                var cell = new Vector2Int(x, y);
                if (IsWalkable(home, cell) &&
                    (player == null || (player.position - Center(home, cell)).sqrMagnitude > 2.25f))
                    candidates.Add(cell);
            }
        if (candidates.Count == 0) return;
        var start = candidates[random.Next(candidates.Count)];
        var species = random.Next(sprites.Length);
        var root = new GameObject(sprites[species][0].name);
        root.transform.SetParent(home.transform, false);
        root.transform.position = Center(home, start);
        var shadowObject = new GameObject("Shadow");
        shadowObject.transform.SetParent(root.transform, false);
        var shadow = shadowObject.AddComponent<SpriteRenderer>();
        shadow.sprite = shadowSprite;
        shadow.sortingLayerName = "Player";
        shadow.sortingOrder = -2;
        // Foxes have a longer footprint; chicks are especially tiny.
        shadowObject.transform.localScale = species == 2 ? new Vector3(1.15f, 0.9f, 1f)
            : species == 1 ? new Vector3(0.78f, 0.75f, 1f) : new Vector3(0.95f, 0.85f, 1f);

        var visual = new GameObject("Pixel art");
        visual.transform.SetParent(root.transform, false);
        var renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = sprites[species][0];
        float scale = species == 1 ? 0.82f : species == 2 ? 1.08f : 0.96f;
        visual.transform.localScale = new Vector3(scale, scale, 1f);
        renderer.sortingLayerName = "Player";
        renderer.sortingOrder = -1;
        renderer.flipX = random.Next(2) == 0;
        animals.Add(position, new Animal {
            home = home, root = root, renderer = renderer, shadow = shadow, random = random,
            species = species, blinkAfter = 1.5f + (float)random.NextDouble() * 3f,
            cell = start, target = start, speed = (species == 2 ? 0.8f : 0.55f) + (float)random.NextDouble() * 0.25f,
            pause = 0.5f + (float)random.NextDouble() * 2f
        });
    }

    public void OnChunkUnloaded(Vector3Int position) {
        ClearChunkGore(position);
        RemoveAnimal(position);
    }

    void RemoveAnimal(Vector3Int position) {
        if (!animals.TryGetValue(position, out var animal)) return;
        if (animal.root != null) UnityEngine.Object.Destroy(animal.root);
        animals.Remove(position);
    }

    public void Tick(float deltaTime) {
        if (deltaTime <= 0) return;
        TickGore(deltaTime);
        expired.Clear();
        foreach (var entry in animals) {
            var animal = entry.Value;
            if (animal.home == null || animal.root == null) {
                expired.Add(entry.Key);
                continue;
            }
            // Fire contact takes precedence over ordinary obstacle avoidance.
            if (!animal.burning && IsOnFire(animal)) BeginBurning(animal);
            if (animal.burning) {
                if (TickBurning(entry.Key, animal, deltaTime)) {
                    SpawnGore(entry.Key, animal);
                    expired.Add(entry.Key);
                }
                continue;
            }
            // If an edit removes the ground or blocks an animal, hide it rather than
            // walking through an obstacle. It may reappear once the cell is restored.
            if (!IsWalkable(animal.home, animal.cell)) {
                animal.renderer.enabled = false;
                animal.shadow.enabled = false;
                animal.target = animal.cell;
                animal.root.transform.position = Center(animal.home, animal.cell);
                continue;
            }
            animal.renderer.enabled = true;
            animal.shadow.enabled = true;

            if (!IsWalkable(animal.home, animal.target)) {
                animal.target = animal.cell;
                animal.root.transform.position = Center(animal.home, animal.cell);
            }

            var target = Center(animal.home, animal.target);
            if (animal.cell != animal.target) {
                animal.root.transform.position = Vector3.MoveTowards(
                    animal.root.transform.position, target, animal.speed * deltaTime);
                animal.phase += deltaTime * (animal.species == 0 ? 9f : animal.species == 1 ? 13f : 11f);
                float bounce = animal.species == 0 ? 0.11f : animal.species == 1 ? 0.045f : 0.025f;
                animal.renderer.transform.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(animal.phase)) * bounce, 0);
                animal.renderer.sprite = sprites[animal.species][1 + ((int)(animal.phase / 1.57f) & 1)];
                animal.idleClock = 0f;
                if ((animal.root.transform.position - target).sqrMagnitude < 0.0001f) {
                    animal.cell = animal.target;
                    animal.pause = 0.6f + (float)animal.random.NextDouble() * 2.4f;
                    animal.renderer.transform.localPosition = Vector3.zero;
                    animal.renderer.sprite = sprites[animal.species][0];
                }
                continue;
            }

            animal.renderer.transform.localPosition = Vector3.zero;
            animal.idleClock += deltaTime;
            if (animal.idleClock >= animal.blinkAfter + 0.16f) {
                animal.idleClock = 0f;
                animal.blinkAfter = 1.7f + (float)animal.random.NextDouble() * 3.5f;
            }
            animal.renderer.sprite = sprites[animal.species][animal.idleClock >= animal.blinkAfter ? 3 : 0];
            animal.pause -= deltaTime;
            if (animal.pause > 0) continue;
            animal.pause = 0.4f + (float)animal.random.NextDouble();
            int first = animal.random.Next(Directions.Length);
            for (int i = 0; i < Directions.Length; i++) {
                var direction = Directions[(first + i) % Directions.Length];
                var next = animal.cell + direction;
                if (!IsWalkable(animal.home, next)) continue;
                if (player != null && (Center(animal.home, next) - player.position).sqrMagnitude < 1f) continue;
                animal.target = next;
                if (direction.x != 0) animal.renderer.flipX = direction.x < 0;
                break;
            }
        }
        foreach (var key in expired) RemoveAnimal(key);
    }

    public void Dispose() {
        foreach (var animal in animals.Values)
            if (animal.root != null) UnityEngine.Object.Destroy(animal.root);
        animals.Clear();
        foreach (var p in goreParticles) if (p.root != null) UnityEngine.Object.Destroy(p.root);
        foreach (var d in goreDecals) if (d.root != null) UnityEngine.Object.Destroy(d.root);
        goreParticles.Clear();
        goreDecals.Clear();
        foreach (var frames in sprites)
            foreach (var sprite in frames) DestroySprite(sprite);
        DestroySprite(shadowSprite);
        foreach (var frame in fireFrames) DestroySprite(frame);
        foreach (var frame in goreSprites) DestroySprite(frame);
        foreach (var frame in ashSprites) DestroySprite(frame);
    }

    static void DestroySprite(Sprite sprite) {
        if (sprite == null) return;
        var texture = sprite.texture;
        UnityEngine.Object.Destroy(sprite);
        UnityEngine.Object.Destroy(texture);
    }
}
