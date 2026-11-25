
using System.Numerics;
using UnityEngine;

[CreateAssetMenu(fileName = "MudTileBehaviour", menuName = "Tile Behaviours/MudTileBehaviour")]
public class MudTileBehaviour : TileBehaviour
{
    [SerializeField] int idOfClayTile;
    [SerializeField] int idOfSandTile;
    public override void OnPlaced(
        TileLocation tileLocation, ChunkManager chunkManager
    ) {
        OnUpdated(tileLocation, chunkManager);
    }

    public override void OnUpdated(
        TileLocation tileLocation, ChunkManager chunkManager
    ) {

        var positions = new[]{
            Vector3Int.up, Vector3Int.down, 
            Vector3Int.left, Vector3Int.right
        };
        
        foreach (var relativePos in positions) {
            var success = chunkManager.TryGetTileLocation(
                tileLocation.positionInWorld + relativePos, 
                tileLocation.isBackground,
                out var location
            );
            if (!success) return;
            
            var tile = location.Resolve();
            var tileId = GlobalTileInfo.s_instance.tiles.IndexOf(tile);
            if (tileId == -1) return;
            
            if (tileId != this.idOfSandTile) {
                return;
            }
        }
        
        tileLocation.TryPut(GlobalTileInfo.s_instance.tiles[this.idOfClayTile]);
    }
}