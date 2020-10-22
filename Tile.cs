using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public OutlineObject outline;
    public List<TileForGeneration> DataForNeighborsList;
    public List<Tile> neighboursList;
    public List<string> neighboursName;
    public GameObject tileObject;
    public Vector3 position;
    public string tileName;
    public int style;
    public MapGenerator.strategic stratElt;

    private GameObject orderCanvas_obj;
    private Canvas orderCanvas;
    private GameObject moveOrder;
    private Image moveOrderImage;
    private Order moveOrderScript;
    private GameObject guerrillaOrder;
    private Image guerrillaOrderImage;
    private Order guerrillaOrderScript;
    private GameObject entrenOrder;
    private Image entrenOrderImage;
    private Order entrenOrderScript;
    private GameObject adminOrder;
    private Image adminOrderImage;
    private Order adminOrderScript;
    private GameObject hiddenOrder;
    private Image hiddenOrderImage;
    private Order hiddenOrderScript;
    
    private GameObject resourcesCanvas_obj;
    [SerializeField]
    private GameObject foodResources_obj;
    [SerializeField]
    private GameObject strategicResources_obj;
    [SerializeField]
    private GameObject goldResources_obj;
    private int foodIncome;
    private int goldIncome;
    private int strategicIncome;

    private List<Vector3> unitsPositions;
    [SerializeField]
    private int numberOfUnits;

    private PhotonView photonView;
    private GameObject targetingSphere;
    private Renderer targetingSphereRenderer;
   
    public string controller;  // three controller: redPlayer, bluePlayer, neutral
    public bool hasAnActiveOrder;

    public AnimationClip orderAnimation;
    #region INIT

    public void Awake()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            GameObject map = GameObject.Find("Map(Clone)");
            this.transform.parent = map.transform;
        }

        numberOfUnits = 0;
        unitsPositions = new List<Vector3>();
        unitsPositions.Add(new Vector3(-0.25f, 0.8f, 0.25f));
        unitsPositions.Add(new Vector3(0.25f, 0.8f, 0.25f));
        unitsPositions.Add(new Vector3(0, 0.8f, -0.25f));
    }

    public void Start()
    {
        outline = GetComponent<OutlineObject>();
        photonView = GetComponent<PhotonView>();

        // Create orders canvas
        orderCanvas_obj = new GameObject();
        orderCanvas_obj.name = "orderCanvas";
        orderCanvas = orderCanvas_obj.gameObject.AddComponent<Canvas>();
        orderCanvas_obj.transform.SetParent(this.gameObject.transform);
        orderCanvas.renderMode = RenderMode.WorldSpace;
        RectTransform rectTransform = orderCanvas_obj.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(0, 1f, 0);
        rectTransform.localRotation = Quaternion.Euler(90, 0, 0);
        rectTransform.sizeDelta = new Vector2(1, 1);


        moveOrder = new GameObject();
        setOrderImage(moveOrder, "moveOrder", "Orders/move", ref moveOrderImage, ref moveOrderScript);

        entrenOrder = new GameObject();
        setOrderImage(entrenOrder, "entrenOrder", "Orders/entrenchment", ref entrenOrderImage, ref entrenOrderScript);

        guerrillaOrder = new GameObject();
        setOrderImage(guerrillaOrder, "guerrillaOrder", "Orders/guerrilla", ref guerrillaOrderImage, ref guerrillaOrderScript);

        adminOrder = new GameObject();
        setOrderImage(adminOrder, "adminOrder", "Orders/administration", ref adminOrderImage, ref adminOrderScript);

        hiddenOrder = new GameObject();
        setOrderImage(hiddenOrder, "hiddenOrder", "Order/hiddenOrder", ref hiddenOrderImage, ref hiddenOrderScript);

        CleanCanvasOrder();

        // Get resources sprite
        resourcesCanvas_obj = this.gameObject.transform.Find("Canvas").gameObject;
        foodResources_obj = resourcesCanvas_obj.transform.Find("Food").gameObject;
        strategicResources_obj = resourcesCanvas_obj.transform.Find("Strategic").gameObject;
        goldResources_obj = resourcesCanvas_obj.transform.Find("Gold").gameObject;
        SetResourcesSprite();

    }

    private void setOrderImage(GameObject gameObject, string orderName, string resourcesPath, ref Image image, ref Order orderScript)
    {
        gameObject.name = orderName;
        gameObject.transform.SetParent(orderCanvas_obj.transform);
        image = gameObject.AddComponent<Image>();
        image.sprite = Resources.Load<Sprite>(resourcesPath);
        image.material = Resources.Load<Material>("Materials/UiOnTop");

        orderScript = gameObject.AddComponent<Order>();
        orderScript.activationOrderAnimation = gameObject.AddComponent<Animation>();
        orderScript.activationOrderAnimation.clip = orderAnimation;//(AnimationClip)Resources.Load("Animations/OrderActivation_Animation"); // 
        orderScript.activationOrderAnimation.playAutomatically = false;
        orderScript.tileNumber = tileName.Substring(4, 2);
        orderScript.orderImage = image;
        orderScript.orderType = stringToOrderType(orderName);

        gameObject.AddComponent<GraphicRaycaster>();
       
        RectTransform rectTransform;
        rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0.7f, 0.7f);
        rectTransform.localPosition = new Vector3(0, 0, 0);
        rectTransform.localRotation = Quaternion.Euler(0, 0, -30);

        gameObject.tag = "Order";
        gameObject.SetActive(false);
    }

    private OrderManager.OrderType stringToOrderType(string orderName)
    {
        switch (orderName)
        {
            case "moveOrder":
                return OrderManager.OrderType.Move;
            case "entrenOrder":
                return OrderManager.OrderType.Entrenchment;
            case "guerrillaOrder":
                return OrderManager.OrderType.Guerilla;
            case "adminOrder":
                return OrderManager.OrderType.Administration;
            default:
                return OrderManager.OrderType.Hidden;
        }
    }

    private void SetResourcesSprite()
    {
        if (foodIncome > 0)
            foodResources_obj.SetActive(true);
        else foodResources_obj.SetActive(false);
        if (goldIncome > 0)
            goldResources_obj.SetActive(true);
        else goldResources_obj.SetActive(false);
        if (foodIncome > 0)
            strategicResources_obj.SetActive(true);
        else strategicResources_obj.SetActive(false);
    }

    //Appelée uniquement par le Master
    public void InitializeTile(int _style, MapGenerator.strategic _stratElt, int foodIncome, int goldIncome, int strategicIncome)
    {
        tileName = gameObject.name;
        tileObject = gameObject;
        position = tileObject.transform.position;
        style = _style;
        stratElt = _stratElt;
        this.foodIncome = foodIncome;
        this.goldIncome = goldIncome;
        this.strategicIncome = strategicIncome;
    }

    //Initialisation chez le client
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        style = (int)instantiationData[0];
        tileName = (string)instantiationData[1];
        stratElt = stringToStratElt((string)instantiationData[2]);
        tileObject = this.gameObject;
        position = tileObject.transform.position;
        tileObject.name = tileName;
        this.controller = (string)instantiationData[3];
        this.foodIncome = (int)instantiationData[4];
        this.goldIncome = (int)instantiationData[5];
        this.strategicIncome = (int)instantiationData[6];
        neighboursName = new List<string>();
        for (int i=7; i< instantiationData.Length; i++)
        {
            string tileName = (string)instantiationData[i];
            neighboursName.Add((string)instantiationData[i]);
        }
    }

    public void CreateNeighboursList()
    {
        neighboursList = new List<Tile>();
        foreach(string tileName in neighboursName)
        {
            string tileNumber = Map.instance.tileNameToNumber(tileName);
            neighboursList.Add(Map.instance.FindTile(tileNumber));
        }
    }
    #endregion

    #region UTILITIES

    public void SetController(string incomingController)
    {
        if (controller == "neutral")
            controller = incomingController;
        else
        {
            // FIGHT
        }
    }

    private MapGenerator.strategic stringToStratElt(string stratEltName)
    {
        switch (stratEltName)
        {
            case "castle":
                return MapGenerator.strategic.castle;
            case "nothing":
                return MapGenerator.strategic.nothing;
            default:
                return MapGenerator.strategic.nothing;
        }
    }
    public void CreateTargetingSphere(GameObject targetingSpherePrefab)
    {
        targetingSphere = Instantiate(targetingSpherePrefab);
        Debug.Log(targetingSphere.name);
        targetingSphere.transform.position = transform.position + new Vector3(0, 0.7f, 0);
        targetingSphere.transform.parent = transform;
        targetingSphere.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        targetingSphereRenderer = targetingSphere.GetComponent<Renderer>();
        targetingSphere.SetActive(false);
    }

    public void IsSelected()
    {
        //targetingSphere.SetActive(true);
        //targetingSphereRenderer.material.color = new Vector4(255, 0, 0, 82);
    }

    public void IsTargeted()
    {
        //targetingSphere.SetActive(true);
        //targetingSphereRenderer.material.color = new Vector4(0, 0, 255, 82);
    }

    public void IsNoLongerFocused()
    {
        //targetingSphere.SetActive(false);
    }

    public Vector3 returnTransformPosition()
    {
        return this.transform.position;
    }

    public string ReturnTileNumber()
    {
        return this.name.Substring(name.Length - 2);
    }
    #endregion

    #region ORDER
    public void AddOrder(OrderManager.OrderType order, bool isOpponentOrder)
    {
        hasAnActiveOrder = true;
        switch (order)
        {
            case OrderManager.OrderType.Move:
                moveOrder.SetActive(true);
                if (isOpponentOrder)
                {
                    moveOrderImage.color = new Color(0.77f, 0.21f, 0.21f, 1);
                    moveOrderScript.belongsLocalPlayer = false;
                }
                else
                {
                    moveOrderImage.color = new Color(1, 1, 1, 1);
                    moveOrderScript.belongsLocalPlayer = true;
                }
                break;
            case OrderManager.OrderType.Guerilla:
                guerrillaOrder.SetActive(true);
                if (isOpponentOrder)
                {
                    guerrillaOrderImage.color = new Color(0.77f, 0.21f, 0.21f, 1);
                    guerrillaOrderScript.belongsLocalPlayer = false;
                }
                else
                {
                    guerrillaOrderImage.color = new Color(1, 1, 1, 1);
                    guerrillaOrderScript.belongsLocalPlayer = true;
                }
                break;
            case OrderManager.OrderType.Entrenchment:
                entrenOrder.SetActive(true);
                if (isOpponentOrder)
                {
                    entrenOrderImage.color = new Color(0.77f, 0.21f, 0.21f, 1);
                    entrenOrderScript.belongsLocalPlayer = false;
                }
                else
                {
                    entrenOrderImage.color = new Color(1, 1, 1, 1);
                    entrenOrderScript.belongsLocalPlayer = true;
                }
                break;
            case OrderManager.OrderType.Administration:
                adminOrder.SetActive(true);
                if (isOpponentOrder)
                {
                    adminOrderImage.color = new Color(0.77f, 0.21f, 0.21f, 1);
                    adminOrderScript.belongsLocalPlayer = false;
                }
                else
                {
                    adminOrderImage.color = new Color(1, 1, 1, 1);
                    adminOrderScript.belongsLocalPlayer = true;
                }
                break;
            case OrderManager.OrderType.Hidden:
                hiddenOrder.SetActive(true);
                if (isOpponentOrder)
                {
                    hiddenOrderImage.color = new Color(0.77f, 0.21f, 0.21f, 1);
                    hiddenOrderScript.belongsLocalPlayer = false;
                }
                else
                {
                    hiddenOrderImage.color = new Color(1, 1, 1, 1);
                    hiddenOrderScript.belongsLocalPlayer = false;
                }
                break;
            default:
                hasAnActiveOrder = false;
                break;
        }
    }

    public OrderManager.OrderType CleanCanvasOrder()
    {
        hasAnActiveOrder = false;
        Debug.Log("CLEAN");
        if (moveOrder.activeSelf)
        {
            moveOrder.SetActive(false);
            return (OrderManager.OrderType.Move);
        }
        else if (guerrillaOrder.activeSelf)
        {
            guerrillaOrder.SetActive(false);
            return (OrderManager.OrderType.Guerilla);
        }
        else if (entrenOrder.activeSelf)
        {
            entrenOrder.SetActive(false);
            return (OrderManager.OrderType.Entrenchment);
        }
        else if (adminOrder.activeSelf)
        {
            adminOrder.SetActive(false);
            return (OrderManager.OrderType.Administration);
        }
        else
        {
            hiddenOrder.SetActive(false);
            return (OrderManager.OrderType.Hidden);
        }

    }

    #endregion

    #region UNITS
    public int GetNumberOfUnit()
    {
        return numberOfUnits;
    }
    public void SetNumberOfUnit(int increment)
    {
        numberOfUnits += increment;
    }
    public bool ReturnAvailablePositionForUnit(ref Vector3 position)
    {
        if(numberOfUnits>=3)
        {
            numberOfUnits = 3;
            return false;
        }
        numberOfUnits += 1;
        position = transform.position + unitsPositions[numberOfUnits - 1];
        return true;
    }
    #endregion

}
