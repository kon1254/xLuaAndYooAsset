using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ConvertLuaToTxt
{
    private static string luaFilePath = Application.dataPath + "/LuaScripts";
    private static string targetFilePath = Application.dataPath + "/GameResources";

    [MenuItem("Tools/Convert Lua to Txt")]
    public static void Convert()
    {
        if (!System.IO.Directory.Exists(luaFilePath))
        {
            Debug.LogError("Lua scripts directory does not exist: " + luaFilePath);
            return;
        }

        if (!System.IO.Directory.Exists(targetFilePath))
        {
            System.IO.Directory.CreateDirectory(targetFilePath);
        }
        else
        {
            // Clear the target directory if it already exists
            string[] existingFiles = System.IO.Directory.GetFiles(targetFilePath, "*.lua.txt");
            foreach (string file in existingFiles)
            {
                System.IO.File.Delete(file);
                Debug.Log($"Deleted existing file: {file}");
            }
        }

        ProcessDirectory(luaFilePath, targetFilePath);
        AssetDatabase.Refresh();
    }

    private static void ProcessDirectory(string sourceDir, string targetDir)
    {
        string[] files = System.IO.Directory.GetFiles(sourceDir, "*.lua");
        foreach (string file in files)
        {
            ProcessFile(file, targetDir);
        }

        string[] subDirs = System.IO.Directory.GetDirectories(sourceDir);
        foreach (string subDir in subDirs)
        {
            string newTargetDir = System.IO.Path.Combine(targetDir, System.IO.Path.GetFileName(subDir));
            if (!System.IO.Directory.Exists(newTargetDir))
            {
                System.IO.Directory.CreateDirectory(newTargetDir);
            }
            ProcessDirectory(subDir, newTargetDir);
        }
    }

    private static void ProcessFile(string file, string targetDir)
    {
        string content = System.IO.File.ReadAllText(file);
        string txtFilePath = System.IO.Path.Combine(targetDir, System.IO.Path.GetFileNameWithoutExtension(file) + ".lua.txt");
        System.IO.File.WriteAllText(txtFilePath, content);
        Debug.Log($"Converted {file} to {txtFilePath}");
    }
}