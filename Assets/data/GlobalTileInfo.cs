using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "GlobalTileInfo", menuName = "Scriptable Objects/GlobalTileInfo")]
public class GlobalTileInfo : ScriptableObject {
    public List<TileBase> tiles = new(256);
    public byte backgroundEmptyID;
    public byte foregroundEmptyID;

    [field: SerializeField]
    public byte placeablesStartID {
        get; private set;
    }

    [field: SerializeField]
    public byte placeablesEndID {
        get; private set;
    }

    [SerializeField] public Dictionary<byte, String> tileNames = new();

    [SerializeField] public Dictionary<int, TileBehaviour> tileBehaviours = new();
    [SerializeField] public List<int> foregroundExclusiveTiles;

    public static GlobalTileInfo s_instance {
        get; private set;
    }

    public void PerformOnPlace(
        TileLocation location, TileBase toPut,
        ChunkManager chunkManager, GlobalItemInfo globalItemInfo
    ) {
        var toPutId = this.tiles.IndexOf(toPut);
        if (toPutId == -1) return;
        if (this.tileBehaviours.ContainsKey(toPutId)) {
            this.tileBehaviours[toPutId].OnPlaced(
                location,
                chunkManager,
                this, globalItemInfo
            );
        }
    }
    public void PerformOnDestroy(
        TileLocation location, TileBase toDestroy,
        ChunkManager chunkManager, GlobalItemInfo globalItemInfo
    ) {
        var toDestroyId = this.tiles.IndexOf(toDestroy);
        if (toDestroyId == -1) return;
        if (this.tileBehaviours.ContainsKey(toDestroyId)) {
            this.tileBehaviours[toDestroyId].OnDestroyed(
                location,
                chunkManager,
                this, globalItemInfo
            );
        }
    }

    public void UpdateNeighbours(
        TileLocation location, TileBase toDestroy,
        ChunkManager chunkManager, GlobalItemInfo globalItemInfo
    ) {
        var positions = new[]{
            Vector3Int.up, Vector3Int.down, 
            Vector3Int.left, Vector3Int.right
        };
        
        foreach (var relativePos in positions) {
            var success = chunkManager.TryGetTileLocation(
                location.positionInWorld + relativePos, 
                location.isBackground,
                out var neighbourLocation
            );
            if (!success) continue;
            var tile = neighbourLocation.Resolve();
            var tileId = this.tiles.IndexOf(tile);
            if (tileId == -1) continue;
            if (this.tileBehaviours.ContainsKey(tileId)) {
                this.tileBehaviours[tileId].OnUpdated(
                    neighbourLocation,
                    chunkManager,
                    this, globalItemInfo
                );
            }
        }
    }

    public Sprite GetSpriteForTile(int tileId) {
        var tile = this.tiles[tileId];
        
        return tile switch {
            Tile t => t.sprite,
            AnimatedTile t => t.m_AnimatedSprites[0],
            _ => throw new ArgumentException("not a valid tile to fetch sprite for")
        };
    }

    void OnEnable() {
        if (s_instance == null) {
            s_instance = this;
        }
    }
}
