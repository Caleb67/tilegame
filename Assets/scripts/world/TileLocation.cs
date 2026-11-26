
using System;
using UnityEngine;

public struct TileLocation {
    public readonly ChunkGameObject chunk;
    public readonly Chunk.TileChunkPos positionInChunk;
    public readonly Vector3 positionInWorld;
    public readonly bool isBackground;

    public TileLocation(ChunkGameObject chunk, Chunk.TileChunkPos tileChunkPos, Vector3 worldPos, bool isBackground) {
        this.chunk = chunk;
        this.positionInChunk = tileChunkPos;
        this.positionInWorld = worldPos;
        this.isBackground = isBackground;
    }

    public readonly bool TryResolve(out byte tile) {
        tile = default;
        try {
            tile = this.chunk.chunk.GetTile(this.positionInChunk, this.isBackground);    
            return true;
        } catch (NullReferenceException) {
            return false;
        }
    }

    public bool TryPut(byte tile, bool updateNeighbours = false) {
        try {
            this.chunk.chunk.PutTile(this, tile, this.isBackground);    
            return true;
        } catch (NullReferenceException) {
            Debug.Log($"failed to place tile at: {this.positionInWorld}, tile id: {tile}");
            return false;
        }
    }

    public readonly bool IsSameLocation(TileLocation other) {
        return (
            other.chunk == this.chunk &&
            other.positionInChunk == this.positionInChunk
        );
    }
}