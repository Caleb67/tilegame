
using System;
using System.Collections;
using UnityEngine;


public class TileBehaviour : ScriptableObject
{
    public virtual void OnPlaced(
        TileLocation tileLocation, ChunkManager chunkManager, 
        GlobalTileInfo globalTileInfo, GlobalItemInfo globalItemInfo) {}
    public virtual void OnDestroyed(
        TileLocation tileLocation, ChunkManager chunkManager, 
        GlobalTileInfo globalTileInfo, GlobalItemInfo globalItemInfo) {}
    public virtual void OnUpdated(
        TileLocation tileLocation, ChunkManager chunkManager, 
        GlobalTileInfo globalTileInfo, GlobalItemInfo globalItemInfo) {}

    public virtual IEnumerator Ticker() { return null; }

    public virtual void QuitTicking() {}
}