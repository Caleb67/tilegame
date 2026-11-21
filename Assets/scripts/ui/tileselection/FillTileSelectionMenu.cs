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

            Debug.Log("test?");
            var idx = tiles.IndexOf(tile);
            if (idx > validEndIdx || idx < validStartIdx) {
                continue;
            }
            Debug.Log("test?");
            
            var sprite = tile.sprite;
            var name = this.globalTileInfo.tileNames[(byte)idx];
            var tileEntry = Instantiate(this.tileEntryPrefab, this.tileListPanel);
            
            var image = tileEntry
                    .GetComponentInChildren<TileEntryIcon>()
                    .GetComponent<Image>();

            var tileEntryInfo = tileEntry.GetComponentInChildren<TileEntryName>();
            
            var nameField = tileEntryInfo.GetComponent<TextMeshProUGUI>();
            
            tileEntryInfo.tileId = (byte)idx;
            image.sprite = sprite;
            nameField.text = name.ToUpper();

            Debug.Log("test?");
        }
    }
}
