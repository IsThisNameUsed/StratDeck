using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileLoader
{
    private GameObject[] desertObject;
    private GameObject[] greenObject;
    private GameObject[] evilObject;
    private GameObject[] snowObject;
    public GameObject[] castle;

    public void ConstructTileTab()
    {
        Debug.Log("Construct");
        desertObject = Resources.LoadAll<GameObject>("Tile/desertLand");
        greenObject = Resources.LoadAll<GameObject>("Tile/greenLand");
        evilObject = Resources.LoadAll<GameObject>("Tile/evilLand");
        snowObject = Resources.LoadAll<GameObject>("Tile/snowLand");
        castle = Resources.LoadAll<GameObject>("Tile/castle");

        for (int i = 0; i < greenObject.Length; i++)
        {
            
            //Debug.Log(greenObject[i].name);
        }

        for (int i=0;i< desertObject.Length;i++)
        {
            //Debug.Log("tour");
            //Debug.Log(desertObject[i].name);
        }
    }

    public string CastleLoader(string name)
    {
        return ("Tile/castle/" + name);
    }

    private string ChooseCastle(TileForGeneration tile)
    {
        string castle = "";
        switch (tile.style)
        {
            case 0:
                castle = "desertCastle";
                break;
            case 1:
                castle = "greenCastle";
                break;
            case 2:
                castle = "hellCastle";
                break;
            case 3:
                castle = "snowCastle";
                break;
        }
        return CastleLoader(castle);
    }

    private string ChooseMiscTile(TileForGeneration tile)
    {
        int rand = 0;
        string path = null;
        switch (tile.style)
        {
            case 0:
                rand = Random.Range(0, desertObject.Length);
                path = "Tile/desertLand/"+desertObject[rand].name;
                break;
            case 1:
                rand = Random.Range(0, greenObject.Length);
                path = "Tile/greenLand/" + greenObject[rand].name;
                break;
            case 2:
                rand = Random.Range(0, evilObject.Length);
                path = "Tile/evilLand/" + evilObject[rand].name;
                break;
            case 3:
                rand = Random.Range(0, snowObject.Length);
                path = "Tile/snowLand/" + snowObject[rand].name;
                break;
        }
        return path;
    }
    

    public string ChooseTile(TileForGeneration tile)
    {
        string path = null;
        switch (tile.stratElt)
        {
            case MapGenerator.strategic.castle:
                path = ChooseCastle(tile);
                break;
            case MapGenerator.strategic.nothing:
                path = ChooseMiscTile(tile);
                break;
        }
        return path;

    }
}
