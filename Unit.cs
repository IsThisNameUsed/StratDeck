using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public string tileNumber;

    private int moveCapacity;
    [SerializeField]
    private UnitManager.unitType unitType;

    public Material mat;
    
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        tileNumber = (string)instantiationData[0];
    }

    void Start()
    {
        if (unitType == UnitManager.unitType.infantryman)
            InitializeAsInfantryman();
        else if(unitType == UnitManager.unitType.bowman)
            InitializeAsBowman();
        else if (unitType == UnitManager.unitType.horseman)
            InitializeAsHorseman();

        mat = gameObject.GetComponent<MeshRenderer>().material;
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 500))
            {
                MeshRenderer meshRenderer = hit.transform.GetComponent<MeshRenderer>();
                if (meshRenderer != null && hit.collider.transform.tag == "Unit")
                {

                }
            }
        }
    }

    public void MoveTo(Tile targetedTile)
    {
        Tile startTile = Map.instance.FindTile(tileNumber);
        Vector3 newPosition = new Vector3();
        targetedTile.ReturnAvailablePositionForUnit(ref newPosition);
        this.transform.position = newPosition;

        tileNumber = targetedTile.ReturnTileNumber();
        targetedTile.SetController(MatchManager.instance.localPlayer);

        SetUnitColor(UnitManager.instance.ReturnUnitColor(false));
    }

    public bool MoveToAuthorized(Tile targetedTile)
    {
        Tile startTile = Map.instance.FindTile(tileNumber);
        bool moveAuthorized = Map.instance.isTileAccessible(targetedTile, startTile, moveCapacity);
        if (moveAuthorized)
        {
            return true;
        }
        else return false;
    }

    public void SetUnitColor(Color newColor)
    {
        mat.color = newColor;
    }
    private void InitializeAsHorseman()
    {
        moveCapacity = 2;
    }

    private void InitializeAsInfantryman()
    {
        moveCapacity = 1;
    }

    private void InitializeAsBowman()
    {
        moveCapacity = 1;
    }

    public void SetTileNumber(string tileNumber)
    {
        this.tileNumber = tileNumber;
    }
}
