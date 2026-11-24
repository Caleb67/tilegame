
using System;
using UnityEngine;


public class TileBehaviour : ScriptableObject
{
    public bool wasUpdatedAlready {
        get; private set;
    }
    public virtual void OnPlaced(
        TileLocation tileLocation, ChunkManager chunkManager, 
        GlobalTileInfo globalTileInfo, GlobalItemInfo globalItemInfo) {}
    public virtual void OnDestroyed(
        TileLocation tileLocation, ChunkManager chunkManager, 
        GlobalTileInfo globalTileInfo, GlobalItemInfo globalItemInfo) {}
    public virtual void OnUpdated(
        TileLocation tileLocation, ChunkManager chunkManager, 
        GlobalTileInfo globalTileInfo, GlobalItemInfo globalItemInfo) {}
}