using System.Collections.Generic;
using System.Linq;
using static System.Linq.Enumerable;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class TerrainGenerator : MonoBehaviour
{

    [Header("Noise")]
    
    [SerializeField] float scaleX;
    [SerializeField] float scaleY;

    [Header("Chunk")]
    [SerializeField] Grid chunkManager;
    [SerializeField] int chunkLoadRadius;
    
    [Serializable]
    public struct Variant {
        [Range(0.0f, 1.0f)]
        public float lowerThreshold;
        
        [Serializable]
        public struct ChunkEntry {
            [Range(0.0f, 1.0f)]
            public float lowerThreshold;
            public Chunk chunkPrefab;
        }

        public List<ChunkEntry> entries;

        public Chunk Choose(float variance) {
            foreach (var entry in this.entries) {
                if (variance > entry.lowerThreshold) {
                    return entry.chunkPrefab;
                }
            }
            return null;
        }
    }

    [Header("Variants")]

    [SerializeField] List<Variant> variants = new();
    [SerializeField] Chunk defaultChunkPrefab;

    [Header("Data")]
    [SerializeField] PlayerData playerData;

    Dictionary<Vector3Int, Chunk> loadedChunks = new();

    float offsetX;
    float offsetY;

    float varianceOffsetX;
    float varianceOffsetY;

    int seed;

    bool shouldSave = false;
    public void SetShouldSave() {
        this.shouldSave = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        this.seed = this.playerData.saveSeed;
        System.Random random = new(this.seed);
        this.offsetX = (float) random.NextDouble() * 10_000f;
        this.offsetY = (float) random.NextDouble() * 10_000f;
        this.varianceOffsetX = (float) random.NextDouble() * 10_000f;
        this.varianceOffsetY = (float) random.NextDouble() * 10_000f;
    }

    
    void LateUpdate() {
        // because it needs to always happen after chunks mark themselves dirty
        Tick();
    }

    public void Tick() {
        var curChunkPos = this.chunkManager.WorldToCell(this.transform.position);
        var positionsToLoad = GetPositionsToLoad(curChunkPos);
        foreach (var pos in positionsToLoad) {
            if (!this.loadedChunks.ContainsKey(pos)) {
                this.loadedChunks[pos] = LoadOrGenerate(pos);
            }
            if (this.loadedChunks[pos].IsModified() && this.shouldSave) {
                WriteToFile(this.loadedChunks[pos], pos);
                
            }
            this.loadedChunks[pos].MarkClean();
        }
        this.shouldSave = false;
    }

    void Update() {
        var toRemove = this.loadedChunks.Where(
                        entry => entry.Value.IsDirty()
                    ).ToList();

        foreach (var entry in toRemove) {
            if (entry.Value.IsModified()) {
                WriteToFile(entry.Value, entry.Key);
            }
            Destroy(entry.Value.gameObject);
            this.loadedChunks.Remove(entry.Key);
            
        }
    }

    Vector3Int[] GetPositionsToLoad(Vector3Int center) {
        
        var bottomLeft = new Vector3Int(
            center.x - this.chunkLoadRadius,
            center.y - this.chunkLoadRadius
        );
        var topRight = new Vector3Int(
            center.x + this.chunkLoadRadius,
            center.y + this.chunkLoadRadius
        );
        
        var all = from x in Range(bottomLeft.x, topRight.x - bottomLeft.x)
                from y in Range(bottomLeft.y, topRight.y - bottomLeft.y)
                select new Vector3Int(x, y);
        return all.ToArray();
    }

    Chunk LoadOrGenerate(Vector3Int chunkPos) {
        var chunk = LoadFromFile(chunkPos);
        if (chunk == null) {
            chunk = Generate(chunkPos);
        }
        return chunk;
    }

    Chunk Generate(Vector3Int chunkPos) {
        float x = chunkPos.x + 0.001f;
        float y = chunkPos.y + 0.001f;
        float rand = Mathf.PerlinNoise(
            (x * this.scaleX) + this.offsetX, 
            (y * this.scaleY) + this.offsetY
        );
        float variance = Mathf.PerlinNoise(
            (x * this.scaleX) + this.varianceOffsetX, 
            (y * this.scaleY) + this.varianceOffsetY
        );

        Chunk chunk = null;
        foreach (var variant in this.variants) {
            if (rand > variant.lowerThreshold) {
                chunk = variant.Choose(variance);
                break;
            }
        }
        if (chunk == null) chunk = this.defaultChunkPrefab;

        var realPos = this.chunkManager.CellToWorld(chunkPos);

        return Instantiate(chunk, realPos, Quaternion.identity, this.chunkManager.transform);
    }

    Chunk LoadFromFile(Vector3Int chunkPos) {
        var data = LevelData.LoadFromFile(this.playerData.savePath);

        Chunk chunkOut = null;
        
        foreach (var chunkData in data.chunks) {
            var pos = (Vector3Int)chunkData.position;
            if (pos != chunkPos) continue;

            var realPos = this.chunkManager.CellToWorld(chunkPos);
            chunkOut = Instantiate(this.defaultChunkPrefab, realPos, Quaternion.identity, this.chunkManager.transform);

            chunkOut.FillData(chunkData.foregroundData, isBackground:false);
            chunkOut.FillData(chunkData.backgroundData, isBackground:true);
        }

        return chunkOut;
    }
    
    void WriteToFile(Chunk chunk, Vector3Int chunkPos) {
        var data = LevelData.LoadFromFile(this.playerData.savePath);
        if (data == null) data = new LevelData();

        var foregroundData = chunk.GetAllTileData(false);
        var backgroundData = chunk.GetAllTileData(true);

        data.chunks.RemoveAll(
            savedChunk => (Vector3Int)savedChunk.position == chunkPos
        );
        data.chunks.Add(new ChunkData(chunk, chunkPos));

        LevelData.Save(this.playerData.savePath, data);
    }
}
