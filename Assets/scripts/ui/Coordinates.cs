using TMPro;
using UnityEngine;

public class Coordinates : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] TextMeshProUGUI coordinateDisplay;

    public const float TILE_POS_OFFSET = 0.5F; 
    void Update() {
        this.coordinateDisplay.text = 
            $"({(int)(this.player.position.x + TILE_POS_OFFSET)},{(int)(this.player.position.y + TILE_POS_OFFSET)})";
    }
}
