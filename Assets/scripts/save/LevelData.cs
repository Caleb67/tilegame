
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class LevelData {
    public List<ChunkData> chunks = new();
    public Inventory inventory;

    public static LevelData LoadFromFile(String saveName) {
        var filePath = Path.Combine(Files.savesPath, $"{saveName}.json");
        if (!File.Exists(filePath)) {
            Directory.CreateDirectory(Files.savesPath);
            using var newSaveFile = File.Create(filePath);
            using var writer = new StreamWriter(newSaveFile, Encoding.UTF8);
            writer.Write("{}");
        }
        var json = File.ReadAllText(filePath);
        
        return JsonUtility.FromJson<LevelData>(json);
    }

    public static void Save(String saveName, LevelData data) {
        var jsonOut = JsonUtility.ToJson(data);
        var filePath = Path.Combine(Files.savesPath, $"{saveName}.json");
        if (!File.Exists(filePath)) {
            Directory.CreateDirectory(Files.savesPath);
            using var newSaveFile = File.Create(filePath);
        }
        File.WriteAllText(filePath, jsonOut);
    }
}

[Serializable]
public class ChunkData {
    public Vector2Int position;
    public byte[] foregroundData = new byte[Chunk.CHUNK_SIZE*Chunk.CHUNK_SIZE];
    public byte[] backgroundData = new byte[Chunk.CHUNK_SIZE*Chunk.CHUNK_SIZE];

    public ChunkData(Chunk chunk, Vector3Int pos) {
        this.position = (Vector2Int) pos;
        var tilesList = chunk.globalTileInfo.tiles;
        var foregroundData = from tile in chunk.GetAllTileData(isBackground:false)
                            select (byte) tilesList.IndexOf(tile);
        var backgroundData = from tile in chunk.GetAllTileData(isBackground:true)
                            select (byte) tilesList.IndexOf(tile);
        this.foregroundData = foregroundData.ToArray();
        this.backgroundData = backgroundData.ToArray();
    }
}
