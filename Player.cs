using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string nickName;
    public string color;
    public bool isReady = false;

    public void setPlayer(string nickName)
    {
        this.nickName = nickName;
    }

    public void IsReady()
    {
        isReady = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Player says" + nickName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string GetNickName()
    {
        return nickName;
    }

    public bool GetIsReady()
    {
        return isReady;
    }

    public void SetIsReady(bool value)
    {
        isReady = value;
    }
}
