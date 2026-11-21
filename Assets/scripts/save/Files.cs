
using System.IO;
using NUnit.Framework.Constraints;
using UnityEngine;

public class Files
{
    public static readonly string path = Application.persistentDataPath;
    public static readonly string savesPath = Path.Combine(path, "saves");
}