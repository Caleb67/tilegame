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

    public Sprite GetSpriteForTile(int tileId) {
        var tile = this.tiles[tileId];
        
        return tile switch {
            Tile t => t.sprite,
            AnimatedTile t => t.m_AnimatedSprites[0],
            _ => throw new ArgumentException($"not a valid tile to fetch sprite for tile id {tileId}")
        };
    }

    void OnEnable() {
        if (s_instance == null) {
            s_instance = this;
        }
    }
}
