using UnityEngine;

public class SaveState : MonoBehaviour {
    [Header("Debug")]
    [SerializeField] bool saveOnExit;
    [Header("Data")]
    [SerializeField] TerrainGenerator terrainGenerator;
    [SerializeField] PlayerData playerData;
    
    public void RunSave() {
        this.terrainGenerator.SetShouldSave();
        this.playerData.inventory.Save();
    }

    public void Start() {
        this.playerData.inventory.LoadFromFile();
    }

    void OnApplicationQuit() {
        if (this.saveOnExit) RunSave();
        this.terrainGenerator.Tick();
    }
}