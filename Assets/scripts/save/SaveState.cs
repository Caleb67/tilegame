using UnityEngine;

public class SaveState : MonoBehaviour {
    [Header("Debug")]
    [SerializeField] bool saveOnExit;
    [Header("Data")]
    [SerializeField] TerrainGenerator terrainGenerator;
    [SerializeField] Transform player;
    [SerializeField] PlayerData playerData;
    
    public void RunSave() {
        this.terrainGenerator.SetShouldSave();
        this.playerData.inventory.Save();
        SaveMetadata();
    }

    public void Start() {
        this.playerData.inventory.LoadFromFile();
    }

    void OnApplicationQuit() {
        if (this.saveOnExit) RunSave();
        this.terrainGenerator.Tick();
    }

    void SaveMetadata() {
        var data = LevelData.LoadFromFile(this.playerData.savePath);
        data.saveName = this.playerData.savePath;
        data.saveSeed = this.playerData.saveSeed;
        data.playerPositon = this.player.position;
        LevelData.Save(this.playerData.savePath, data);
    }
}