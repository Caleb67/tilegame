using System;
using System.Linq;
using static System.Linq.Enumerable;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;
using NUnit.Framework;

public class Chunk : MonoBehaviour
{
    [SerializeField] Tilemap backgroundTiles;
    [SerializeField] Tilemap foregroundTiles;
    public GlobalTileInfo globalTileInfo;

    bool dirty = false;
    bool modified = false;

    public void MarkClean() {
        this.dirty = false;
    }

    public void SetModified() {
        this.modified = true;
    }

    public bool IsModified() {
        return this.modified;
    }

    public Tile[] GetAllTileData(bool isBackground) {
        var tiles = new Tile[CHUNK_SIZE*CHUNK_SIZE];
        var bounds = new BoundsInt(
            Vector3Int.zero, 
            new(CHUNK_SIZE, CHUNK_SIZE, 1)
        );
        var tilemap = isBackground
            ? this.backgroundTiles
            : this.foregroundTiles;

        tilemap.GetTilesBlockNonAlloc(
            bounds,
            tiles
        );
        return tiles;
    }

    public bool IsDirty() { 
        return this.dirty;
    }

    void Update() {
        if (this.dirty && !this.modified) {
            Destroy(this.gameObject);
        }
        this.dirty = true;
    }


    public const int CHUNK_SIZE = 8;
    public bool TryPutTile(Vector3 worldPos, Tile toPut, bool isBackground) {
        var tilemap = isBackground
            ? this.backgroundTiles
            : this.foregroundTiles;        

        var tilePos = tilemap.WorldToCell(worldPos);

        if (tilePos.x is < 0 or >= CHUNK_SIZE) return false;
        if (tilePos.y is < 0 or >= CHUNK_SIZE) return false;

        if (toPut == null) {
            var emptyId = isBackground
                ? this.globalTileInfo.backgroundEmptyID
                : this.globalTileInfo.foregroundEmptyID;

            toPut = this.globalTileInfo.tiles[emptyId];
        }
        tilemap.SetTile(tilePos, toPut);

        SetModified();

        return true;
    }

    public Tile GetTile(Vector3Int tilePosInChunk, bool isBackground) {
        var tilemap = isBackground
            ? this.backgroundTiles
            : this.foregroundTiles;
        var tileBaseAtPos = tilemap.GetTile(tilePosInChunk);
        if (tileBaseAtPos is not Tile tileAtPos) return null;
        return tileAtPos;
    }

    public Tile GetTile(Vector3 worldPos, bool isBackground) {
        var tilemap = isBackground
            ? this.backgroundTiles
            : this.foregroundTiles;
        var tilePos = tilemap.WorldToCell(worldPos);
        return GetTile(tilePos, isBackground);
    }

    public bool IsInChunk(Vector3 worldPos) {
        var tilePos = this.backgroundTiles.WorldToCell(worldPos);

        if (tilePos.x is < 0 or >= CHUNK_SIZE) return false;
        if (tilePos.y is < 0 or >= CHUNK_SIZE) return false;
        
        return true;
    }

    public bool TryGetTileLocation(Vector3 worldPos, bool isBackground, out TileLocation location) {
        var tilemap = isBackground
            ? this.backgroundTiles
            : this.foregroundTiles;
        
        var tilePos = tilemap.WorldToCell(worldPos);
        
        
        var outOfBoundsX = tilePos.x is < 0 or >= CHUNK_SIZE;
        var outOfBoundsY = tilePos.y is < 0 or >= CHUNK_SIZE;
        if (outOfBoundsX || outOfBoundsY) {
            location = default;
            return false;
        }

        location = new TileLocation(this, tilePos, tilemap.CellToWorld(tilePos), isBackground);
        return true;
    }
    
    public void FillData(byte[] data, bool isBackground) {
        var toPut = data.Take(CHUNK_SIZE*CHUNK_SIZE)
                .Select(
                    tileId => this.globalTileInfo.tiles[tileId]
                );

        var tilemap = isBackground
            ? this.backgroundTiles
            : this.foregroundTiles;

        var bounds = new BoundsInt(
            Vector3Int.zero, 
            new(CHUNK_SIZE, CHUNK_SIZE, 1)
        );

        tilemap.SetTilesBlock(bounds, toPut.ToArray());
    }
}
