using UnityEngine;

public class TileSelection : MonoBehaviour {
    UIManager uiManager;
    PlaceTile cursor;

    TileEntryName tileEntryInfo;

    void Start() {
        this.uiManager = FindObjectOfType<UIManager>();
        this.cursor = FindObjectOfType<PlaceTile>();
        this.tileEntryInfo = GetComponentInChildren<TileEntryName>();
    }

    public void OnClick() {
        this.cursor.SelectTile(this.tileEntryInfo.tileId);
        this.uiManager.SetActiveUI(UIManager.UIType.None);
    }
}