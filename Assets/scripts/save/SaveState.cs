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
        this.playerData.Load();
        this.playerData.inventory.LoadFromFile();
    }

    void OnApplicationQuit() {
        FindObjectOfType<ChunkManager>().CleanTickers();
        if (this.saveOnExit) RunSave();
        this.terrainGenerator.Tick();
    }

    void SaveMetadata() {
        var data = LevelData.LoadFromFile(this.playerData.savePath);
        data.saveName = this.playerData.savePath;
        data.saveSeed = this.playerData.saveSeed;
        data.playerPositon = this.player.position;
        data.isSandboxMode = this.playerData.isSandboxMode;
        LevelData.Save(this.playerData.savePath, data);
    }
}