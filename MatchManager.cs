using Com.MyCompany.MyGame;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchManager : MonoBehaviour
{
    bool debugMode;
    public static MatchManager instance;

    public enum GamePhase { blueAction, redAction, planification };
    public GamePhase actualGamePhase;

    private const int maxAction = 3;
    public int actionPlayed;

    public Player redPlayer;
    public Player bluePlayer;
    public string localPlayer;
    public bool localPlayerTurn;

    public Text turnNumberText;
    public Text playerActivText;

    private int turnNumber;

    public GameObject gameInterface;
    public UIReferences uiReferences;
    public GameObject endTurnButton;

    private PhotonView photonView;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else Destroy(this);

        gameInterface = GameObject.Find("GameManager").GetComponent<MatchMaking>().inGamePanel;
        uiReferences = gameInterface.GetComponent<UIReferences>();

        endTurnButton = uiReferences.endTurnButton;
        turnNumberText = uiReferences.turnNumberText.GetComponent<Text>();
        playerActivText = uiReferences.playerActivText.GetComponent<Text>();

        if (PhotonNetwork.PlayerList.Length <= 1)
            debugMode = true;
        else debugMode = false;

    }

    private void Start()
    {
        photonView = PhotonView.Get(this);

        //instanciation des players
        GameObject redPlayerObject;
        GameObject bluePlayerObject;
        string path;
        
        if (PhotonNetwork.IsMasterClient)
        {
            path = "Prefabs/Player1";
            redPlayerObject = PhotonNetwork.InstantiateSceneObject(path, new Vector3(0, 0, 0), Quaternion.identity, 0);
           
            path = "Prefabs/Player2";
            bluePlayerObject = PhotonNetwork.InstantiateSceneObject(path, new Vector3(0, 0, 0), Quaternion.identity, 0);       
        }
        else
        {
            redPlayerObject = GameObject.Find("Player1(Clone)");           
            bluePlayerObject = GameObject.Find("Player2(Clone)");
        }

        redPlayerObject.transform.parent = this.transform;
        bluePlayerObject.transform.parent = this.transform;

        bluePlayerObject.name = "bluePlayer";
        redPlayerObject.name = "redPlayer";

        this.redPlayer = redPlayerObject.GetComponent<Player>();
        this.redPlayer.setPlayer(PhotonNetwork.PlayerList[0].NickName);

        this.bluePlayer = bluePlayerObject.GetComponent<Player>();

        if (PhotonNetwork.PlayerList.Length > 1) // If we are not in solo debug mod
        {  
            this.bluePlayer.setPlayer(PhotonNetwork.PlayerList[1].NickName);
        }

        
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView player1View = redPlayerObject.GetComponent<PhotonView>();
            player1View.TransferOwnership(1);

            PhotonView player2View = bluePlayerObject.GetComponent<PhotonView>();
            player2View.TransferOwnership(2);
        }

        SetLocalPlayer();

        actualGamePhase = GamePhase.planification;
        playerActivText.text= "planification";
        actionPlayed = 0;

        //We create starting units
        if (PhotonNetwork.IsMasterClient)
        {
           UnitManager.instance.InstantiateUnit(UnitManager.unitType.infantryman, "00", localPlayer);
        }
            
        else
        {
            string tileNumber = "40";// (Map.instance.mapWidth - 1).ToString() + "0";
            UnitManager.instance.InstantiateUnit(UnitManager.unitType.infantryman, tileNumber, localPlayer);
        }
    }

    private void Update()
    {
        if (debugMode)
            turnSystemDebugMode();
        else turnSystem();
    }
    private void turnSystem()
    {
        if (actualGamePhase == GamePhase.planification && redPlayer.GetIsReady() && bluePlayer.GetIsReady())
        {
            turnNumber += 1;
            turnNumberText.text = turnNumber.ToString();
            redPlayer.SetIsReady(false);
            bluePlayer.SetIsReady(false);
            NextPhase();

            OrderManager.instance.revealOpponentOrder();
        }
        else if (actualGamePhase == GamePhase.blueAction && bluePlayer.GetIsReady())
        {
            actionPlayed++;
            bluePlayer.SetIsReady(false);
            NextPhase();
        }
        else if (actualGamePhase == GamePhase.redAction && redPlayer.GetIsReady())
        {
            actionPlayed++;
            redPlayer.SetIsReady(false);
            NextPhase();
        }
    }
    private void turnSystemDebugMode()
    {
        if (actualGamePhase == GamePhase.planification && redPlayer.GetIsReady() && debugMode)
        {
            turnNumber += 1;
            turnNumberText.text = turnNumber.ToString();
            redPlayer.SetIsReady(false);
            bluePlayer.SetIsReady(false);
            NextPhase();

            OrderManager.instance.revealOpponentOrder();
        }
        else if (actualGamePhase == GamePhase.blueAction && debugMode)
        {
            actionPlayed++;
            bluePlayer.SetIsReady(false);
            NextPhase();
        }
        else if (actualGamePhase == GamePhase.redAction && redPlayer.GetIsReady())
        {
            actionPlayed++;
            redPlayer.SetIsReady(false);
            NextPhase();
        }
    }
    private void NextPhase()
    {
        if (actionPlayed / 2 == maxAction)
        {
            actualGamePhase = GamePhase.planification;

            OrderManager.instance.clearOpponentOrderList();
            OrderManager.instance.clearLocalPlayerOrderList();

            endTurnButton.SetActive(true);
            playerActivText.text = "planification";
            actionPlayed = 0;
            localPlayerTurn = false;

        }
        else if (actualGamePhase == GamePhase.planification || actualGamePhase == GamePhase.redAction)
        {
            actualGamePhase = GamePhase.blueAction;
            if (localPlayer == "redPlayer")
            {
                endTurnButton.SetActive(false);
                localPlayerTurn = false;
            }
            else
            {
                endTurnButton.SetActive(true);
                localPlayerTurn = true;
            }
            playerActivText.text = "bluePlayer";
        }
        else if (actualGamePhase == GamePhase.blueAction)
        {
            actualGamePhase = GamePhase.redAction;
            if (localPlayer == "redPlayer")
            {
                endTurnButton.SetActive(true);
                localPlayerTurn = true;
            }
            else 
            {
                endTurnButton.SetActive(false);
                localPlayerTurn = false;
            }
            playerActivText.text = "redPlayer";
        }
       
    }

    public void SetPlayerReady()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName);
        if (localPlayer == "redPlayer")
            photonView.RPC("PlayerIsReady", RpcTarget.All, "red");
        else photonView.RPC("PlayerIsReady", RpcTarget.All, "blue");
    }

    public void SetLocalPlayer()
    {
        if (PhotonNetwork.LocalPlayer.NickName == redPlayer.GetNickName())
        {
            localPlayer = "redPlayer";
            //GameObject.Find("TileMenu").GetComponent<TileMenu>().Setup("red");
        }
        else if (PhotonNetwork.LocalPlayer.NickName == bluePlayer.GetNickName())
        {
            localPlayer = "bluePlayer";
            //GameObject.Find("TileMenu").GetComponent<TileMenu>().Setup("blue");
        }

        
    }

    public string ReturnLocalPlayerColor()
    {
        if (localPlayer == "redPlayer")
            return "red";
        else return "blue";
    }

    #region PunRpc
    [PunRPC]
    public void PlayerIsReady(string playerColor)
    {
        if (playerColor == "red")
            redPlayer.SetIsReady(true);
        else bluePlayer.SetIsReady(true);
    }

    #endregion
}
