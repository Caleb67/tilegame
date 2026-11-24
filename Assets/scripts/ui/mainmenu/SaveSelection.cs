
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSelection : MonoBehaviour
{
    [SerializeField] PlayerData playerData;
    
    public const int WORLD_SCENE_ID = 1;

    public SaveEntryName saveEntryInfo;

    public void Start() {
        this.saveEntryInfo = GetComponentInChildren<SaveEntryName>();
    }
    public void OnClick() {
        this.playerData.savePath = this.saveEntryInfo.savePath;
        this.playerData.saveSeed = this.saveEntryInfo.saveSeed;
        this.playerData.playerStartPos = this.saveEntryInfo.playerStartPos;
        this.playerData.isSandboxMode = this.saveEntryInfo.isSandboxMode;
        SceneManager.LoadScene(WORLD_SCENE_ID);
    }
}