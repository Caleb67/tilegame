

using static System.Linq.Enumerable;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class ChunkManager : MonoBehaviour {
    [SerializeField] List<TileBehaviour> behavioursToTick;
    public static ChunkManager s_instance {
        get; private set;
    }

    public Chunk[] GetAllChunksLoaded() {
        return this.transform.GetComponentsInChildren<Chunk>();
    }

    public bool TryGetTileLocation(Vector3 worldPos, bool isBackground, out TileLocation location) {
        var chunks = GetAllChunksLoaded();

        foreach (var chunk in chunks) {
            if (chunk.TryGetTileLocation(worldPos, isBackground, out location)) {
                //Debug.Log($"found location: posInChunk:{location.positionInChunk},posInWorld:{location.positionInWorld}");
                return true;
            }
        }

        location = default;
        return false;
    }

    public bool TryGetTileLocations(Vector3 worldPosTopLeft, int width, int height, bool isBackground, out TileLocation[] locations) {
        locations = default;
        
        var outLocations = new List<TileLocation>();
        var worldPositions = 
                from x in Range(0, width)
                from y in Range(0, height)
                select new Vector3(x, y, 0) + worldPosTopLeft;
        
        foreach (var pos in worldPositions) {
            var success = TryGetTileLocation(
                pos, isBackground, 
                out TileLocation location
            );
            if(!success) return false;
            outLocations.Add(location);
        }
        
        locations = outLocations.ToArray();
        return true;
    }

    void Awake() {
        if (s_instance == null) {
            s_instance = this;
        }
        StartTileBehaviours();
    }

    void StartTileBehaviours() {
        foreach (var behaviour in this.behavioursToTick)
        {
            StartCoroutine(behaviour.Ticker());
        }
    }

    public void CleanTickers() {
        foreach (var behaviour in this.behavioursToTick) {
            StopCoroutine(behaviour.Ticker());
            behaviour.QuitTicking();
        }
    }
}