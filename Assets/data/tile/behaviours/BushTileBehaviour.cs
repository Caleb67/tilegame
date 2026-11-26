
using UnityEngine;

[CreateAssetMenu(fileName = "BushTileBehaviour", menuName = "Tile Behaviours/BushTileBehaviour")]
public class BushTileBehaviour : TileBehaviour
{
    [SerializeField] int indexOfBushTile;
    [SerializeField] int indexOfWoodTile;
    [Space(5)]
    [SerializeField] int indexOfAshTile;
    [SerializeField] int indexOfDirtTile;
    [SerializeField] int idOfEmptyTile;

    [Space(10)]
    [Range(0f, 1f)]
    [SerializeField] float spreadChanceEachRandomTick; 
    [Range(0f, 1f)]
    [SerializeField] float ashConsumptionChanceOnSpread; 
    [Range(0f, 1f)]
    [SerializeField] float woodChance; 

    public override void OnRandomlyTick(TileLocation tileLocation, ChunkManager chunkManager) {
        if (tileLocation.isBackground) return;
        if (!ShouldSpread()) return;

        var positions = new[]{
            Vector3Int.up, Vector3Int.down, 
            Vector3Int.left, Vector3Int.right
        };
        
        var (success, neighbours) = TileUtil.TryGetAllRelative(tileLocation, positions);

        foreach (var neighbour in neighbours) {
            var neighbourBackground = neighbour.AsBackground();
            success = neighbour.TryResolve(
                out byte aboveTile
            );
            if (!success) continue;
            success = neighbourBackground.TryResolve(
                out byte backgroundTile
            );
            if (!success) continue;

            if (aboveTile != this.idOfEmptyTile) continue;
            if (backgroundTile != this.indexOfAshTile) continue;

            neighbour.TryPut(
                ShouldGrowWoodInstead()
                    ? (byte) this.indexOfWoodTile
                    : (byte) this.indexOfBushTile
            );
            if (ShouldConsumeAsh()) {
                neighbourBackground.TryPut((byte) this.indexOfDirtTile);
            }
            break;
        }
    }

    System.Random random = new();
    bool ShouldSpread() => this.random.NextDouble() < this.spreadChanceEachRandomTick;

    bool ShouldConsumeAsh() => this.random.NextDouble() < this.ashConsumptionChanceOnSpread;
    bool ShouldGrowWoodInstead() => this.random.NextDouble() < this.woodChance;
}