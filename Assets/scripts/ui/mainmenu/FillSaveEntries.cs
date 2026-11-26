using UnityEngine;
using System;
using TMPro;
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
            var isSandboxMode = levelData.isSandboxMode;

            var saveEntryName = saveEntry.GetComponentInChildren<SaveEntryName>();
            saveEntryName.savePath = relativeSaveFilePath;
            saveEntryName.saveSeed = seed;
            saveEntryName.playerStartPos = playerStartPos;
            saveEntryName.isSandboxMode = isSandboxMode;

            var saveEntryNameField = saveEntryName.GetComponent<TextMeshProUGUI>();
            
            var saveEntrySeed = saveEntry.GetComponentInChildren<SaveEntrySeed>();
            var saveEntrySeedField = saveEntrySeed.GetComponent<TextMeshProUGUI>();
            Debug.Log(name);
            saveEntryNameField.text = name.Replace("_", " ").ToUpper();

            var sandboxModeIcon = isSandboxMode ? "S. " : "";
            saveEntrySeedField.text = $"{sandboxModeIcon}SEED ({seed})";

        }
    }
}
