using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "GlobalTileInfo", menuName = "Scriptable Objects/GlobalTileInfo")]
public class GlobalTileInfo : ScriptableObject {
    public List<Tile> tiles = new(256);
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
}
