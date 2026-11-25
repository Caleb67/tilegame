using System.Linq;
using static System.Linq.Enumerable;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class FillTileSelectionMenu : MonoBehaviour
{
    [SerializeField] Transform tileListPanel;
    [SerializeField] GlobalTileInfo globalTileInfo;
    [SerializeField] GlobalItemInfo globalItemInfo;
    
    [SerializeField] GameObject tileEntryPrefab;

    void Start() {
        var tiles = this.globalTileInfo.tiles;
        var validStartIdx = this.globalTileInfo.placeablesStartID;
        var validEndIdx = this.globalTileInfo.placeablesEndID;

        foreach (var tile in tiles) {
            if (tile == null) continue;

            var tileId = tiles.IndexOf(tile);
            if (tileId > validEndIdx || tileId < validStartIdx) {
                continue;
            }

            
            var sprite = this.globalTileInfo.GetSpriteForTile(tileId);
            var name = this.globalTileInfo.tileNames[(byte)tileId];
            var tileEntry = Instantiate(this.tileEntryPrefab, this.tileListPanel);
            
            var image = tileEntry
                    .GetComponentInChildren<TileEntryIcon>()
                    .GetComponent<Image>();

            var tileEntryInfo = tileEntry.GetComponentInChildren<TileEntryName>();
            
            var nameField = tileEntryInfo.GetComponent<TextMeshProUGUI>();
            
            tileEntryInfo.tileId = (byte)tileId;
            image.sprite = sprite;
            nameField.text = name.ToUpper();
        }
    }
}
