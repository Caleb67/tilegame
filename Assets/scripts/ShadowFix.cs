using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class ShadowFix : MonoBehaviour
{
    Tilemap tilemap;
    ShadowCaster2D shadowCaster;

    void Start() {
        this.tilemap = GetComponent<Tilemap>();
        this.shadowCaster = GetComponent<ShadowCaster2D>();
    }

    void Update() {
        var numTiles = this.tilemap.GetTilesRangeCount(
            new Vector3Int(0, 0),
            new Vector3Int(Chunk.CHUNK_SIZE - 1, Chunk.CHUNK_SIZE - 1)
        );
        if (numTiles == 0) this.shadowCaster.castsShadows = false;
        else this.shadowCaster.castsShadows = true;
    }
}
