using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalItemInfo", menuName = "Scriptable Objects/GlobalItemInfo")]
public class GlobalItemInfo : ScriptableObject {
    
    [Serializable]
    public struct ItemDefinition {
        public uint sizeUnits;
        public string name;
    }
    

    public List<ItemDefinition> itemDefinitions = new(256);
    
    [Serializable]
    public struct TileItemContents {
        public static TileItemContents NONE = new() { items = new()};
        public List<Item> items;
    }
    
    [DictionaryDisplay(keyLabel = "Tile ID", valueLabel = "Item Contents")]
    [SerializeField] public Dictionary<int, TileItemContents> tileItemContents = new(256);
}
