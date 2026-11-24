using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateNewSave : MonoBehaviour {
    [SerializeField] TMP_InputField saveNameInputField;
    [SerializeField] TMP_InputField seedInputField;
    [SerializeField] Toggle sandboxModeToggle;

    [SerializeField] uint initialInventoryCapacity;
    [SerializeField] Chunk spawnChunkPrefab;
    public void OnClick() {
        var newSaveName = this.saveNameInputField.text;
        if (string.IsNullOrEmpty(newSaveName)) return;
        if (Files.ExistsSave(newSaveName)) return;
        
        var seed = ChooseSeed();

        var newData = new LevelData();
        
        newData.saveName = newSaveName;
        newData.saveSeed = seed;
        newData.inventory.capacityUnits = this.initialInventoryCapacity;
        newData.chunks.Add(new ChunkData(this.spawnChunkPrefab, Vector3Int.zero));
        newData.playerPositon = new Vector2(4f, 4f);
        newData.isSandboxMode = this.sandboxModeToggle.isOn;

        LevelData.Save(newSaveName, newData);

        this.saveNameInputField.text = "";
        this.seedInputField.text = "";

        var thisSceneId = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(thisSceneId);
    }

    int ChooseSeed() {
        var chosenSeed = this.seedInputField.text;
        if (string.IsNullOrEmpty(chosenSeed)) {
            return new System.Random().Next();
        }
        else return int.Parse(chosenSeed);
    }
}