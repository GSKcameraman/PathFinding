using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Linq;


public class Map : MonoBehaviour
{
    public List<string> files;
    TileGrid grid;
    GraphGrid graphgrid;

    [Dropdown("files")]
    public string filename;


    public Camera cam;

    public int height;
    public int width;

    public GameObject tileGround;
    public GameObject tileWall;
    public GameObject tileTree;
    public GameObject graphEmpty;
    public GameObject graphClosed;
    public GameObject graphOpen;
    public GameObject graphPath;

    public TMP_Dropdown dropdown;


    [Space]
    [Range(0,100)]
    public int heturisticsWeight = 50;
    [Range(0, 100)]
    public int gWeight = 50;

    int distance;

    public bool Manhattan = false;

    int startx;
    int starty;
    int endx;
    int endy;

    int graphWidth;
    int graphHeight;

    

    char[,] mapstore;

    List<PathTile> closed = new List<PathTile>();
    List<PathTile> open = new List<PathTile>();

    public Vector2Int start;
    public Vector2Int end;
    public Vector2Int current;
    public bool Started = false;
    bool work = false;


    AudioSource source;


    // Start is called before the first frame update
    void Start()
    {
        ReadRoutine();
        //Graphing();

        //graphgrid.SwitchTileStatus(0, 0, PathTile.status.CLOSED);
        //graphgrid.SwitchTileStatus(Mathf.FloorToInt(height * 0.5f), width / 2, PathTile.status.OPEN);

        Debug.Log(graphgrid.GetTileWeight(0, 0));

        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Started && !work)
        {
            work = true;
            Started = false;
            StartCoroutine(AStarTile());
            
        }
    }

    public void readfile()
    {
        StreamReader reader = new StreamReader(filename);

        //octtile, we dont' need to know that (same for all)
        reader.ReadLine();

        //height
        string heightline = reader.ReadLine();
        string[] heightsplit = heightline.Split(' ');
        height = int.Parse(heightsplit[1]);
        //width
        string widthline = reader.ReadLine();
        string[] widthsplit = widthline.Split(' ');
        width = int.Parse(widthsplit[1]);

        if (grid == null)
        {
            grid = new TileGrid(width, height, 10f, 1f, Vector3.zero);

        }
        else
        {
            grid.Reload(width, height);
        }
        mapstore = new char[width,height];
        //map (the line)
        reader.ReadLine() ;
        //rest of the file
        for (int y = 0; y < height; y++)
        {
            string line = reader.ReadLine();
            
            for(int x = 0; x < width; x++)
            {
                char c = line[x];
                grid.SetValue(x, y, c);
                mapstore[x, y] = c;
            }
        }

        //grid.PrintValue();
        reader.Close();
    }

    void ReadRoutine()
    {
        readfile();
        grid.tileGround = tileGround;
        grid.tileWall = tileWall;
        grid.tileTree = tileTree;
        grid.CreateTiles();
        //Debug.Log(mapstore[0]);
        cam.transform.position = new Vector3(width / 2, height / 2, -20);
        cam.orthographicSize = Mathf.Max(height, width / 1.6f) / 2.0f;
        Graphing();
    }

    void Graphing()
    {
        graphWidth = Mathf.CeilToInt(width * 0.5f);
        graphHeight = Mathf.CeilToInt(height * 0.5f);
        if (graphgrid != null)
        {
            
            graphgrid.Reload(graphWidth, graphHeight);
        }
        else
        {
            graphgrid = new GraphGrid(graphWidth, graphHeight, 9.0f, 2f, Vector3.zero);

        }

        graphgrid.unreadTile = graphEmpty;
        graphgrid.openTile = graphOpen;
        graphgrid.closedTile = graphClosed;
        graphgrid.pathTile = graphPath;
        for (int x  =0; x < graphWidth; x++)
        {
            for (int y =0; y < graphHeight; y++)
            {
                int weight = 5;
                if (mapstore[x * 2,y * 2] == '.')
                {
                    weight--;
                }
                if (x * 2 < width - 1)
                {
                    if (y * 2 < height - 1)
                    {
                        if (mapstore[x * 2 + 1, y * 2 + 1] == '.')
                        {
                            weight--;
                        }
                    }
                    if (mapstore[x * 2 + 1, y * 2] == '.')
                    {
                        weight--;
                    }
                }
                if (y * 2 > height - 1)
                {
                    if (mapstore[x * 2, y * 2 + 1] == '.')
                    {
                        weight--;
                    }

                }

                graphgrid.CreateGraphTile(x, y, weight);
            }
        }
    }


    public void SwitchMapFile()
    {
        filename = files[dropdown.value];
        grid.ClearGraph();
        graphgrid.ClearGraph();
        ReadRoutine();
    }

    public int EuclideanDistance(int x1, int y1, int x2, int y2)
    {
        return Mathf.RoundToInt(Mathf.Sqrt(((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1))));
    }

    public int ManhattanDistance(int x1, int y1, int x2, int y2)
    {
        return Mathf.Abs(y2 - y1) + Mathf.Abs(x2 - x1);
    }


    public void SetStart(int x, int y)
    {
        startx = x;
        starty = y;
    }

    public void SetEnd(int x, int y)
    {
        endx = x; endy = y;
    }

    public void StartAStar()
    {
        PathTile st = graphgrid.GetTile(start.x, start.y);
        st.g = st.getWeight();
        if (Manhattan)
        {
            st.h = ManhattanDistance(start.x, start.y, end.x, end.y);
        }
        else
        {
            st.h = EuclideanDistance(start.x, start.y, end.x, end.y);
        }

        distance = st.h;
        st.f = heturisticsWeight * st.h + gWeight * st.g;
        open.Add(st);
        Started = true;
    }

    

    IEnumerator AStarTile()
    {

        while (open.Count > 0 && work)
        {
            PathTile tile = open.First();
            open.RemoveAt(0);

            int x = tile.getX();
            int y = tile.getY();

            Debug.Log(x + " " + y);
            Debug.Log("f: " + tile.f);

            if (x == end.x && y == end.y)
            {
                work = false;
                
                PaintPath(tile);
                StopCoroutine(AStarTile());
            }

            PathTile[] t = new PathTile[8];

            t[0] = graphgrid.GetTile(x + 1, y + 1);
            t[1] = graphgrid.GetTile(x + 1, y);
            t[2] = graphgrid.GetTile(x, y + 1);
            t[3] = graphgrid.GetTile(x + 1, y - 1);
            t[4] = graphgrid.GetTile(x - 1, y - 1);
            t[5] = graphgrid.GetTile(x - 1, y);
            t[6] = graphgrid.GetTile(x, y - 1);
            t[7] = graphgrid.GetTile(x - 1, y + 1);

            for (int i = 0;  i < 8; i++)
            {
                if (closed.Contains(t[i]) || t[i] == null)
                {
                    continue;
                }
                else if (open.Contains(t[i]))
                {
                    int ng = tile.g + t[i].getWeight();
                    int h = t[i].h;
                    int f1 = ng * gWeight + h * heturisticsWeight;
                    if (f1 < t[i].f)
                    {
                        t[i].f = f1;
                        t[i].g = ng;
                        t[i].parent = tile;
                    }
                }
                else if (t[i].getWeight() >= 5)
                {
                    continue;
                }
                else
                {
                    int h;
                    if (Manhattan)
                    {
                        h = ManhattanDistance(t[i].getX(), t[i].getY(), end.x, end.y);
                    }
                    else
                    {
                        h = EuclideanDistance(t[i].getX(), t[i].getY(), end.x, end.y);

                    }


                    int ng = tile.g + t[i].getWeight();
                    t[i].g = ng;
                    t[i].h = h;
                    int f1 = ng * gWeight + h * heturisticsWeight;
                    t[i].f = f1;

                    t[i].parent = tile;

                    open.Add(t[i]);


                    graphgrid.SwitchTileStatus(t[i].getX(), t[i].getY(), PathTile.status.OPEN);

                }
            }
            open = open.OrderBy(x => x.f).ToList();
            closed.Add(tile);
            graphgrid.SwitchTileStatus(x, y, PathTile.status.CLOSED);
            source.Stop();
            source.pitch = 0.2f + (distance - tile.h)* 1.0f / distance * 0.8f;
            source.Play();
            yield return new WaitForSeconds(.1f);

        }
        
    }

    public void PaintPath(PathTile t)
    {
        PathTile t1 = t;
        while (t1 != null)
        {
            graphgrid.SwitchTileStatus(t1.x, t1.y, PathTile.status.PATH);
            t1 = t1.parent;
        }
    }
}
