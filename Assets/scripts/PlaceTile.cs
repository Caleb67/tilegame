using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlaceTile : MonoBehaviour
{
    
    [SerializeField] PlayerData playerData;

    [Header("UI")]
    [SerializeField] SpriteRenderer tilePreview;
    [SerializeField] SpriteRenderer tilePreviewIndicator;
    [Space(5)]
    [SerializeField] Color backgroundIndicatorColor;
    [SerializeField] Color foregroundIndicatorColor;

    [Space(10)]
    [SerializeField] ErrorPrompt errorPrompt;

    [TextArea(3, 3)]
    [SerializeField] String notEnoughResources;
    [TextArea(3, 3)]
    [SerializeField] String notEnoughInventorySpace;
    [TextArea(3, 3)]
    [SerializeField] String cannotPlaceInBackground;
    [TextArea(3, 3)]
    [SerializeField] String cannotPlaceInForeground;


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
        var chunks = ChunkManager.s_instance.GetAllChunksLoaded();
        var tileToPut = this.index;
        foreach (var chunk in chunks) {
            if (!chunk.TryGetPosInChunk(mouseWorldPos, out var posInChunk)) continue;
            var originalTile = chunk.chunk.GetTile(posInChunk, this.isBackground);
            
            var testInventory = this.playerData.inventory.Clone();

            if (!AttemptSpendResourcesForTile(testInventory, this.index)) return;
            if (!AttemptObtainResourcesFromTile(testInventory, originalTile)) return;
            
            if (
                this.globalTileInfo.foregroundExclusiveTiles.Contains(this.index)
                && this.isBackground
            ) {
                goto CANNOT_PLACE_IN_BACKGROUND;
            }
            if (
                this.globalTileInfo.backgroundExclusiveTiles.Contains(this.index)
                && !this.isBackground
            ) {
                goto CANNOT_PLACE_IN_FOREGROUND;
            }

            if (!_TryPutTile(chunk, mouseWorldPos, (byte)tileToPut, this.isBackground, updateNeighbours:true)) return;
            
            this.playerData.inventory.ReplaceWith(testInventory);
        }
        return;
    CANNOT_PLACE_IN_BACKGROUND: 
        this.errorPrompt.ShouldShow(this.cannotPlaceInBackground);
        return;
    CANNOT_PLACE_IN_FOREGROUND:
        this.errorPrompt.ShouldShow(this.cannotPlaceInForeground);
        return;
    }

    public void EraseTile() {
        var mouseWorldPos = GetMouseWorld();
        var chunks = ChunkManager.s_instance.GetAllChunksLoaded();
        foreach (var chunk in chunks) {
            if (!chunk.TryGetPosInChunk(mouseWorldPos, out var posInChunk)) continue;
            var tileAtPos = chunk.chunk.GetTile(posInChunk, this.isBackground);
            var testInventory = this.playerData.inventory.Clone();

            if (!AttemptObtainResourcesFromTile(testInventory, tileAtPos)) return;

            var location = new TileLocation(chunk, posInChunk, mouseWorldPos, this.isBackground);

            var tileToPut = this.isBackground
                ? GlobalTileInfo.s_instance.backgroundEmptyID
                : GlobalTileInfo.s_instance.foregroundEmptyID;
            
            chunk.chunk.PutTile(location, tileToPut, this.isBackground, updateNeighbours:true);

            this.lastPlacedLocation = default;

            this.playerData.inventory.ReplaceWith(testInventory);   
        }
    }

    public void PickTile() {
        var mouseWorldPos = GetMouseWorld();
        var chunks = ChunkManager.s_instance.GetAllChunksLoaded();

        var tiles = chunks.Select(
            chunk => chunk.TryGetPosInChunk(mouseWorldPos, out var posInChunk)
                ? new {success = true, tile = chunk.chunk.GetTile(posInChunk, this.isBackground)}
                : new {success = false, tile = (byte)0}
        );
        var tileInfoAtPos = tiles.FirstOrDefault(it => it.success == true);
        if (tileInfoAtPos == null) return;
        var tileAtPos = tileInfoAtPos.tile;
        
        try {
            SelectTile(tileAtPos);
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
        SetPreviewIndicator();
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

    bool AttemptObtainResourcesFromTile(Inventory testInventory, byte tileToRemove) {
        if (this.playerData.isSandboxMode) return true;
        
        var TILE_ITEM_CONTENTS = this.globalItemInfo.tileItemContents;

        var originalTileContents = TILE_ITEM_CONTENTS.GetValueOrDefault(
            tileToRemove,
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
        ChunkGameObject chunk,
        Vector3 worldPos, byte tileToPut, bool isBackground, bool updateNeighbours = false
    ) {
        var success = chunk.TryGetPosInChunk(
            worldPos,
            out var posInChunk
        );
        if (!success) return false;

        var location = new TileLocation(chunk,posInChunk,worldPos,isBackground);
        if (
            location.IsSameLocation(this.lastPlacedLocation) &&
            this.index == this.lastPlacedTileId
        ) return false;
        else {
            this.lastPlacedLocation = location;
            this.lastPlacedTileId = this.index;
            chunk.chunk.PutTile(location, tileToPut, isBackground, updateNeighbours);
            return true;
        }
    }

    void SetPreviewIndicator() {
        this.tilePreviewIndicator.color = 
            this.isBackground
            ? this.backgroundIndicatorColor
            : this.foregroundIndicatorColor;
    }
}
