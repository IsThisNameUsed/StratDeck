using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager instance;
    public enum unitType { infantryman, bowman, horseman };

    private Color redUnitSelected = new Color(0.8f, 0.5f, 0.4f, 1);
    private Color redUnit = new Color(0.7f, 0, 0, 1);
    private Color blueUnitSelected = new Color(0.4f, 0.4f, 0.9f, 1);
    private Color blueUnit = new Color(0, 0, 1, 1);

    void Awake()
    {
        if (instance == null)
            instance = this;
        else Destroy(this);
    }

    public Color ReturnUnitColor(bool selected)
    {
        if(MatchManager.instance.localPlayer == "redPlayer")
        {
            if (selected) return redUnitSelected;
            else return redUnit;
        }
        else
        {
            if (selected) return blueUnitSelected;
            else return blueUnit;
        }
       
    }
    public void InstantiateUnit(unitType type, string tileNumber, string controller)
    {
        string prefabPath = "Prefabs/";
        switch(type)
        {
            case unitType.bowman:
                    prefabPath += "bowman";
                break;
            case unitType.horseman:
                    prefabPath += "horseman";
                break;
            case unitType.infantryman:
                    prefabPath += "infantryman";  
                break;
            default: prefabPath = "";
                break;
        }

        if (prefabPath == "")
            return;

        if (controller == "redPlayer")
            prefabPath += "_red";
        else prefabPath += "_blue";

        Tile targetedTile = Map.instance.FindTile(tileNumber);
        Vector3 position = new Vector3();
        if(targetedTile.ReturnAvailablePositionForUnit(ref position))
        {
            object[] instanciationData = new object[1];
            instanciationData[0] = targetedTile.ReturnTileNumber();
            GameObject unitObj = Photon.Pun.PhotonNetwork.Instantiate(prefabPath, position, Quaternion.identity, 0, instanciationData);
            unitObj.GetComponent<Unit>().SetTileNumber(tileNumber);
        }
        
    }
}
