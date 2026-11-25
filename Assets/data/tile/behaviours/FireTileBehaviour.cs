
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "FireTileBehaviour", menuName = "Tile Behaviours/FireTileBehaviour")]
public class FireTileBehaviour : TileBehaviour
{
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
        if (location.Resolve() == null) return;

        var tile = location.Resolve();
        var tileId = GlobalTileInfo.s_instance.tiles.IndexOf(tile);
        if (tileId == -1) return;

        if (this.idsOfBurnableTiles.Contains(tileId)) {
            location.TryPut(GlobalTileInfo.s_instance.tiles[this.idOfAshTile]);
        }
    }

    void TickFire(TileLocation tileLocation, ChunkManager chunkManager) {
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
            if (!success) break;
            
            success = chunkManager.TryGetTileLocation(
                tileLocation.positionInWorld + relativePos, 
                isBackground:false,
                out var aboveLocation
            );
            if (!success) break;

            if (aboveLocation.Resolve() != null) continue;
            if (location.Resolve() == null) continue;

            var tile = location.Resolve();
            var tileId = GlobalTileInfo.s_instance.tiles.IndexOf(tile);
            if (tileId == -1) break;
            
            if (this.idsOfBurnableTiles.Contains(tileId)) {
                aboveLocation.TryPut(GlobalTileInfo.s_instance.tiles[this.idOfFireTile]);
                location.TryPut(GlobalTileInfo.s_instance.tiles[this.idOfAshTile]);
            }
        }
        if (this.random.NextDouble() < this.extinguishChance) {
            tileLocation.TryPut(null);    
        }
    }

    public override IEnumerator Ticker() {
        while (true) {
            this.activeFires.RemoveAll(
                it => it.Resolve() == null
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
            fireLocation.TryPut(null);
        }
    }
}