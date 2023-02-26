using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Grid<T> : MonoBehaviour 
{
    protected int width;
    protected int height;
    protected T[,] gridArray;

    public float cellsize = 1;
    protected Vector3 origin = Vector3.zero;


    public Grid(int w, int h, float size, Vector3 origin)
    {
        width = w;
        height = h;
        cellsize = size;
        gridArray = new T[w, h];
        Debug.Log("new grid " + width + " " + height);
        this.origin = origin;
    }

    public void SetValue(int x, int y, T value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
        }
    }

    protected T GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(T);
        }

    }

    protected Vector3 ToWorldPos(int x, int y)
    {
        return new Vector3(x, y) * cellsize + origin;
    }

    protected Vector2Int Converpos(Vector3 position)
    {
        Vector2Int v = new Vector2Int();
        v.x = Mathf.FloorToInt((position - origin).x/cellsize);
        v.y = Mathf.FloorToInt((position - origin).y/cellsize);
        return v;
    }

    public void SetValuePos(Vector3 position, T value)
    {
        Vector2Int v = Converpos(position);
        SetValue(v.x, v.y, value);
    }
}
