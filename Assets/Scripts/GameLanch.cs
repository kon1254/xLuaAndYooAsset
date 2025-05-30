using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLanch : MonoSingleton<GameLanch>
{
    protected override void Awake()
    {
        base.Awake();

        this.gameObject.AddComponent<xLuaManager>();
        this.gameObject.AddComponent<ResourceManager>();
    }

    private void Start()
    {
        this.StartCoroutine(GameStart());
    }

    IEnumerator GameStart()
    {
        yield return this.StartCoroutine(checkHotUpdate());
        xLuaManager.Instance.EnterGame();
    }

    IEnumerator checkHotUpdate()
    {
        while (!ResourceManager.Instance.isInitialized)
        {
            yield return null;
        }
    }
}
