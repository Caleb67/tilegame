using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Inventory {

    [field: SerializeField]
    public uint capacityUnits {
        get; set;
    }

    [field: SerializeField]
    public uint occupiedUnits {
        get; private set;
    }
    public List<Item> contents = new();

    [Header("Data")]
    [SerializeField] PlayerData playerData;

    public bool CanFit(uint units) {
        return 
        this.occupiedUnits + units
                <= 
        this.capacityUnits;
    }

    public bool CanFit(Item item, GlobalItemInfo.ItemDefinition definition) {
        var totalUnits = definition.sizeUnits * item.count;
        return CanFit(totalUnits);
    }
    
    public bool TryPut(Item item, GlobalItemInfo.ItemDefinition definition) {
        var totalUnits = definition.sizeUnits * item.count;

        if (!CanFit(totalUnits)) return false;

        AddItem(item);
        this.occupiedUnits += totalUnits;

        return true;
    }

    public bool TryTake(Item item, GlobalItemInfo.ItemDefinition definition) {
        var idx = this.contents.FindIndex(it => it.id == item.id);
        if (idx == -1) return false;
        
        var storedItem = this.contents[idx];

        if (item.count > storedItem.count) return false;

        this.occupiedUnits -= item.count * definition.sizeUnits;
        storedItem.count -= item.count;

        if (storedItem.count == 0) this.contents.RemoveAt(idx);
        else this.contents[idx] = storedItem;
        
        return true;
    }
    
    private void AddItem(Item item) {
        var idx = this.contents.FindIndex(it => it.id == item.id);
        if (idx == -1) {
            this.contents.Add(new Item{id = item.id, count = 0});
            idx = this.contents.Count - 1;
        }
        var storedItem = this.contents[idx];
        storedItem.count += item.count;
        this.contents[idx] = storedItem;
    }

    public Inventory Clone() {
        var newInventory = new Inventory
        {
            capacityUnits = this.capacityUnits,
            occupiedUnits = this.occupiedUnits,
            contents = new List<Item>(this.contents)
        };
        return newInventory;
    }

    public void ReplaceWith(Inventory other) {
        this.contents = other.contents;
        this.capacityUnits = other.capacityUnits;
        this.occupiedUnits = other.occupiedUnits;
    }

    public void Save() {
        var data = LevelData.LoadFromFile(this.playerData.saveName);
        data.inventory = this;
        LevelData.Save(this.playerData.saveName, data);
    }

    public void LoadFromFile() {
        var data = LevelData.LoadFromFile(this.playerData.saveName);
        ReplaceWith(data.inventory);
        if (this.capacityUnits == 0) {
            this.capacityUnits = this.playerData.baseInventoryCapacity;    
        }
    }
}
