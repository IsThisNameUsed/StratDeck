using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Order : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public OrderManager.OrderType orderType;

    //script of the child to get the drag method
    public DragDrop_Order dragDrop_Order;
    public Image orderImage;
    public string tileNumber;

    public bool belongsLocalPlayer;

    public bool isActiv;
    public Animation activationOrderAnimation;

    public void StopAnimation()
    {
        activationOrderAnimation.Stop();
        isActiv = false;
    }

    void Start()
    {
        switch(orderType)
        {
            case OrderManager.OrderType.Move:
                dragDrop_Order = OrderManager.instance.moveOrderDragDrop.GetComponent<DragDrop_Order>();
                break;
            case OrderManager.OrderType.Administration:
                dragDrop_Order = OrderManager.instance.AdminOrderDragDrop.GetComponent<DragDrop_Order>();
                break;
            case OrderManager.OrderType.Guerilla:
                dragDrop_Order = OrderManager.instance.guerillaOrderDragDrop.GetComponent<DragDrop_Order>();
                break;
            case OrderManager.OrderType.Entrenchment:
                dragDrop_Order = OrderManager.instance.entrenchmentOrderDragDrop.GetComponent<DragDrop_Order>();
                break;
        }
        
    }
    
    public void SetDragDropOrder(DragDrop_Order dragDrop_Order)
    {
        this.dragDrop_Order = dragDrop_Order;
    }

    public void OnPointerDown(PointerEventData data)
    {
        if(MatchManager.instance.localPlayerTurn && belongsLocalPlayer)
        {
            Debug.Log("RESOLVE ORDER");
            //OrderManager.instance.DeleteOrder(tileNumber);
            if (!isActiv)
            {
                activationOrderAnimation.Play();
                isActiv = true;
            }
                
            PlayerInput.instance.SetActivOrder(this);
            PlayerInput.instance.selectedOrderData.orderType = orderType;
            PlayerInput.instance.selectedOrderData.tileNumber = tileNumber;
        }     
    }

    public void OnBeginDrag(PointerEventData data)
    {
        orderImage.enabled = false;
        if (dragDrop_Order != null)
        {
            dragDrop_Order.OnBeginDrag(data);
        }
    }
    public void OnDrag(PointerEventData data)
    {
        if (dragDrop_Order != null)
        {
            dragDrop_Order.OnDrag(data);
        }
    }
    public void OnEndDrag(PointerEventData data)
    {
        orderImage.enabled = true;
        OrderManager.instance.DeleteOrder(tileNumber);
        dragDrop_Order.OnEndDrag(data);
        orderImage.enabled = true;
        this.gameObject.SetActive(false);
    }
}
