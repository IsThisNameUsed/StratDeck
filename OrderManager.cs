using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OrderManager : MonoBehaviour
{
    public struct OrderData
    {
        public string tileNumber;
        public OrderType orderType;
    }

    public List<OrderData> oppponentOrdersList;
    public List<OrderData> localPlayerOrdersList;

    public static OrderManager instance;

    public enum OrderType { Move, Entrenchment, Administration, Guerilla, Hidden, Nothing};
    public int moveOrderActive = 0;
    public int entrenchmentOrderActive = 0;
    public int AdminOrderActive = 0;
    public int guerillaOrderActive = 0;

    public GameObject moveOrderDragDrop;
    public GameObject entrenchmentOrderDragDrop;
    public GameObject AdminOrderDragDrop;
    public GameObject guerillaOrderDragDrop;

    private PhotonView photonView;
    
    void Start()
    {
        if (instance == null)
            instance = this;
        else Destroy(this);
        
        photonView = PhotonView.Get(this);

        oppponentOrdersList = new List<OrderData>();
        localPlayerOrdersList = new List<OrderData>();

       
        moveOrderDragDrop = MatchManager.instance.uiReferences.moveOrderDragDrop;
        entrenchmentOrderDragDrop = MatchManager.instance.uiReferences.entrenchmentOrderDragDrop;
        AdminOrderDragDrop = MatchManager.instance.uiReferences.AdminOrderDragDrop;
        guerillaOrderDragDrop = MatchManager.instance.uiReferences.guerillaOrderDragDrop;
}

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool AddOrder(string tileName, OrderType orderType)
    {
        string tileNumber = tileName.Substring(tileName.Length - 2);
        Tile targetedTile = Map.instance.FindTile(tileNumber);

        if (targetedTile.hasAnActiveOrder || MatchManager.instance.localPlayer != targetedTile.controller)
            return true;

        targetedTile.AddOrder(orderType, false);
        AddOpponentOrder(tileNumber, orderType);
        OrderData orderData = new OrderData();
        orderData.tileNumber = tileNumber;
        orderData.orderType = orderType;
        localPlayerOrdersList.Add(orderData);
        majActiveOrders(1, orderType);
        return false;
    }

    private void majActiveOrders(int increment, OrderType orderType)
    {
        switch(orderType)
        {
            case OrderType.Move:
                moveOrderActive += increment;
                break;
            case OrderType.Entrenchment:
                entrenchmentOrderActive += increment;
                break;
            case OrderType.Guerilla:
                guerillaOrderActive += increment;
                break;
            case OrderType.Administration:
                AdminOrderActive += increment;
                break;
            default:
                break;
        }
    }

    public void AddOpponentOrder(string tileNumber, OrderType orderType)
    {
        photonView.RPC("RPCAddOpponentOrder", RpcTarget.Others, tileNumber, orderType);
    }

    public void DeleteOrder(string tileNumber)
    {
        Debug.Log("DeleteORDER");
        Tile targetedTile = Map.instance.FindTile(tileNumber);
        targetedTile.hasAnActiveOrder = false;
        OrderType orderType = targetedTile.CleanCanvasOrder();

        int index = 0;
        foreach (OrderData localPlayerOrder in localPlayerOrdersList)
        {
            if (tileNumber == localPlayerOrder.tileNumber)
            {
                break;
            }
            index += 1;
        }
        if(index < localPlayerOrdersList.Count)
        {
            localPlayerOrdersList.RemoveAt(index);
            majActiveOrders(-1, orderType);
            DeleteOpponentOrder(tileNumber);
        }
        
        
    }

    public void DeleteOpponentOrder(string tileNumber)
    {
        photonView.RPC("RPCDeleteOpponentOrder", RpcTarget.Others, tileNumber);
    }

    public void revealOpponentOrder()
    {
        foreach (OrderData oppOrder in oppponentOrdersList)
        {
            Tile targetedTile = Map.instance.FindTile(oppOrder.tileNumber);
            targetedTile.CleanCanvasOrder();
            targetedTile.AddOrder(oppOrder.orderType, true);
        }
    }

    public void clearOpponentOrderList()
    {
        if (oppponentOrdersList.Count <= 0)
            return;

        foreach (OrderData oppOrder in oppponentOrdersList)
        {
            Tile targetedTile = Map.instance.FindTile(oppOrder.tileNumber);
            targetedTile.CleanCanvasOrder();
        }
        oppponentOrdersList.Clear();
    }

    public void clearLocalPlayerOrderList()
    {
        if (localPlayerOrdersList.Count <= 0)
            return;

        foreach (OrderData oppOrder in localPlayerOrdersList)
        {
            Tile targetedTile = Map.instance.FindTile(oppOrder.tileNumber);
            targetedTile.CleanCanvasOrder();
        }
        localPlayerOrdersList.Clear();
    }

    #region PunRpc
    //Affiche l'ordre, face caché, chez l'adversaire
    [PunRPC]
    public void RPCAddOpponentOrder(string tileNumber, OrderType orderType)
    {
        OrderData hiddenOrder = new OrderData();
        hiddenOrder.tileNumber = tileNumber;
        hiddenOrder.orderType = orderType;
        oppponentOrdersList.Add(hiddenOrder);
        Tile targetedTile = Map.instance.FindTile(tileNumber);
        targetedTile.AddOrder(OrderType.Hidden, true);
    }

    //Supprime l'affichage de l'ordre joué ou annulé chez l'adversaire
    [PunRPC]
    public void RPCDeleteOpponentOrder(string tileNumber)
    {
        int index = 0;
        foreach(OrderData oppOrder in oppponentOrdersList)
        {
            if(tileNumber == oppOrder.tileNumber)
            {
                Tile targetedTile = Map.instance.FindTile(tileNumber);
                targetedTile.CleanCanvasOrder();
            }
            index += 1;
        }

        oppponentOrdersList.RemoveAt(index);
    }
#endregion

}
