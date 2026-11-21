
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
    
    [Serializable]
    public enum UIType {
        None = 0,
        Inventory = 1,
        TileSelection = 2
    }

    [DictionaryDisplay(keyLabel = "UI", valueLabel = "Path")]
    [SerializeField] Dictionary<UIType, String> uiPaths;
    

    UIType currentUi = UIType.None;
    UIType nextUi = UIType.None;
    bool wasChanged = false;
    
    public void SetActiveUI(UIType type) {
        this.wasChanged = true;
        this.nextUi = type;
    }

    public static bool IsMouseOverUI() {
        if (EventSystem.current == null) return false;
        else return EventSystem.current.IsPointerOverGameObject();
    }

    public void SetOrUnsetActiveUI(UIType type) {
        this.wasChanged = true;

        if (this.currentUi == type) this.nextUi = UIType.None;
        else this.nextUi = type;
    }

    void Update() {
        if (!this.wasChanged) return;
        this.wasChanged = false;
        
        UnloadPreviousUI(this.currentUi);
        var nextScenePath = ScenePathFromUIType(this.nextUi);
        
        this.currentUi = this.nextUi;
        this.nextUi = UIType.None;

        if (nextScenePath == "") return;
        SceneManager.LoadSceneAsync(nextScenePath, LoadSceneMode.Additive);
    }

    void UnloadPreviousUI(UIType previous) {
        if (previous == UIType.None) return;
        SceneManager.UnloadSceneAsync(this.uiPaths[previous], UnloadSceneOptions.None);
    }

    String ScenePathFromUIType(UIType type) {
        return type switch {
            UIType.None => "",
            _ => this.uiPaths[type]
        };
    }
}