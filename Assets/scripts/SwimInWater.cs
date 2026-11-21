using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

public class SwimInWater : MonoBehaviour
{
    [SerializeField] ChunkManager chunkManager;
    [SerializeField] GlobalTileInfo globalTileInfo;

    [SerializeField] SpriteRenderer waterFoam;
    [SerializeField] PlayerController playerController;

    [SerializeField] SpriteMask waterEffectsMaskPrefab;

    SpriteMask[] spriteMasks = new SpriteMask[9];

    void Start() {
        foreach (var idx in Enumerable.Range(0, 9)) {
            var mask = Instantiate(this.waterEffectsMaskPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            mask.maskSource = SpriteMask.MaskSource.Sprite;
            mask.gameObject.SetActive(false);
            spriteMasks[idx] = mask;
        }
    }

    void Update() {
        var isInWater = IsInWater();
        this.waterFoam.enabled = isInWater;
        this.playerController.SetSwimming(isInWater);
        
        MaskLand();
    }

    bool IsInWater() {
        var chunks = this.chunkManager.transform.GetComponentsInChildren<Chunk>();
        var success = this.chunkManager.TryGetTileLocation(
            this.transform.position,
            isBackground:true,
            out var playerTilePosition
        );
        if (!success) {
            Debug.Log("not currently standing on a chunk?");
            return false;
        }

        var tileUnder = playerTilePosition.Resolve();

        var tileIdx = this.globalTileInfo.tiles.IndexOf(tileUnder);
        //Debug.Log($"tileIdx={tileIdx}, tileUnder={tileUnder}");
        if (tileIdx == this.globalTileInfo.backgroundEmptyID) {
            
            return true;
        }
        else return false;
    }

    void MaskLand() {
        var locations = GetAllTilesAround();
        
        var idx = 0;
        while (idx < locations.Count()) {
            var location = locations[idx];
            var tileAt = location.Resolve();
            var tileIdx = this.globalTileInfo.tiles.IndexOf(tileAt);
            var mask = this.spriteMasks[idx];

            mask.transform.position = location.positionInWorld;
            mask.transform.position += new Vector3(0.5f, 0.5f);
            mask.sprite = tileAt.sprite;

            if (tileIdx == this.globalTileInfo.backgroundEmptyID) {
                mask.gameObject.SetActive(false);
            }
            else mask.gameObject.SetActive(true);
            
            idx += 1;
        }
    }
    TileLocation[] GetAllTilesAround() {
        var success = this.chunkManager.TryGetTileLocations(
            this.transform.position + new Vector3(-1, -1),
            width:3, height:3,
            isBackground:true,
            out var positionsAroundPlayer
        );
        if (!success) return new TileLocation[0];
        else return positionsAroundPlayer;
    }
}
