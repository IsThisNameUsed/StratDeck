using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput instance;
    private Tile oldSelected;
    private Tile oldTargeted;
    private Tile selectedTile;
    private Tile targetedTile;

    public OrderManager.OrderData selectedOrderData;
    private bool unitSelectionAuthorized;
    private Order activOrder;

    public List<Unit> selectedUnits;

    //Debug
    private bool addUnit;

    public void SetActivOrder(Order activOrder)
    {
        this.activOrder = activOrder;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        else Destroy(this);

        ResetOrderData();

        addUnit = false;
        unitSelectionAuthorized = false;
    }

    // Update is called once per frame
    void Update()
    {
        CaptureMouseInput();
    }

    private void CaptureMouseInput()
    {
        if (Input.GetMouseButtonDown(0) && Camera.main != null)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 500))
            {
                MeshRenderer meshRenderer = hit.transform.GetComponent<MeshRenderer>();

                if (meshRenderer != null && hit.collider.transform.tag == "Tile")
                {
                    Debug.Log("touche tile");
                    GameObject targetedTileObj = hit.transform.gameObject;
                    Tile targetedTile = targetedTileObj.GetComponent<Tile>();
                    if(addUnit)
                    {
                        string tileNumber = targetedTile.ReturnTileNumber();
                        UnitManager.instance.InstantiateUnit(UnitManager.unitType.infantryman, tileNumber, MatchManager.instance.localPlayer);
                    }
                    
                }
                else if(meshRenderer != null && hit.collider.transform.tag == "Unit")
                {
                    GameObject unitObj = hit.collider.gameObject;
                    Unit unitScript = unitObj.GetComponent<Unit>();

                    if (selectedOrderData.orderType == OrderManager.OrderType.Move && 
                        selectedOrderData.tileNumber == unitScript.tileNumber)
                    {
                        bool isSelected;
                        Color newColor;
                        
                        if (selectedUnits.Contains(unitScript))
                        {
                            isSelected = false;
                            int index = selectedUnits.IndexOf(unitScript);
                            selectedUnits.RemoveAt(index);
                            newColor = UnitManager.instance.ReturnUnitColor(isSelected);  
                        }
                        else
                        {
                            isSelected = true;
                            selectedUnits.Add(unitScript);
                            newColor = UnitManager.instance.ReturnUnitColor(isSelected);

                        }
                        unitScript.SetUnitColor(newColor);
                    } 
                }
            }
        }
        else if(Input.GetMouseButtonDown(1) && Camera.main != null)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 500))
            {
                MeshRenderer meshRenderer = hit.transform.GetComponent<MeshRenderer>();
                CanvasRenderer canvasRenderer = hit.transform.GetComponent<CanvasRenderer>();
                if(meshRenderer != null && hit.collider.transform.tag == "Tile" && selectedUnits.Count > 0)
                {
                    Tile targetedTile = hit.collider.gameObject.GetComponent<Tile>();
                    bool authorization = true;
                    foreach (Unit unit in selectedUnits)
                    {
                        if (!unit.MoveToAuthorized(targetedTile))
                        {
                            authorization = false;
                            break;
                        }
                    }

                    if (authorization == true)
                    {
                        int numberOfUnitMoving = 0;
                        foreach (Unit unit in selectedUnits)
                        {
                            unit.MoveTo(targetedTile);
                            numberOfUnitMoving += 1;
                        }
                        ClearSelectedUnit();

                        Tile startTile = Map.instance.FindTile(selectedOrderData.tileNumber);
                        startTile.SetNumberOfUnit(-numberOfUnitMoving);
                        if (startTile.GetNumberOfUnit() <= 0)
                        {
                            activOrder.StopAnimation();
                            OrderManager.instance.DeleteOrder(selectedOrderData.tileNumber);
                        }
                    }
                }
            }
        }
        else if(Input.GetKeyDown(KeyCode.A))
        {
            ClearSelectedUnit();
            addUnit = !addUnit;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (selectedUnits.Count > 0)
                ClearSelectedUnit();
            else
            {
                activOrder.StopAnimation();
                ResetOrderData();
            }
        }
    }

    private void ClearSelectedUnit()
    {
        bool isSelected = false;
        foreach(Unit unit in selectedUnits)
        {
            unit.SetUnitColor(UnitManager.instance.ReturnUnitColor(isSelected));
        }
        selectedUnits.Clear();
    }

    private void ResetOrderData()
    {
        selectedOrderData.orderType = OrderManager.OrderType.Nothing;
        selectedOrderData.tileNumber = "";
    }

}
