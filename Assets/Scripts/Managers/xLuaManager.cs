using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using XLua;

public class xLuaManager : MonoSingleton<xLuaManager>
{
    LuaEnv luaEnv = null;
    private bool isGameStarted = false;
    private static string LuaScriptsFolder = "LuaScripts";

    protected override void Awake()
    {
        base.Awake();
        InitLuaEnv();
    }

    private void InitLuaEnv()
    {
        this.luaEnv = new LuaEnv();
        isGameStarted = false;
    }

    public byte[] LuaScriptLoader(ref string filePath)
    {
        string scriptPath = string.Empty;
#if UNITY_EDITOR
        filePath = filePath.Replace('.', '/') + ".lua";
        scriptPath = Path.Combine(Application.dataPath, LuaScriptsFolder, filePath);
        byte[] data = File.ReadAllBytes(scriptPath);
        return data;
#endif

        filePath = filePath.Replace('.', '/') + ".lua.txt";

    }

    public void EnterGame()
    {
        isGameStarted = true;
        //添加自定义代码装载器
        this.luaEnv.AddLoader(LuaScriptLoader);
        this.luaEnv.DoString("require (\"Main\")");
        this.luaEnv.DoString("Main.Init()");
    }

    // void Update()
    // {
    //     if (isGameStarted)
    //     {
    //         this.luaEnv.DoString("Main.Update()");
    //     }
    // }
}
