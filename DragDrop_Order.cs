using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DragDrop_Order : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private RectTransform rectTransform;
    [SerializeField]
    OrderManager.OrderType orderType;
    Vector3 startPosition;

    private void Awake()
    { 
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(MatchManager.instance.actualGamePhase == MatchManager.GamePhase.planification)
            rectTransform.anchoredPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(MatchManager.instance.actualGamePhase == MatchManager.GamePhase.planification)
            rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!(MatchManager.instance.actualGamePhase == MatchManager.GamePhase.planification))
            return;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 500))
        {
            MeshRenderer meshRenderer = hit.transform.GetComponent<MeshRenderer>();
            if(!CheckIfOrderIsAvailable(orderType))
            {
                //Unavailable Order
            }
            else if(meshRenderer != null && hit.collider.transform.tag == "Tile")
            {
                string tileName = hit.collider.transform.name;
                bool tileNotAvailableForOrder = OrderManager.instance.AddOrder(tileName, orderType);
            }
            else if(meshRenderer != null && hit.collider.transform.tag == "Unit")
            {
                string tileNumber = hit.transform.gameObject.GetComponent<Unit>().tileNumber;
                string tileName = Map.instance.tileNumberToName(tileNumber);
                bool tileNotAvailableForOrder = OrderManager.instance.AddOrder(tileName, orderType);
            }
        }
        rectTransform.anchoredPosition = startPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
       
    }

    private bool CheckIfOrderIsAvailable(OrderManager.OrderType orderType)
    {
        if (orderType == OrderManager.OrderType.Move && OrderManager.instance.moveOrderActive < 2)
            return true;
        else if (orderType == OrderManager.OrderType.Entrenchment && OrderManager.instance.entrenchmentOrderActive < 2)
            return true;
        else if (orderType == OrderManager.OrderType.Administration && OrderManager.instance.AdminOrderActive < 2)
            return true;
        else if (orderType == OrderManager.OrderType.Guerilla && OrderManager.instance.guerillaOrderActive < 2)
            return true;
        else return false;
    }
}
