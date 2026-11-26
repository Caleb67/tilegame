
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "FireTileBehaviour", menuName = "Tile Behaviours/FireTileBehaviour")]
public class FireTileBehaviour : TileBehaviour
{
    [SerializeField] int idOfEmptyTile;
    [SerializeField] int idOfAshTile;
    [SerializeField] int idOfFireTile;
    [SerializeField] List<int> idsOfBurnableTiles = new();
    [SerializeField] float fireTickDelay;

    [Range(0f, 1f)]
    [SerializeField] float extinguishChance;

    List<TileLocation> activeFires = new();

    readonly System.Random random = new();
    public override void OnPlaced(TileLocation tileLocation, ChunkManager chunkManager) {
        this.activeFires.Add(tileLocation);

        var success = chunkManager.TryGetTileLocation(
            tileLocation.positionInWorld, 
            isBackground:true,
            out var location
        );
        if (!success) return;

        success = location.TryResolve(
            out byte backgroundTile
        );
        if (!success) return;

        if (this.idsOfBurnableTiles.Contains(backgroundTile)) {
            location.TryPut((byte)this.idOfAshTile);
        }
    }
    public override void OnDestroyed(TileLocation tileLocation, ChunkManager chunkManager) {
        Debug.Log("test?");
        this.activeFires.RemoveAll(loc => tileLocation.IsSameLocation(loc));
    }
    void TickFire(TileLocation tileLocation, ChunkManager chunkManager) {
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
            if (this.idsOfBurnableTiles.Contains(backgroundTile)) {
                neighbour.TryPut((byte)this.idOfFireTile);
                neighbourBackground.TryPut((byte)this.idOfAshTile);
            }
        }
        
        if (this.random.NextDouble() < this.extinguishChance) {
            tileLocation.TryPut((byte)this.idOfEmptyTile);
        }
    }

    public override IEnumerator Ticker() {
        while (true) {
            this.activeFires.RemoveAll(
                it => it.TryResolve(out var _) == false
            );
            Debug.Log("fire ticked");

            var toTick = this.activeFires.ToList();
            foreach (var fireLocation in toTick) {
                TickFire(
                    fireLocation,
                    ChunkManager.s_instance
                );
            }

            yield return new WaitForSeconds(this.fireTickDelay);
        }
    }

    public override void QuitTicking()
    {
        var toTick = this.activeFires.ToList();
        foreach (var fireLocation in toTick) {
            fireLocation.TryPut((byte)this.idOfEmptyTile);
        }
    }
}