using System.IO;
using UnityEngine;

public static class SaveLoadSystem 
{
    public static readonly string SAVE_DIR = Application.persistentDataPath + "/NeuralNetworks/";

    public static void Init()
    {
        if (!Directory.Exists(SAVE_DIR))
        {
            Directory.CreateDirectory(SAVE_DIR);
        }
    }

    public static void Save(string filename, string json)
    {
        int index = 0;
        while (File.Exists(SAVE_DIR + filename + " " + index + ".txt"))
        {
            index++;
        }
        File.WriteAllText(SAVE_DIR + filename + " " + index + ".txt", json);
    }

    public static string Load(string filename)
    {
        DirectoryInfo infoDir = new DirectoryInfo(SAVE_DIR);
        FileInfo infoFile = null;
        foreach (FileInfo f in infoDir.GetFiles())
        {
            if (f.Name.Substring(0, filename.Length) == filename)
            {
                if(infoFile == null || infoFile.LastWriteTime < f.LastWriteTime)
                    infoFile = f;
            }
        }
        if (infoFile != null)
        {
            return File.ReadAllText(infoFile.FullName);
        }
        return null;
    }
}
