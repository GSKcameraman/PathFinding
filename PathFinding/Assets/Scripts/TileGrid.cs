using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileGrid : Grid<char>
{

    public enum tile{ TILE1, TILE2,TILE3 };
    public GameObject tileGround;
    public GameObject tileWall;
    public GameObject tileTree;
    public float z = 0f;

    protected GameObject[,] tiles;
    public TileGrid(int w, int h,float z, float size, Vector3 origin) : base(w, h, size, origin)
    {
        width = w;
        height = h;
        cellsize = size;
        this.z = z;
        gridArray = new char[w, h];
        Debug.Log("new grid " + width + " " + height);
        this.origin = origin;
        tiles = new GameObject[w, h];
    }

    public void CreateTile(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            char type = gridArray[x, y];
            GameObject targettile;
            if (type == '.')
            {
                targettile = tileGround;
                if (tiles[x, y] == null)
                {
                    tiles[x, y] = (GameObject)Instantiate(targettile, new Vector3(x * cellsize + cellsize / 2, (y * cellsize + cellsize / 2), z + 1) + origin, Quaternion.identity);

                }
                return;
            }
            else if (type == 'T')
            {
                targettile = tileTree;
            }
            else
            {
                targettile = tileWall;
            }

            if (tiles[x,y] == null)
            {
                tiles[x, y] = (GameObject)Instantiate(targettile, new Vector3(x * cellsize + cellsize / 2, (y * cellsize + cellsize / 2), z) + origin, Quaternion.identity);

            }

        }


    }

    public void CreateTiles()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y =0; y < height; y++)
            {
                CreateTile(x, y);
            }
        }
    }

    public void PrintValue()
    {
        string s = "";
        for (int y = 0; y < height; y++)
        {
            for (int x = 0;x < width; x++)
            {
                s += gridArray[x, y];

            }
            s += "\n";
        }

        Debug.Log(s);
    }

    public void ClearGraph()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                DestroyImmediate(tiles[x, y]);
                
                
            }
            
        }

    }

    public void Reload(int w, int h)
    {
        ClearGraph();


        width = w;
        height = h;
        gridArray = new char[w, h];
        Debug.Log("new grid " + width + " " + height);
        
        tiles = new GameObject[w, h];

    }
    
    public Vector3 GetWorldPos(int x, int y)
    {
        return new Vector3(x * cellsize + cellsize / 2, (y * cellsize + cellsize / 2), z - 3) + origin;
    }

    public Vector2Int WorldPosToGraph(Vector3 pos)
    {
        Vector3 pos1 = pos - origin;
        return new Vector2Int(Mathf.FloorToInt(pos1.x * 1.0f / cellsize), Mathf.FloorToInt(pos1.y * 1.0f / cellsize));
    }


    ~TileGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Destroy(tiles[x, y]);
            }
        }

    }
}
