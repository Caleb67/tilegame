using TMPro;
using UnityEngine;

public class InventoryCapacity : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI capacityField;
    [SerializeField] PlayerData playerData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update() {
        var occupied = this.playerData.inventory.occupiedUnits;
        var capacity = this.playerData.inventory.capacityUnits;
        this.capacityField.text =
            $"{occupied}u/{capacity}u";
    }
}
