using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

using EntryList = System.Collections.Generic.IEnumerable<
    System.Tuple<
        UnityEngine.Transform, 
        TMPro.TextMeshProUGUI, 
        TMPro.TextMeshProUGUI
    >
>;

public class InventoryItemList : MonoBehaviour
{
    [SerializeField] Transform itemListPanel;

    [Header("Item Entry")]
    [SerializeField] GameObject itemEntryPrefab;
    [SerializeField] TMP_SpriteAsset spriteAssets;
    [SerializeField] TMP_FontAsset fontAsset;

    [Header("Data")]
    [SerializeField] PlayerData playerData;
    [SerializeField] GlobalItemInfo globalItemInfo;


    void Update() {
        // for each item in the player's inventory (sorted alphabetically)
        // set the itemList to show the items

        var childrenEntries = from Transform transform in this.itemListPanel
                            select transform;

        var numItemEntries = this.playerData.inventory.contents.Count();
        
        var newEntries = GetResized(childrenEntries, numItemEntries);
        RebuildEntries(childrenEntries, newEntries);

        var entries = from Transform transform in this.itemListPanel
                select Tuple.Create(
                    transform,
                    transform.GetComponentInChildren<ItemEntryName>().GetComponent<TextMeshProUGUI>(),
                    transform.GetComponentInChildren<ItemEntryCount>().GetComponent<TextMeshProUGUI>()
                );
        FillEntries(entries);
    }

    List<Transform> GetResized(IEnumerable<Transform> entries, int to) { 
        var newEntries = new List<Transform>();
        
        while (newEntries.Count < to) {
            var idx = newEntries.Count;
            var entry = entries.ElementAtOrDefault(idx);
            newEntries.Add(entry);
        }

        return newEntries;
    }

    void RebuildEntries(IEnumerable<Transform> entries, IEnumerable<Transform> expectedEntries) { 
        foreach (var entry in entries) {
            if (!expectedEntries.Contains(entry)) {
                Destroy(entry.gameObject);
            }
        }

        while (this.itemListPanel.childCount < expectedEntries.Count()) {
            Instantiate(this.itemEntryPrefab, this.itemListPanel);
        }
    }

    
    void FillEntries(EntryList entries) {
        var sortedItems = this.playerData.inventory.contents
            .OrderBy(item => this.globalItemInfo.itemDefinitions[item.id].name);

        var entriesToItems = entries.Zip(
            sortedItems,
            (entry, item) => new {entry, item}
        );
        
        foreach(var entryToItem in entriesToItems) {
            var entry = entryToItem.entry;
            var item = entryToItem.item;
            var itemId = item.id;

            var itemName = this.globalItemInfo.itemDefinitions[itemId].name;
            var itemUnitSize = this.globalItemInfo.itemDefinitions[itemId].sizeUnits;
            var count = item.count;
        
            entry.Item2/*name*/.text = $"{count}x {itemName.ToUpper()}";
            entry.Item2/*name*/.font = this.fontAsset;
            entry.Item2/*name*/.spriteAsset = this.spriteAssets;

            entry.Item3/*units*/.text = $"{count*itemUnitSize}u";
            entry.Item3/*units*/.font = this.fontAsset;
            entry.Item3/*units*/.spriteAsset = this.spriteAssets;
        }
    }
}
