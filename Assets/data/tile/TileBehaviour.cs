
using System;
using System.Collections;
using UnityEngine;


public class TileBehaviour : ScriptableObject
{
    public virtual void OnPlaced(TileLocation tileLocation, ChunkManager chunkManager) {}
    public virtual void OnDestroyed(TileLocation tileLocation, ChunkManager chunkManager) {}
    public virtual void OnUpdated(TileLocation tileLocation, ChunkManager chunkManager) {}
    public virtual void OnRandomlyTick(TileLocation tileLocation, ChunkManager chunkManager) {}

    public virtual IEnumerator Ticker() { return null; }

    public virtual void QuitTicking() {}


    public static void PerformOnPlace(TileLocation location, byte tileToPut, ChunkManager chunkManager) {
        var tileInfo = GlobalTileInfo.s_instance;
        if (tileInfo.tileBehaviours.ContainsKey(tileToPut)) {
            tileInfo.tileBehaviours[tileToPut].OnPlaced(
                location,
                chunkManager
            );
        }
    }
    public static void PerformOnDestroy(TileLocation location, byte tileToDestroy, ChunkManager chunkManager) {
        var tileInfo = GlobalTileInfo.s_instance;
        if (tileInfo.tileBehaviours.ContainsKey(tileToDestroy)) {
            tileInfo.tileBehaviours[tileToDestroy].OnDestroyed(
                location,
                chunkManager
            );
        }
    }
    public static void PerformOnRandomlyTick(TileLocation location, byte tileToTick, ChunkManager chunkManager)
    {
        var tileInfo = GlobalTileInfo.s_instance;
        if (tileInfo.tileBehaviours.ContainsKey(tileToTick)) {
            tileInfo.tileBehaviours[tileToTick].OnRandomlyTick(
                location,
                chunkManager
            );
        }
    }
    public static void UpdateNeighbours(TileLocation location, ChunkManager chunkManager) {
        var tileInfo = GlobalTileInfo.s_instance;

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
            success = location.TryResolve(
                out byte neighbourTile
            );
            if (!success) continue;

            if (tileInfo.tileBehaviours.ContainsKey(neighbourTile)) {
                tileInfo.tileBehaviours[neighbourTile].OnUpdated(
                    neighbourLocation,
                    chunkManager
                );
            }
        }
    }

}