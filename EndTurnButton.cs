using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void PlayerIsReady()
    {
        MatchManager.instance.SetPlayerReady();
    }
}
