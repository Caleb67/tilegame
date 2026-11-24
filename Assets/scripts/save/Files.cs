
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class Files
{
    public static readonly string path = Application.persistentDataPath;
    public static readonly string savesPath = Path.Combine(path, "saves");

    public static string GetOrCreatePath(string relativeSaveFilePath) {
        var filePath = Path.Combine(Files.savesPath, $"{relativeSaveFilePath}");
        if (!File.Exists(filePath)) {
            var dir = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(dir);
            using var newSaveFile = File.Create(filePath);
            using var writer = new StreamWriter(newSaveFile, Encoding.UTF8);
            writer.Write("{}");
        }
        return filePath;
    }
    
    public static bool ExistsSave(string saveName) {
        var relativeSaveFilePath = $"{saveName}.json";
        var filePath = Path.Combine(Files.savesPath, $"{relativeSaveFilePath}");
        return File.Exists(filePath);
    }

    public static List<Tuple<LevelData, String>> GetAllSaves() {
        var allFilePaths = Directory.GetFiles(savesPath);

        return allFilePaths.Select(
            filePath => {
                var relativeSaveFilePath = Path.GetRelativePath(savesPath, filePath);
                var pathWOExtension = Path.ChangeExtension(relativeSaveFilePath, null);
                return Tuple.Create(
                    LevelData.LoadFromFile(pathWOExtension),
                    pathWOExtension
                );
            }
        ).ToList();
    }
}