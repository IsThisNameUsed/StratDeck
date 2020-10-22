using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject mapGroup;
    [Range(5,12)]
    public int mapWidth;
    private List<TileForGeneration[]> map;
    private Germ[] germs;
    private TileLoader tileLoader;
    private int castleNumber;
    public enum strategic { castle,nothing };
    private GameObject targetingSpherePrefab;
    private PhotonView photonView;
    private List<int> mapColumnSize;

    void Start()
    {
        mapGroup = PhotonNetwork.InstantiateSceneObject("Prefabs/Map", new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), 0);
        mapGroup.name = "Map";

        photonView = GetComponent<PhotonView>();

        mapColumnSize = new List<int>();
    }

    public void CreateMap()
    {
        float mapSize = GameObject.Find("MapSize").GetComponent<Slider>().value;
        mapWidth = Mathf.RoundToInt(mapSize);
        if (mapWidth % 2 == 0) mapWidth = mapWidth + 1;
        castleNumber = numberOfGerm();
        tileLoader = new TileLoader();
        tileLoader.ConstructTileTab();

        int heightColumn = 3;
        mapColumnSize.Add(heightColumn);
        float positionX = -((mapWidth-1) / 2);
        float positionZ = -1;

        //Initialisation de la map
        map = new List<TileForGeneration[]>();
        for (int width = 0; width < mapWidth; width++)
        {
            map.Add(new TileForGeneration[heightColumn]);
            if (width <= mapWidth/2)
            {
                heightColumn += 1;
                mapColumnSize.Add(heightColumn);
            }
            else
            {
                heightColumn -= 1;
                mapColumnSize.Add(heightColumn);
            }
        }
        heightColumn = 3;

        for (int width = 0; width < mapWidth; width++)
        {
            for (int height = 0; height < heightColumn; height++)
            {
                Vector3 position = new Vector3(positionX, 0, positionZ + height);
                map[width][height] = new TileForGeneration();
                map[width][height].position = position;
                map[width][height].name = "Tile" + width + height;
                map[width][height].stratElt = strategic.nothing;
                map[width][height].neighbors = new List<TileForGeneration>();
                map[width][height].neighborsNames = new List<string>();
                map[width][height].gameObject = null;
            }
            positionX += 0.86f;
            if (positionX <= 0)
            {
                positionZ -= 0.5f;
                heightColumn += 1;
            }
            else
            {
                positionZ += 0.5f;
                heightColumn -= 1;
            }  
        }


        placeGerm();
        placeCastle();
        placeResources();
        Map.instance.InitializeMap(mapWidth);
        //Construction de la map
        positionX = -((mapWidth - 1) / 2);
        positionZ = -1;
        heightColumn = 3;
        for (int width=0; width < mapWidth; width++)
        {
            for(int height=0; height<heightColumn; height++)
            {
                map[width][height].style = belongsZone(map[width][height]);
                string path = tileLoader.ChooseTile(map[width][height]);

                string controller;
                //Tile attribution to players
                if ((width == 0 && height == 0) || (width == 0 && height == 1))
                    controller = "redPlayer";
                else if ((width == mapWidth - 1 && height == 0) || (width == mapWidth - 1 && height == 1))
                    controller = "bluePlayer";
                else controller = "neutral";

                DetermineNeighbors(map[width][height], width, height, heightColumn);

                //Préparation des datas pour initialisation des Tiles chez le client
                int sizeNeighborsList = map[width][height].neighbors.Count;
                string[] neighborsName = new string[sizeNeighborsList];
                for(int i=0; i< sizeNeighborsList; i++)
                {
                    neighborsName[i] = map[width][height].neighbors[i].name;
                }

                object[] tileInitDataForClient = new object[7 + neighborsName.Length];
                tileInitDataForClient[0] = map[width][height].style;
                tileInitDataForClient[1] = map[width][height].name;
                tileInitDataForClient[2] = map[width][height].stratElt.ToString();
                tileInitDataForClient[3] = controller;
                tileInitDataForClient[4] = map[width][height].foodIncome;
                tileInitDataForClient[5] = map[width][height].goldIncome;
                tileInitDataForClient[6] = map[width][height].strategicIncome;

                for(int i=7; i< 7 + neighborsName.Length; i++)
                {
                    tileInitDataForClient[i] = neighborsName[i-7];
                }
                
                map[width][height].gameObject = PhotonNetwork.InstantiateSceneObject(path, map[width][height].position, Quaternion.Euler(0, -30, 0), 0, tileInitDataForClient);

                map[width][height].gameObject.transform.parent = mapGroup.transform;
                map[width][height].gameObject.transform.localScale = new Vector3(1,1,1);
                map[width][height].gameObject.name = map[width][height].name;

                //Initialisation tiles coté serveur
                Tile tileScript = map[width][height].gameObject.GetComponent<Tile>();
                tileScript.neighboursName = new List<string>();
                foreach (string tilename in neighborsName)
                {
                    tileScript.neighboursName.Add(tilename);
                }               
                tileScript.InitializeTile(map[width][height].style, map[width][height].stratElt, map[width][height].foodIncome, 
                    map[width][height].goldIncome, map[width][height].strategicIncome);
                Map.instance.tileList.Add(tileScript);
            }

            positionX += 0.86f;
            if (positionX <= 0)
            {
                
                heightColumn += 1;
            }
            else
            {
                heightColumn -= 1;
            } 
        }

        Map.instance.InitializeTargetingSphere();

        foreach(Tile tile in Map.instance.tileList)
        {
            tile.CreateNeighboursList();
        }
    }

    public void DetermineNeighbors(TileForGeneration tile, int xTile, int yTile, int heighColumn)
    {
        int rightColumnHeigh, leftColumnHeigh;
        int partOfMap = -1; //0=left, 1=middle, 2=right
        if (xTile < (mapWidth - 1) / 2)
        {
            partOfMap = 0;
            rightColumnHeigh = heighColumn + 1;
            if (xTile > 0)
                leftColumnHeigh = heighColumn - 1;
            else leftColumnHeigh = -1;
        } 
        else if (xTile > (mapWidth - 1) / 2)
        {
            partOfMap = 2;
            leftColumnHeigh = heighColumn +1;
            if (xTile < mapWidth - 1)
                rightColumnHeigh = heighColumn - 1;
            else rightColumnHeigh = -1;
        }
        else
        {
            partOfMap=1;
            leftColumnHeigh = heighColumn -1;
            rightColumnHeigh = heighColumn - 1;
        }    
       
        int x = xTile - 1;
        int yMin, yMax;
        if(x >= 0)
        {
            if(partOfMap==0 || partOfMap == 1)
            {
                yMin = yTile - 1;
                yMax = yTile;
            }
            else
            {
                yMin = yTile;
                yMax = yTile + 1;
            }

            if (yMax <= leftColumnHeigh-1)
            {
                tile.neighbors.Add(map[x][yMax]);
                tile.neighborsNames.Add(map[x][yMax].name);
            }
            if (yMin >= 0)
            {
                tile.neighbors.Add(map[x][yMin]);
                tile.neighborsNames.Add(map[x][yMin].name);
            }
               
        }

        x = xTile;
        if(yTile + 1 <= heighColumn - 1)
        {
            tile.neighbors.Add(map[x][yTile + 1]);
            tile.neighborsNames.Add(map[x][yTile + 1].name);
        }
           
        if (yTile - 1 >= 0)
        {
            tile.neighbors.Add(map[x][yTile - 1]);
            tile.neighborsNames.Add(map[x][yTile - 1].name);
        }
            
        x = xTile + 1;
        if(x <= mapWidth - 1)
        {
            if(partOfMap == 2 || partOfMap == 1)
            {
                yMin = yTile-1;
                yMax = yTile;
            }
            else
            {
                yMin = yTile;
                yMax = yTile + 1;
            }
            if (yMax <= rightColumnHeigh-1)
            {
                tile.neighbors.Add(map[x][yMax]);
                tile.neighborsNames.Add(map[x][yMax].name);
            }
                
            if (yMin >= 0)
            {
                tile.neighbors.Add(map[x][yMin]);
                tile.neighborsNames.Add(map[x][yMin].name);
            }
               
        }
    }

    //Reparti les chateaux sur la map selon le style des tiles 
    private void placeCastle()
    {
        map[0][0].stratElt = strategic.castle;
        map[mapWidth - 1][0].stratElt = strategic.castle;
        int index = 2;
        int xPosition=0;
        int yPosition=0;

        while (index<castleNumber)
        {
            if(index == 2)
            {
                xPosition = (mapWidth - 1)/2;
                int yRange = map[xPosition].Length - 1;
                yPosition = Random.Range(0, yRange);
            }
            else if(index == 3)
            {
                xPosition = Random.Range((mapWidth - 1) / 2, mapWidth-1);
                int yRange = map[xPosition].Length - 1;
                yPosition = Random.Range(0, yRange);
            }
            else if(index == 4)
            {
                xPosition = Random.Range(0, mapWidth-1);
                int yRange = map[xPosition].Length - 1;
                yPosition = Random.Range((yRange) /2, yRange);
            }
            else if (index == 5)
            {
                xPosition = Random.Range(0, mapWidth-1);
                int yRange = map[xPosition].Length - 1;
                yPosition = Random.Range(0 ,(yRange) /2);
            }
            else if (index == 6)
            {
                xPosition = (mapWidth - 1) / 2;
                int yRange = map[xPosition].Length - 1;
                yPosition = Random.Range(0, yRange);
            }

            Debug.Log("index =" + index + "xPosition/yPosition=" +xPosition +"/" +yPosition);
            if (map[xPosition][yPosition].stratElt != strategic.castle)
            {
                map[xPosition][yPosition].stratElt = strategic.castle;
                index++;
            }      
        } 
    }

    private void placeResources()
    {
        for (int width = 0; width < mapWidth; width++)
        {
            for (int height = 0; height < mapColumnSize[width]; height++)
            {
                map[width][height].foodIncome = Random.Range(0, 3); ;
                map[width][height].goldIncome = Random.Range(0, 3); ;
                map[width][height].strategicIncome = Random.Range(0, 3); ;   
            }
        }

        //Ressources des zones de départ;
        map[0][0].foodIncome = 3;
        map[0][0].goldIncome = 3;
        map[0][1].foodIncome = 2;
        map[0][1].goldIncome = 1;

        map[mapWidth - 1][0].foodIncome = 3;
        map[mapWidth - 1][0].goldIncome = 3;
        map[mapWidth - 1][1].foodIncome = 2;
        map[mapWidth - 1][1].goldIncome = 1;
    }

    private int numberOfGerm()
    {
        if (mapWidth < 7)
            return 3;
        else if (mapWidth < 10)
            return 4;
        else if (mapWidth < 11)
            return 5;  
         else return 6;
    }

    //germIndex[0] = numero de colonne(ou de list dans map) et germIndex[1] = numero de ligne (ou de Tile dans map)
    //place les "germes" décidant du style de paysage d'une région
    private void placeGerm()
    {
        int[,] germIndex = new int[numberOfGerm(),2];
        germs = new Germ[numberOfGerm()];
        //Debug.Log(numberOfGerm()+" germes");
        Random random = new Random();
        bool ok=false;
        for(int index=0;index< numberOfGerm(); index ++)
        {
            while(!ok)
            {
                ok = true;
                germIndex[index, 0] = Random.Range(0, mapWidth - 1);
                germIndex[index, 1] = Random.Range(0, map[germIndex[index, 0]].Length - 1);
                germs[index].position = map[germIndex[index, 0]][germIndex[index, 1]].position;
                for(int k = 0; k<index; k++)
                {
                    if(germIndex[index, 0] == germIndex[k, 0] && germIndex[index, 1] == germIndex[k, 1])
                    {
                        ok = false;
                        break;
                    }
                }
            }
            ok = false;
        }

        //Assign a style of Tile to a germ
        for(int i=0; i < numberOfGerm(); i++)
        {
            germs[i].style = Random.Range(0, 4);
        }

        for(int index = 0; index <= numberOfGerm()-1; index++)
        {
            //Debug.Log("style " + germs[index].style);
        }
    }

    //calcul le germe le plus proche de la position de la tile
    public int belongsZone(TileForGeneration tile)
    {
        float distanceMin = 1000;
        int zone =1000;
        for(int i=0;i< germs.Length;i++)
        {
            float distance = Vector3.Distance(germs[i].position, tile.position);
            //Debug.Log(tile.name+" "+tile.position + "to germ "+ i +" "+germs[i].position+" = " + distance + "compar" + distanceMin);
            if (distance < distanceMin)
            {
                distanceMin = distance;
                zone = germs[i].style;
            }     
        }
        return zone;
    }

    /*
        if(Application.isPlaying)
        {
            foreach (Tile[] tileTab in map)
            {
                for (int i = 0; i < tileTab.Length; i++)
                {
                    Gizmos.color = Color.red;
                    Vector3 gizPosition = new Vector3(tileTab[i].position.x, tileTab[i].position.y + 0.2f, tileTab[i].position.z);
                    Gizmos.DrawSphere(gizPosition, 0.1f);
                }
            }

            for (int i = 0; i < germs.Length; i++)
            {
                if (i == 0) Gizmos.color = Color.yellow;
                else if (i == 1) Gizmos.color = Color.green;
                else if(i==2)Gizmos.color = Color.black;
                else if (i == 3) Gizmos.color = Color.blue;
                else if (i == 4) Gizmos.color = Color.white;
                
                Vector3 gizPosition = germs[i].position;
                gizPosition = new Vector3(gizPosition.x, gizPosition.y + 0.2f, gizPosition.z);
                Gizmos.DrawSphere(gizPosition, 0.2f);
            }
        }
    }*/

    void OnApplicationQuit()
    {
        Debug.Log("appQuit");
    }
}

public struct Germ
{
    public Vector3 position;
    public int style;
}

public struct TileForGeneration // make a class?
{
    public List<TileForGeneration> neighbors;
    public List<string> neighborsNames;
    public GameObject gameObject;
    public Vector3 position;
    public string name;
    public int style;
    public MapGenerator.strategic stratElt;
    public int foodIncome;
    public int goldIncome;
    public int strategicIncome;

}



