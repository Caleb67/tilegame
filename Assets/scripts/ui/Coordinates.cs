using TMPro;
using UnityEngine;

public class Coordinates : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] TextMeshProUGUI coordinateDisplay;
    void Update() {
        this.coordinateDisplay.text = 
            $"({(int)this.player.position.x},{(int)this.player.position.y})";
    }
}
