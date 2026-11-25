
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class LevelData {
    public List<Chunk> chunks = new();
    public Inventory inventory = new();
    public Vector2 playerPositon = new();
    public String saveName = "";
    public int saveSeed;
    public bool isSandboxMode = false;

    public static LevelData LoadFromFile(String saveName) {
        var filePath = Files.GetOrCreatePath($"{saveName}.json");
        
        var json = File.ReadAllText(filePath);
        
        return JsonUtility.FromJson<LevelData>(json);
    }

    public static void Save(String saveName, LevelData data) {
        var filePath = Path.Combine(Files.savesPath, $"{saveName}.json");
        filePath = Files.GetOrCreatePath(filePath);
        
        var jsonOut = JsonUtility.ToJson(data);

        File.WriteAllText(filePath, jsonOut);
    }
}
