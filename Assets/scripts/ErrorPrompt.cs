
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ErrorPrompt : MonoBehaviour {
    [SerializeField] TextMeshPro errorText;
    [SerializeField] float durationVisible;
    
    float durationLeft = 0;
    bool showing = false;
    
    public void ShouldShow(String text) {
        this.durationLeft = this.durationVisible;
        this.errorText.text = text;
        this.errorText.gameObject.SetActive(true);
        if (!this.showing) {
            this.showing = true;
            StartCoroutine(WaitToHideError());
        }
    }

    IEnumerator WaitToHideError() {
        while (this.durationLeft > 0) {
            this.durationLeft -= Time.deltaTime;
            yield return null;
        }
        this.showing = false;
        this.errorText.gameObject.SetActive(false);
    }
}