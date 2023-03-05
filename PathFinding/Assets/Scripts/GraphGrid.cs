using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphGrid : Grid<PathTile>
{
    public GameObject unreadTile;
    public GameObject closedTile;
    public GameObject openTile;
    public GameObject pathTile;

    public float z = 9.0f;

    public GameObject[,] show;
    public GraphGrid(int w, int h, float z, float size, Vector3 origin) : base(w, h, size, origin)
    {
        show = new GameObject[w, h];
        this.z = z;

        Debug.Log("new graphgrid " + width +", " + height);
    }

    public void CreateGraphTile(int x, int y, int w)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {

            gridArray[x, y] = new PathTile(x, y, w);
            show[x, y] = Instantiate(unreadTile, new Vector3(x * cellsize + cellsize / 2, (y * cellsize + cellsize / 2), z) + origin, Quaternion.identity);

        }
    }

    public void ClearGraph()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                DestroyImmediate(show[x, y]);


            }

        }

    }

    public void Reload(int w, int h)
    {
        ClearGraph();


        width = w;
        height = h;
        gridArray = new PathTile[w, h];
        Debug.Log("new graph grid " + width + " " + height);

        show = new GameObject[w, h];

    }



    public void SwitchTileStatus(int x, int y, PathTile.status status)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x,y].s = status;
            Destroy(show[x, y]);
            if (gridArray[x, y].s == PathTile.status.CLOSED)
            {
                show[x, y] = Instantiate(closedTile, new Vector3(x * cellsize + cellsize / 2, (y * cellsize + cellsize / 2), z) + origin, Quaternion.identity);

            }
            else if (gridArray[x, y].s == PathTile.status.OPEN)
            {
                show[x, y] = Instantiate(openTile, new Vector3(x * cellsize + cellsize / 2, (y * cellsize + cellsize / 2), z) + origin, Quaternion.identity);

            }
            else if (gridArray[x,y].s == PathTile.status.PATH)
            {
                show[x, y] = Instantiate(pathTile, new Vector3(x * cellsize + cellsize / 2, (y * cellsize + cellsize / 2), z) + origin, Quaternion.identity);

            }
            else
            {
                show[x, y] = Instantiate(unreadTile, new Vector3(x * cellsize + cellsize / 2, (y * cellsize + cellsize / 2), z) + origin, Quaternion.identity);

            }
        }
    }

    public PathTile GetTile(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        return null;

    }

    public int GetTileWeight(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y].getWeight();
        }
        return int.MaxValue;
    }

    public int GetTileH(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y].h;
        }
        return int.MaxValue;
    }

    public void SetTileH(int x, int y, int h1)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y].h = h1;
        }

    }



    public int GetTileG(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y].g;
        }
        return int.MaxValue;
    }
    public void SetTileG(int x, int y, int g1)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y].setG(g1);
        }
        
    }

    public int GetTileF(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y].f;
        }
        return int.MaxValue;

    }

    public void SetTileF(int x, int y, int wg, int wh)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            int f1 = gridArray[x, y].calculateF(wg, wh);

            gridArray[x, y].setF(f1);
        }
        

    }

}
