
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ShowResourcesOnHover : MonoBehaviour {
    [SerializeField] HoverPanel hoverPanel;

    PointerEventData pointerEventData;
    void Start() {
        this.pointerEventData = new PointerEventData(EventSystem.current);
    }
    void Update() {
        this.pointerEventData.position = Mouse.current.position.ReadValue();
        var isOnEntry = CheckForEntryHover();
        this.hoverPanel.gameObject.SetActive(isOnEntry);
    }

    bool CheckForEntryHover() {
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(this.pointerEventData, raycastResults);
        foreach (var result in raycastResults) {
            var tag = result.gameObject.GetComponent<TileEntryTag>();
            if (tag == null) continue;

            var entryInfo = result.gameObject.GetComponentInChildren<TileEntryName>();
            
            var tileId = entryInfo.tileId;
            this.hoverPanel.SetResourcesFor(tileId);
            return true;
        }
        return false;
    }
}