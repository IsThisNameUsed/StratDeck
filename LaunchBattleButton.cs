using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchBattleButton : MonoBehaviour
{
    private Com.MyCompany.MyGame.MatchMaking gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<Com.MyCompany.MyGame.MatchMaking>();
    }

    public void LaunchMatch()
    {
        gameManager.LaunchMatch();
    }
}
