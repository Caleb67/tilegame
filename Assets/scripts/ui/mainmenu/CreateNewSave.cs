
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateNewSave : MonoBehaviour {
    [SerializeField] TMP_InputField inputField;

    [SerializeField] uint initialInventoryCapacity;
    [SerializeField] Chunk spawnChunkPrefab;
    public void OnClick() {
        var newSaveName = this.inputField.text;
        if (newSaveName == "" || newSaveName == null) return;

        this.inputField.text = "";

        newSaveName = newSaveName.Replace(" ", "_");
        var random = new System.Random(); 
        var seed = random.Next();

        var newData = new LevelData();
        
        newData.saveName = newSaveName;
        newData.saveSeed = seed;
        newData.inventory.capacityUnits = this.initialInventoryCapacity;
        newData.chunks.Add(new ChunkData(this.spawnChunkPrefab, Vector3Int.zero));
        newData.playerPositon = new Vector2(4f, 4f);

        LevelData.Save(newSaveName, newData);

        var thisSceneId = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(thisSceneId);
    }
}