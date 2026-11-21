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

    public String saveName = "dev_save";

    [field: Space(10)]
    [field: SerializeField]
    public Inventory inventory {
        get; private set;
    }

    
}
