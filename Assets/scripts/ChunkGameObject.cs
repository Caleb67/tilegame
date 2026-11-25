using System;
using System.Linq;
using static System.Linq.Enumerable;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class ChunkGameObject : MonoBehaviour
{
    public Chunk chunk;
    public GlobalTileInfo globalTileInfo;
    public GlobalItemInfo globalItemInfo;
    
    [SerializeField] Tilemap backgroundTiles;
    [SerializeField] Tilemap foregroundTiles;

    bool dirty = false;
    bool modified = false;

    ChunkManager chunkManager;

    public void SetModified() { this.modified = true; }
    public void MarkClean() { this.dirty = false; }

    public bool IsModified() => this.modified;
    public bool IsDirty() => this.dirty;
    
    public bool TryGetPosInChunk(Vector3 worldPos, out Chunk.TileChunkPos tileChunkPos) {
        tileChunkPos = default;
        var tilePos = this.backgroundTiles.WorldToCell(worldPos);

        if (tilePos.x is < 0 or >= Chunk.CHUNK_SIZE) return false;
        if (tilePos.y is < 0 or >= Chunk.CHUNK_SIZE) return false;
        
        tileChunkPos = new Chunk.TileChunkPos(tilePos.x, tilePos.y);
        return true;
    }

    public Vector3 ChunkPosToWorld(Chunk.TileChunkPos tileChunkPos) {
        var asVector = tileChunkPos.ToVector();
        return this.backgroundTiles.CellToWorld(asVector);
    }

    /// <summary>
    /// perform a random tick for a random tile if it accepts random ticks
    /// </summary>
    /// <remarks>
    /// performs tick on the background tile first, then the foreground
    /// </remarks>
    public void RandomlyTick() {
        var x = this.random.Next(0,8);
        var y = this.random.Next(0,8);
        var pos = new Chunk.TileChunkPos(x, y);
        _RandomlyTick(pos, isBackground:true);
        _RandomlyTick(pos, isBackground:false);
    }

    System.Random random = new();
    void _RandomlyTick(Chunk.TileChunkPos posInChunk, bool isBackground) {
        var tilemap = isBackground 
            ? this.backgroundTiles
            : this.foregroundTiles;

        var tile = this.chunk.GetTile(posInChunk, isBackground);

        TileBehaviour.PerformOnRandomlyTick(
            new TileLocation(
                this,
                posInChunk, tilemap.CellToWorld(posInChunk.ToVector()),
                isBackground
            ),
            tile,
            ChunkManager.s_instance
        );
    }

    void Update() {
        if (this.dirty && !this.modified) {
            Destroy(this.gameObject);
        }
        this.dirty = true;

        if (this.modified) {
            UpdateVisibleTilemap();
        }
    }
    void Awake() {
        this.chunkManager = FindObjectOfType<ChunkManager>();
    }

    void Start() {
        UpdateVisibleTilemap();
    }

    void UpdateVisibleTilemap() {
        var bounds = new BoundsInt(
            new(0,0,0), new(Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE, 1)
        );

        var backgroundData = this.chunk.backgroundData
                .Select(id => GlobalTileInfo.s_instance.tiles[id]);
        this.backgroundTiles.SetTilesBlock(bounds, backgroundData.ToArray());

        var foregroundData = this.chunk.foregroundData
                .Select(id => GlobalTileInfo.s_instance.tiles[id]);
        Debug.Log($"forground data size : {foregroundData.Count()}, background : {backgroundData.Count()}");
        this.foregroundTiles.SetTilesBlock(bounds, foregroundData.ToArray());
    }

    [CustomEditor(typeof(ChunkGameObject))]
    public class FillChunkDataFromTilemap : Editor
    {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            ChunkGameObject chunk = (ChunkGameObject)this.target;

            if (GUILayout.Button("Fill Chunk from Tilemap")) {
                FillChunkData(chunk);
            }
        }

        void FillChunkData(ChunkGameObject chunk) {
            var bounds = new BoundsInt(
                new(0,0,0), new(Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE, 1)
            );
            TileBase[] backgroundData = new TileBase[Chunk.CHUNK_SIZE*Chunk.CHUNK_SIZE];
            var _ = chunk.backgroundTiles.GetTilesBlockNonAlloc(bounds, backgroundData);
        
            TileBase[] foregroundData = new TileBase[Chunk.CHUNK_SIZE*Chunk.CHUNK_SIZE];
            _ = chunk.foregroundTiles.GetTilesBlockNonAlloc(bounds, foregroundData);

            var backgroundDataIds = backgroundData.Select(
                tile => (byte)GlobalTileInfo.s_instance.tiles.IndexOf(tile)
            ).ToArray();
            var foregroundDataIds = foregroundData.Select(
                tile => (byte)GlobalTileInfo.s_instance.tiles.IndexOf(tile)
            ).ToArray();

            var lengthToCopy = Math.Min(chunk.chunk.backgroundData.Length, backgroundDataIds.Length);
            Array.Copy(backgroundDataIds, chunk.chunk.backgroundData, lengthToCopy);

            lengthToCopy = Math.Min(chunk.chunk.foregroundData.Length, foregroundDataIds.Length);
            Array.Copy(foregroundDataIds, chunk.chunk.foregroundData, lengthToCopy);
        }

    }

    

}
