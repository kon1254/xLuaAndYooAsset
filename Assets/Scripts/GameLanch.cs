using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLanch : MonoSingleton<GameLanch>
{
    protected override void Awake()
    {
        base.Awake();

        this.gameObject.AddComponent<xLuaManager>();
    }

    IEnumerator checkHotUpdate()
    {
        yield return 0;
    }

    IEnumerator GameStart()
    {
        yield return this.StartCoroutine(checkHotUpdate());
        xLuaManager.Instance.EnterGame();
    }

    private void Start()
    {
        this.StartCoroutine(GameStart());
    }
}
