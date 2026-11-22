using TMPro;
using UnityEngine;

public class Coordinates : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] TextMeshProUGUI coordinateDisplay;
    void Update() {
        this.coordinateDisplay.text = 
            $"({(int)player.position.x},{(int)player.position.y})";
    }
}
