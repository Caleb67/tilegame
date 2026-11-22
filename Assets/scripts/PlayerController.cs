using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] float landMoveSpeed;
    [SerializeField] float waterMoveSpeed;
    
    [Header("Keybindings")]
    [Space(5)]
    [SerializeField] KeyCode save;
    [SerializeField] KeyCode showInventory;
    [SerializeField] KeyCode showTileSelection;

    [Space(10)]

    [SerializeField] KeyCode placeTile;
    [SerializeField] KeyCode eraseTile;
    [SerializeField] KeyCode pickTile;
    [Space(2)]
    [SerializeField] KeyCode selectPrevTile;
    [SerializeField] KeyCode selectNextTile;
    [SerializeField] KeyCode switchToBackground;
    [SerializeField] KeyCode switchToForeground;


    [Header("Camera")]
    [SerializeField] float zoomSpeed;
    [SerializeField] float maxCameraSize;
    [SerializeField] float minCameraSize;
    [SerializeField] CinemachineCamera camera;
    
    [Header("Data")]
    [SerializeField] UIManager uiManager;
    [SerializeField] PlayerData playerData;

    Rigidbody2D body;
    PlaceTile cursor;
    float curMoveSpeed;

    
    public void SetSwimming(bool value) {
        this.curMoveSpeed = value
            ? this.waterMoveSpeed
            : this.landMoveSpeed;
    }


    void Start() {
        this.curMoveSpeed = this.landMoveSpeed;
        this.transform.position = this.playerData.playerStartPos;
        this.body = GetComponent<Rigidbody2D>();
        this.cursor = FindObjectOfType<PlaceTile>();
    }

    void FixedUpdate() {
        var verticalAxis = Input.GetAxis("Vertical") * Time.deltaTime * this.curMoveSpeed;
        var horizontalAxis = Input.GetAxis("Horizontal") * Time.deltaTime * this.curMoveSpeed;
        
        this.body.linearVelocity = new Vector2(horizontalAxis, verticalAxis);
    }

    void LateUpdate() {
        if (!UIManager.IsMouseOverUI()) {
            CursorAndCameraBehavior();
        }
        MiscBehavior();
    }

    void MiscBehavior() {
        if (Input.GetKeyDown(this.switchToBackground)) this.cursor.isBackground = true;
        if (Input.GetKeyDown(this.switchToForeground)) this.cursor.isBackground = false;

        if (Input.GetKeyDown(this.selectNextTile)) this.cursor.SelectNextTile();
        if (Input.GetKeyDown(this.selectPrevTile)) this.cursor.SelectPrevTile();


        if (Input.GetKeyDown(this.save)) {
            GetComponent<SaveState>().RunSave();
        }
        if (Input.GetKeyDown(this.showInventory)) {
            this.uiManager.SetOrUnsetActiveUI(UIManager.UIType.Inventory);
        }
        if (Input.GetKeyDown(this.showTileSelection)) {
            this.uiManager.SetOrUnsetActiveUI(UIManager.UIType.TileSelection);
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            this.uiManager.SetOrUnsetActiveUI(UIManager.UIType.None);
        }

    }

    void CursorAndCameraBehavior()
    {
        var scroll = Input.mouseScrollDelta;
        var sizeDifference = scroll.y * zoomSpeed;
        
        if (Input.GetKey(this.placeTile)) this.cursor.PutTile();
        if (Input.GetKey(this.eraseTile)) this.cursor.EraseTile();
        if (Input.GetKey(this.pickTile)) this.cursor.PickTile();

        var prevSize = this.camera.Lens.OrthographicSize;
        var newSize = Math.Clamp(prevSize - sizeDifference, this.minCameraSize, this.maxCameraSize);
        this.camera.Lens.OrthographicSize = newSize;
    }

}
