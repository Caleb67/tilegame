
using UnityEngine;

[CreateAssetMenu(fileName = "MudTileBehaviour", menuName = "Tile Behaviours/MudTileBehaviour")]
public class MudTileBehaviour : TileBehaviour
{
    [SerializeField] int idOfClayTile;
    [SerializeField] int idOfSandTile;
    public override void OnPlaced(TileLocation tileLocation, ChunkManager chunkManager) {
        OnUpdated(tileLocation, chunkManager);
    }

    public override void OnUpdated(TileLocation tileLocation, ChunkManager chunkManager) {
        var positions = new[]{
            Vector3Int.up, Vector3Int.down, 
            Vector3Int.left, Vector3Int.right
        };

        var (success, neighbours) = TileUtil.TryGetAllRelative(tileLocation, positions);
        if (!success) return;

        foreach (var neighbour in neighbours) {
            success = neighbour.TryResolve(
                out byte neighbourTile
            );
            if (!success) return;
            if (neighbourTile != this.idOfSandTile) return;
        }
        
        tileLocation.TryPut((byte)this.idOfClayTile);
    }

}