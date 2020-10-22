using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static Map instance;

    public int mapWidth;

    public List<Tile> tileList;
    PhotonView photonView;
    GameObject targetingSpherePrefab;

    public void Start()
    {
        if (instance == null)
            instance = this;
        else Destroy(this);
    }

    public void InitializeMap(int mapWidth)
    {
        this.mapWidth = mapWidth;
        tileList = new List<Tile>();
        this.gameObject.AddComponent<PhotonView>();
    }

    public void InitializeMapForClient()
    {
        GameObject[] Tiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach(GameObject tile in Tiles)
        {
            Tile tileScript = tile.GetComponent<Tile>();
            if(tileScript != null)
            {
                tileList.Add(tileScript);
                string tileName = tile.name;
                tileName.EndsWith("Castle(clone)");
                //InitializeTile(int _style, MapGenerator.strategic _stratElt)
            }       
        }

        //On rempli la liste des voisins pour chaque tile
        foreach(Tile tile in tileList)
        {
            tile.CreateNeighboursList();
        }

    }

    public void InitializeTargetingSphere()
    {
        targetingSpherePrefab = Instantiate(Resources.Load("TargetingSphere", typeof(GameObject)) as GameObject);
        foreach (Tile tile in tileList)
        {
            tile.CreateTargetingSphere(targetingSpherePrefab);
        }
        
    }

    public string tileNumberToName(string tileNumber)
    {
        return "Tile" + tileNumber;
    }
    public string tileNameToNumber(string tileName)
    {
        return tileName.Substring(4, 2);
    }
    public Tile FindTile(string number)
    {
        string tileName = "Tile" + number.ToString();
        foreach (Tile tile in tileList)
        {
            if (tileName == tile.tileName)
                return tile;
        }
        return null;
    }

    public bool isTileAccessible(Tile targetedTile, Tile startTile, int moveCapacity)
    {
        if(moveCapacity == 1)
        {
            if (startTile.neighboursList.Contains(targetedTile))
                return true;
        }
        else if(moveCapacity == 2)
        {
            List<Tile> accessibleTilesRange2 = new List<Tile>();
            foreach (Tile tile in startTile.neighboursList)
            {
                foreach (Tile tileRange2 in tile.neighboursList)
                {
                    if (!accessibleTilesRange2.Contains(tileRange2))
                        accessibleTilesRange2.Add(tileRange2);
                }
            }
            if (accessibleTilesRange2.Contains(targetedTile))
                return true;
        }

        return false;
    }

}
