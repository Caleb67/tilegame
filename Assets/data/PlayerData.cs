using System;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [field: SerializeField]
    public uint baseInventoryCapacity {
        get; private set;
    }

    public String savePath = "dev_save";
    public Vector2 playerStartPos = new();
    public int saveSeed = 10;

    [field: Space(10)]
    [field: SerializeField]
    public Inventory inventory {
        get; private set;
    }
}
