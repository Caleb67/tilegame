
using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class Chunk {
    public const int CHUNK_SIZE = 8;

    [SerializeField] 
    public byte[] foregroundData = new byte[CHUNK_SIZE*CHUNK_SIZE];

    [field: SerializeField]
    public byte[] backgroundData = new byte[CHUNK_SIZE*CHUNK_SIZE];

    [SerializeField]
    public Vector2Int position;

    public Chunk(Vector2Int position) {
        this.position = position;
    }

    public record TileChunkPos {
        public readonly int x;
        public readonly int y;
        public TileChunkPos(int x, int y) {
            Assert.IsTrue(x is >=0 and < CHUNK_SIZE);
            Assert.IsTrue(y is >=0 and < CHUNK_SIZE);
            this.x = x;
            this.y = y;
        }

        public Vector3Int ToVector() {
            return new(this.x, this.y);
        }
    }

    public byte GetTile(TileChunkPos posInChunk, bool isBackground) {
        Func<TileChunkPos, byte> getData = isBackground
            ? GetBackgroundData
            : GetForegroundData;
        return getData(posInChunk);
    }

    public void PutTile(TileLocation location, byte idToPut, bool isBackground, bool updateNeighbours=false) {
        var original = GetTile(location.positionInChunk, isBackground);

        Action<TileChunkPos, byte> setData = isBackground
            ? SetBackgroundData
            : SetForegroundData;
        
        setData(location.positionInChunk, idToPut);

        location.chunk.SetModified();

        TileBehaviour.PerformOnDestroy(
            location, original, ChunkManager.s_instance
        );
        TileBehaviour.PerformOnPlace(
            location, idToPut, ChunkManager.s_instance
        );

        if (updateNeighbours) {
            TileBehaviour.UpdateNeighbours(
                location, ChunkManager.s_instance
            );
        }
    }

    byte GetForegroundData(TileChunkPos pos) {
        return this.foregroundData[pos.x + (CHUNK_SIZE*pos.y)];
    }
    byte GetBackgroundData(TileChunkPos pos) {
        return this.backgroundData[pos.x + (CHUNK_SIZE*pos.y)];
    }

    void SetForegroundData(TileChunkPos pos, byte data) {
        this.foregroundData[pos.x + (CHUNK_SIZE*pos.y)] = data;
    }
    void SetBackgroundData(TileChunkPos pos, byte data) {
        this.backgroundData[pos.x + (CHUNK_SIZE*pos.y)] = data;
    }

}