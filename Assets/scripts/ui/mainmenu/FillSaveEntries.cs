using System.Linq;
using static System.Linq.Enumerable;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class FillSaveEntries : MonoBehaviour
{
    [SerializeField] Transform saveListPanel;
    
    [SerializeField] GameObject saveEntryPrefab;

    void Start() {
        var saves = Files.GetAllSaves();

        foreach (var (levelData, relativeSaveFilePath) in saves) {
            if (levelData == null) continue;

            var name = levelData.saveName;
            var seed = levelData.saveSeed;
            var playerStartPos = levelData.playerPositon;
            var saveEntry = Instantiate(this.saveEntryPrefab, this.saveListPanel);

            var saveEntryName = saveEntry.GetComponentInChildren<SaveEntryName>();
            saveEntryName.savePath = relativeSaveFilePath;
            saveEntryName.saveSeed = seed;
            saveEntryName.playerStartPos = playerStartPos;

            var saveEntryNameField = saveEntryName.GetComponent<TextMeshProUGUI>();
            
            var saveEntrySeed = saveEntry.GetComponentInChildren<SaveEntrySeed>();
            var saveEntrySeedField = saveEntrySeed.GetComponent<TextMeshProUGUI>();
            Debug.Log(name);
            saveEntryNameField.text = name.Replace("_", " ").ToUpper();
            saveEntrySeedField.text = $"SEED ({seed})";

        }
    }
}
