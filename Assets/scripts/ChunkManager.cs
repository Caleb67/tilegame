

using static System.Linq.Enumerable;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class ChunkManager : MonoBehaviour {
    [SerializeField] List<TileBehaviour> behavioursToTick;
    [Header("Random Ticks")]
    [SerializeField] int timeBetweenRandomTicksMS;

    static ChunkManager _s_instance;
    public static ChunkManager s_instance {
        get {
            if (_s_instance == null)
            {
                _s_instance = FindObjectOfType<ChunkManager>();
            }
            return _s_instance;
        }
    }

    public ChunkGameObject[] GetAllChunksLoaded() {
        return this.transform.GetComponentsInChildren<ChunkGameObject>();
    }

    public bool TryGetTileLocation(Vector3 worldPos, bool isBackground, out TileLocation location) {
        var chunks = GetAllChunksLoaded();

        foreach (var chunk in chunks) {
            var success = chunk.TryGetPosInChunk(
                worldPos,
                out var posInChunk
            );
            if (!success) continue;
            var realPos = chunk.ChunkPosToWorld(posInChunk);
            location = new TileLocation(chunk, posInChunk, realPos, isBackground);

            return true;
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
        if (_s_instance == null) {
            _s_instance = this;
        }
        StartTileBehaviours();
        StartRandomlyTicking();
    }

    void StartTileBehaviours() {
        foreach (var behaviour in this.behavioursToTick)
        {
            StartCoroutine(behaviour.Ticker());
        }
    }

    void StartRandomlyTicking() => StartCoroutine(RandomlyTick());

    public void CleanTickers() {
        foreach (var behaviour in this.behavioursToTick) {
            StopCoroutine(behaviour.Ticker());
            behaviour.QuitTicking();
        }
    }

    IEnumerator RandomlyTick() {
        while (true) {
            yield return new WaitForSeconds(
                (float) TimeSpan.FromMilliseconds(this.timeBetweenRandomTicksMS)
                                .TotalSeconds
            );
            PerformRandomTick();
        }
    }

    System.Random random = new();
    void PerformRandomTick() {
        var chunks = GetAllChunksLoaded();
        
        var next = chunks.OrderBy(c => this.random.NextDouble())
            .FirstOrDefault();

        if (next != null) next.RandomlyTick();
    }
}