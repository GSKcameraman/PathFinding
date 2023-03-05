using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTile
{
    public PathTile parent;
    public  int x;
    public  int y;
    public  int weight;
    public int g;
    public int h;
    public status s = status.UNREAD;
    public enum status { CLOSED, OPEN, UNREAD, PATH}

    public int f;

    public PathTile(int x1, int y1, int w)
    {
        parent = null;
        weight = w;
        x = x1;
        y = y1;
        g = int.MaxValue;
        h = int.MaxValue;
        s = status.UNREAD;
    }


    public int getWeight()
    {
        return weight;
    }


    public int getX()
    {
        return x;
    }

    public int getY()
    {
        return y;
    }

    public int getF()
    {
        return f;
    }

    public int calculateF(int wg, int wh)
    {
        f = g * wg + h * wh;
        return f;
    }

    public void setF(int f)
    {
        this.f = f;
    }

    public void setG(int g)
    {
        this.g = g + weight;
    }

    
}
