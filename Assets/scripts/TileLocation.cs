
using UnityEngine;
using UnityEngine.Tilemaps;

public struct TileLocation {
    public readonly Chunk chunk;
    public readonly Vector3Int positionInChunk;
    public readonly Vector3 positionInWorld;
    public readonly bool isBackground;

    public TileLocation(Chunk chunk, Vector3Int tileChunkPos, Vector3 worldPos, bool isBackground) {
        this.chunk = chunk;
        this.positionInChunk = tileChunkPos;
        this.positionInWorld = worldPos;
        this.isBackground = isBackground;
    }

    public Tile Resolve() {
        return this.chunk.GetTile(this.positionInChunk, this.isBackground);
    }
}