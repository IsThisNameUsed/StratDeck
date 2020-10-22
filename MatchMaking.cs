using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;


namespace Com.MyCompany.MyGame
{
    public class MatchMaking : MonoBehaviourPunCallbacks
    {
        public MapGenerator mapGenerator;
        public GameObject gameInterface;
        public GameObject launchBattlePanel;
        public GameObject inGamePanel;
        public GameObject connectionPanel;
        public GameObject launchButton;
        public GameObject mapOptionPanel;
        public MatchManager matchManager;
        public PhotonView pv;
        public GameObject canvasTest;


        #region Unity Methods

        void Start()
        {
            gameInterface = Resources.Load<GameObject>("Prefabs/Canvas");
            gameInterface = Instantiate(gameInterface, new Vector3(0, 0, 0), Quaternion.identity);

            launchBattlePanel = gameInterface.transform.Find("LaunchBattlePanel").gameObject;
            inGamePanel = gameInterface.transform.Find("InGamePanel").gameObject;
            mapOptionPanel = launchBattlePanel.transform.Find("MapOptionPanel").gameObject;
            connectionPanel = launchBattlePanel.transform.Find("ConnectionPanel").gameObject;
            launchButton = launchBattlePanel.transform.Find("ButtonPanel").transform.Find("LaunchBattleButton").gameObject;

            launchBattlePanel.SetActive(true);
            inGamePanel.SetActive(false);

            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Désactive button launch");
                launchButton.SetActive(false);
                Text text = connectionPanel.transform.Find("ConnectionText").gameObject.GetComponent<Text>();
                text.text = "Vous êtes connecté à la room de " + PhotonNetwork.PlayerListOthers[0].NickName;
                mapOptionPanel.SetActive(false);
                Debug.Log("t pas master!!");
            }
            else
            {
                //canvasTest = PhotonNetwork.InstantiateSceneObject("Prefabs/CanvasTest", new Vector3(0, 0, 0), Quaternion.identity, 0);
                //pv = canvasTest.gameObject.GetComponent<PhotonView>();
                //pv.TransferOwnership(1);
                Button button = launchButton.GetComponent<Button>();
                button.interactable = true;
            }
        }


        #endregion


        #region Public Methods

        //Seul l'hôte exécute cette fonction
        public void LaunchMatch()
        {
            Debug.Log("CreateMap");
            mapGenerator.CreateMap();           
            if (launchButton.activeInHierarchy)
            {
                launchButton.SetActive(false);
            }
            //instanciation du matchManager 
            string path = "Prefabs/MatchManager";
            GameObject matchManagerGameObject = PhotonNetwork.InstantiateSceneObject(path, new Vector3(0, 0, 0), Quaternion.identity, 0);
            matchManager = matchManagerGameObject.GetComponent<MatchManager>();
            matchManager.name = "MatchManager";

            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("matchLaunched", RpcTarget.All);
        }

        #endregion

        #region Photon Callbacks

        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

            Debug.Log("Player " + other.NickName + " created");
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
                Text text = connectionPanel.transform.Find("ConnectionText").gameObject.GetComponent<Text>();
                text.text = PhotonNetwork.PlayerList[1].NickName + " is connected, you can launch the game";
                Button button = launchButton.GetComponent<Button>();
                button.interactable = true;
            }
        }


        public override void OnPlayerLeftRoom(Photon.Realtime.Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
                //LoadArena();
                PhotonNetwork.LeaveRoom();
            }
        }
        #endregion

        #region PunRpc
        [PunRPC]
        public void matchLaunched()
        {
            launchBattlePanel.SetActive(false);
            inGamePanel.SetActive(true);
            Debug.Log("MATCH BEGIN ASSHOLE");

            if (!PhotonNetwork.IsMasterClient)
            {
                Map.instance.InitializeMapForClient();
                Map.instance.InitializeTargetingSphere();
            }

        }

        #endregion
    }
}


