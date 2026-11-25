using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlaceTile : MonoBehaviour
{
    
    [SerializeField] PlayerData playerData;

    [Header("UI")]
    [SerializeField] SpriteRenderer tilePreview;
    [SerializeField] ErrorPrompt errorPrompt;

    [TextArea(3, 3)]
    [SerializeField] String notEnoughResources;
    [TextArea(3, 3)]
    [SerializeField] String notEnoughInventorySpace;
    [TextArea(3, 3)]
    [SerializeField] String cannotPlaceInBackground;


    [Header("Tile Info")]
    [SerializeField] GlobalTileInfo globalTileInfo;
    [SerializeField] GlobalItemInfo globalItemInfo;

    public bool isBackground = false;
    TileLocation lastPlacedLocation = default;
    int lastPlacedTileId = -1;

    int index;
    ChunkManager chunkManager;

    public void SelectPrevTile() {
        var placeableStartID = this.globalTileInfo.placeablesStartID;
        var placeableEndID = this.globalTileInfo.placeablesEndID;
        if (this.index <= placeableStartID) this.index = placeableEndID;
        else this.index -= 1;
    }
    public void SelectNextTile() {
        var placeableStartID = this.globalTileInfo.placeablesStartID;
        var placeableEndID = this.globalTileInfo.placeablesEndID;
        if (this.index >= placeableEndID) this.index = placeableStartID;
        else this.index += 1;
    }

    public void SelectTile(int tileId) {
        var placeableStartID = this.globalTileInfo.placeablesStartID;
        var placeableEndID = this.globalTileInfo.placeablesEndID;
        if (
            tileId < placeableStartID || 
            tileId > placeableEndID
        ) {
            throw new ArgumentException($"not a tile that can be placed, id:{tileId}");
        }
        else this.index = tileId;
    }

    public void PutTile() {
        var mouseWorldPos = GetMouseWorld();
        var chunks = this.chunkManager.transform.GetComponentsInChildren<Chunk>();
        var tileToPut = this.globalTileInfo.tiles[this.index];
        foreach (var chunk in chunks) {
            if (!chunk.IsInChunk(mouseWorldPos)) continue;
            var originalTile = chunk.GetTile(mouseWorldPos, this.isBackground);
            
            var testInventory = this.playerData.inventory.Clone();

            if (!AttemptSpendResourcesForTile(testInventory, this.index)) return;
            if (!AttemptObtainResourcesFromTile(testInventory, originalTile)) return;
            
            if (
                this.globalTileInfo.foregroundExclusiveTiles.Contains(this.index)
                && this.isBackground
            ) {
                goto CANNOT_PLACE_IN_BACKGROUND;
            }

            if (!_TryPutTile(chunk, mouseWorldPos, tileToPut, this.isBackground, propogatesUpdates:true)) return;
            
            this.playerData.inventory.ReplaceWith(testInventory);
        }
        return;
    CANNOT_PLACE_IN_BACKGROUND: 
        this.errorPrompt.ShouldShow(this.cannotPlaceInBackground);
        return;
    }

    public void EraseTile() {
        var mouseWorldPos = GetMouseWorld();
        var chunks = this.chunkManager.transform.GetComponentsInChildren<Chunk>();
        foreach (var chunk in chunks) {
            if (!chunk.IsInChunk(mouseWorldPos)) continue;
            var tileAtPos = chunk.GetTile(mouseWorldPos, this.isBackground);
            var testInventory = this.playerData.inventory.Clone();
            if (!AttemptObtainResourcesFromTile(testInventory, tileAtPos)) return;
            if (!chunk.TryPutTile(mouseWorldPos, null, this.isBackground, propogatesUpdates:true)) return;
            this.lastPlacedLocation = default;
            this.playerData.inventory.ReplaceWith(testInventory);   
        }
    }

    public void PickTile() {
        var mouseWorldPos = GetMouseWorld();
        var chunks = this.chunkManager.transform.GetComponentsInChildren<Chunk>();

        var tiles = chunks.Select(
            chunk => chunk.GetTile(mouseWorldPos, this.isBackground)
        );
        var tileAtPos = tiles.FirstOrDefault(it => it != null);
        if (tileAtPos == null) return;
        
        var idx = this.globalTileInfo.tiles.IndexOf(tileAtPos);
        if (idx == -1) return;
        
        try {
            SelectTile(idx);
        } catch (ArgumentException) {
            return;
        }
    }

    void Start() {
        this.chunkManager = FindObjectOfType<ChunkManager>();
        this.index = this.globalTileInfo.placeablesStartID;
    }

    void Update() {
        this.transform.position = GetMouseWorld();
        
        this.tilePreview.sprite = this.globalTileInfo.GetSpriteForTile(this.index);

        var canShowPreview = !UIManager.IsMouseOverUI();
        this.tilePreview.gameObject.SetActive(canShowPreview);
    }

    bool AttemptSpendResourcesForTile(Inventory testInventory, int tileToPlaceIdx) {
        if (this.playerData.isSandboxMode) return true;

        var TILE_ITEM_CONTENTS = this.globalItemInfo.tileItemContents;
        
        var newTileIdx = tileToPlaceIdx;
        var newTileRequirements = TILE_ITEM_CONTENTS.GetValueOrDefault(
            newTileIdx,
            GlobalItemInfo.TileItemContents.NONE
        );

        foreach (var item in newTileRequirements.items) {
            var definition = this.globalItemInfo.itemDefinitions[item.id];
            if (!testInventory.TryTake(item, definition)) goto not_enough_resources;
        }
        return true;
    
    not_enough_resources:
        this.errorPrompt.ShouldShow(this.notEnoughResources);
        return false;
    }

    bool AttemptObtainResourcesFromTile(Inventory testInventory, TileBase tileToRemove) {
        if (this.playerData.isSandboxMode) return true;
        
        var TILE_ITEM_CONTENTS = this.globalItemInfo.tileItemContents;

        var originalTileIdx = this.globalTileInfo.tiles.IndexOf(tileToRemove);
        if (originalTileIdx == -1) return false;

        var originalTileContents = TILE_ITEM_CONTENTS.GetValueOrDefault(
            originalTileIdx,
            GlobalItemInfo.TileItemContents.NONE
        );
        foreach (var item in originalTileContents.items) {
            var definition = this.globalItemInfo.itemDefinitions[item.id];
            if (!testInventory.TryPut(item, definition)) goto not_enough_inventory_space;
        }

        return true;
    
    not_enough_inventory_space:
        this.errorPrompt.ShouldShow(this.notEnoughInventorySpace);
        return false;
    }

    Vector3 GetMouseWorld() {
        var mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = 0 - Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }

    bool _TryPutTile(
        Chunk chunk,
        Vector3 worldPos, TileBase toPut, bool isBackground, bool propogatesUpdates = false
    ) {
        var success = chunk.TryGetTileLocation(
            worldPos, 
            isBackground, 
            out var location
        );
        if (!success) return false;
        if (
            location.IsSameLocation(this.lastPlacedLocation) &&
            this.index == this.lastPlacedTileId
        ) return false;
        else {
            this.lastPlacedLocation = location;
            this.lastPlacedTileId = this.index;
            return chunk.TryPutTile(worldPos, toPut, isBackground, propogatesUpdates);
        }
    }
}
