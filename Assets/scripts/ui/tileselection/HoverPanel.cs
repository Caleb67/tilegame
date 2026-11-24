
using System.Text;
using TMPro;
using UnityEngine;

public class HoverPanel : MonoBehaviour {
    [SerializeField] TextMeshProUGUI resourcesList;
    [SerializeField] GlobalItemInfo globalItemInfo;

    public void SetResourcesFor(int tileId) {
        if (tileId == 0)
        {
            return;
        }
        var contents = this.globalItemInfo.tileItemContents[tileId];
        var list = new StringBuilder();
        foreach (var item in contents.items) {
            var itemName = this.globalItemInfo.itemDefinitions[item.id].name;
            itemName = itemName.ToUpper();
            var itemCount = item.count;
            list.AppendLine($"{itemCount}x {itemName}");
        }
        this.resourcesList.text = list.ToString();
    }
}