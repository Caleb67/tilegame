
using UnityEngine;

[CreateAssetMenu(fileName = "DirtTileBehaviour", menuName = "Tile Behaviours/DirtTileBehaviour")]
public class DirtTileBehaviour : TileBehaviour
{
    [SerializeField] int indexOfMudTile;
    [SerializeField] int idOfWaterTile;
    public override void OnPlaced(TileLocation tileLocation, ChunkManager chunkManager) {
        OnUpdated(tileLocation, chunkManager);
    }

    public override void OnUpdated(TileLocation tileLocation, ChunkManager chunkManager) {
        if (!tileLocation.isBackground) return;

        var positions = new[]{
            Vector3Int.up, Vector3Int.down, 
            Vector3Int.left, Vector3Int.right
        };

        var (success, neighbours) = TileUtil.TryGetAllRelative(tileLocation, positions);

        foreach (var neighbour in neighbours) {
            success = neighbour.TryResolve(
                out var tile
            );
            if (!success) continue;
            
            if (tile == this.idOfWaterTile) {
                tileLocation.TryPut((byte)this.indexOfMudTile);
                break;
            }
        }
    }
}