
using System.Numerics;
using UnityEngine;

[CreateAssetMenu(fileName = "DirtTileBehaviour", menuName = "Tile Behaviours/DirtTileBehaviour")]
public class DirtTileBehaviour : TileBehaviour
{
    [SerializeField] int indexOfMudTile;
    public override void OnPlaced(
        TileLocation tileLocation, ChunkManager chunkManager,
        GlobalTileInfo globalTileInfo, GlobalItemInfo globalItemInfo
    ) {
        OnUpdated(tileLocation, chunkManager, globalTileInfo, globalItemInfo);
    }

    public override void OnUpdated(
        TileLocation tileLocation, ChunkManager chunkManager,
        GlobalTileInfo globalTileInfo, GlobalItemInfo globalItemInfo
    ) {
        if (!tileLocation.isBackground) return;

        var positions = new[]{
            Vector3Int.up, Vector3Int.down, 
            Vector3Int.left, Vector3Int.right
        };
        
        foreach (var relativePos in positions) {
            var success = chunkManager.TryGetTileLocation(
                tileLocation.positionInWorld + relativePos, 
                isBackground:true,
                out var location
            );
            if (!success) continue;
            
            var tile = location.Resolve();
            var tileId = globalTileInfo.tiles.IndexOf(tile);
            if (tileId == -1) continue;
            
            if (tileId == globalTileInfo.backgroundEmptyID/*water*/) {
                tileLocation.TryPut(globalTileInfo.tiles[this.indexOfMudTile]);
                break;
            }
        }
    }
}