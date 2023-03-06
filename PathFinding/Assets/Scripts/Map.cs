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
using UnityEngine.Audio;

public class Map : MonoBehaviour
{
    public List<string> files;
    TileGrid grid;
    GraphGrid graphgrid;
    LinkedList<GameObject> Nodes;

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

    public GameObject graphStart;
    public GameObject graphEnd;

    public GameObject Node;

    GameObject instantStart = null;
    GameObject instantEnd = null;

    public TMP_Dropdown dropdown;

    public Button button;

    [Space]
    [Range(0, 100)]
    public int heturisticsWeight = 50;
    public Slider heturisticsSlider;
    public TMP_Text htext;
    [Range(0, 100)]
    public int gWeight = 50;
    public Slider gSlider;
    public TMP_Text gtext;

    [Space]
    int distance;

    public bool Manhattan = false;

    public bool wayPoint = false;

    public TMP_Text textType;
    public TMP_Text aStarText;

    int graphWidth;
    int graphHeight;
    [Space]
    public bool setStart = false;
    public bool setEnd = false;

    char[,] mapstore;

    List<PathTile> open = new List<PathTile>();

    List<GameObject> openNodes = new List<GameObject>();

    [Space]
    public Vector2Int start;
    public Vector2Int end;
    public Vector2Int current;
    public bool Started = false;
    bool work = false;
    bool graphed = false;
    bool waypointed = false;

    AudioSource source;
    public AudioMixer mixer;

    public AudioSource buzzer;
    public AudioSource click;
    // Start is called before the first frame update
    void Start()
    {
        Nodes = new LinkedList<GameObject>();
        ReadRoutine();
        //Graphing();

        //graphgrid.SwitchTileStatus(0, 0, PathTile.status.CLOSED);
        //graphgrid.SwitchTileStatus(Mathf.FloorToInt(height * 0.5f), width / 2, PathTile.status.OPEN);

        //Debug.Log(graphgrid.GetTileWeight(0, 0));

        source = GetComponent<AudioSource>();

        SetHValue();
        SetGValue();
    }

    // Update is called once per frame
    void Update()
    {
        if (setStart && Input.GetMouseButtonDown(0))
        {
            SetStartPos();
            setStart = false;
        }
        if (setEnd && Input.GetMouseButtonDown(0))
        {
            SetEndPos();
            setEnd = false;
        }

        if (!graphed || !waypointed)
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
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
        mapstore = new char[width, height];
        //map (the line)
        reader.ReadLine();
        //rest of the file
        for (int y = 0; y < height; y++)
        {
            string line = reader.ReadLine();

            for (int x = 0; x < width; x++)
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
        StartCoroutine(Graphing());
        StartCoroutine(WayPoint());
    }

    IEnumerator Graphing()
    {
        graphed = false;
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
        for (int x = 0; x < graphWidth; x++)
        {
            for (int y = 0; y < graphHeight; y++)
            {
                int weight = 5;
                if (mapstore[x * 2, y * 2] == '.')
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
                if (y * 2 < height - 1)
                {
                    if (mapstore[x * 2, y * 2 + 1] == '.')
                    {
                        weight--;
                    }

                }

                graphgrid.CreateGraphTile(x, y, weight);
                yield return null;
                
                buzzer.pitch = 0.5f + 0.5f * (x * 0.5f / width + y * 0.5f / height);
                if (!buzzer.isPlaying)
                {
                    buzzer.Play();

                }

            }
        }
        graphed = true;
        buzzer.Stop();
    }


    IEnumerator WayPoint()
    {
        waypointed = false;
        if (Nodes.Count > 0)
        {

            foreach (GameObject n in Nodes)
            {
                Destroy(n);
            }

        }
        Nodes.Clear();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapstore[x, y] == '.')
                {
                    bool[] inPassible = new bool[8];
                    /*
                     * like this:
                     * 0 | 1 | 2
                     * 3 | t | 4
                     * 5 | 6 | 7
                     */
                    inPassible[0] = (ReadTile(x - 1, y + 1) != '.');
                    inPassible[1] = (ReadTile(x, y + 1) != '.');
                    inPassible[2] = (ReadTile(x + 1, y + 1) != '.');
                    inPassible[3] = (ReadTile(x - 1, y) != '.');
                    inPassible[4] = (ReadTile(x + 1, y) != '.');
                    inPassible[5] = (ReadTile(x - 1, y - 1) != '.');
                    inPassible[6] = (ReadTile(x, y - 1) != '.');
                    inPassible[7] = (ReadTile(x + 1, y - 1) != '.');

                    if ((inPassible[0] && !inPassible[1] && !inPassible[2] && !inPassible[3] && !inPassible[5]) ||
                        (inPassible[2] && !inPassible[1] && !inPassible[0] && !inPassible[4] && !inPassible[7]) ||
                        (inPassible[5] && !inPassible[3] && !inPassible[0] && !inPassible[6] && !inPassible[7]) ||
                        (inPassible[7] && !inPassible[4] && !inPassible[2] && !inPassible[6] && !inPassible[5]) ||
                        (inPassible[0] && inPassible[1] && inPassible[2] && inPassible[3] && inPassible[5]) ||
                        (inPassible[2] && inPassible[1] && inPassible[0] && inPassible[4] && inPassible[7]) ||
                        (inPassible[5] && inPassible[3] && inPassible[0] && inPassible[6] && inPassible[7]) ||
                        (inPassible[7] && inPassible[4] && inPassible[2] && inPassible[6] && inPassible[7])
                        )
                    {
                        GameObject n1 = Instantiate(Node, grid.GetWorldPos(x, y), Quaternion.identity);
                        n1.GetComponent<Node>().x = x;
                        n1.GetComponent<Node>().y = y;
                        Nodes.AddLast(n1);
                        click.pitch = (x * 0.5f / width + y * 0.5f / height);
                        click.Play();

                    }
                    yield return null;

                }
            }
        }
        foreach(GameObject n in Nodes)
        {
            if (n != null)
            {
                n.GetComponent<Node>().FindNeighbors();

            }
            //n.GetComponent<Node>().PrintNumNeighbors();
            yield return null;
        }
        waypointed = true;
    }


    public void SwitchMapFile()
    {
        StopAllCoroutines();
        source.loop = false;
        filename = files[dropdown.value];
        grid.ClearGraph();
        graphgrid.ClearGraph();
        ReadRoutine();
    }

    public int EuclideanDistance(int x1, int y1, int x2, int y2)
    {
        float sq = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
        int sqrt = Mathf.RoundToInt(Mathf.Sqrt(sq));
        return sqrt * 5;
    }

    public int ManhattanDistance(int x1, int y1, int x2, int y2)
    {
        int d = Mathf.Abs(y2 - y1) + Mathf.Abs(x2 - x1);
        return d * 5;
    }



    public void StartAStar()
    {
        if (graphed && waypointed)
        {
            ReloadWayPoints();
            ReloadTiles();
            open.Clear();

            if (!wayPoint)
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
                work = true;
                source.loop = true;
                StartCoroutine(AStarTile());

            }
            else
            {
                Node n = instantStart.GetComponent<Node>();
                if (Manhattan)
                {
                    n.h = ManhattanDistance(start.x, start.y, end.x, end.y);
                }
                else
                {
                    n.h = EuclideanDistance(start.x, start.y, end.x, end.y);
                }
                n.g = 0;
                distance = n.h;
                n.f = heturisticsWeight * n.h + gWeight * n.g;
                n.parentNode = null;
                openNodes.Add(instantStart);
                Started = true;
                work = true;
                source.loop = true;
                StartCoroutine(AStarWayPoint());
            }

        }
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

                StartCoroutine(PaintPath(tile));
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

            for (int i = 0; i < 8; i++)
            {
                if (t[i].s == PathTile.status.CLOSED)
                {

                    continue;
                    int ng = tile.g + t[i].getWeight();
                    int f1 = ng * gWeight + t[i].h * heturisticsWeight;
                    if (f1 < t[i].f)
                    {
                        t[i].parent = tile;
                        t[i].g = ng;
                        t[i].f = f1;
                    }

                }
                else if (t[i] == null)
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
                else if (t[i].getWeight() >= 3)
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

            graphgrid.SwitchTileStatus(x, y, PathTile.status.CLOSED);

            mixer.SetFloat("pitch", Mathf.Clamp(0.5f + (distance - tile.h) * 1.0f / distance * 0.5f, 0.5f, 2));
            //source.pitch = Mathf.Clamp(0.2f + (distance - tile.h)* 1.0f / distance * 0.8f, 0.1f, 2);
            if (!source.isPlaying)
            {
                source.Play();

            }
            //yield return null;
            yield return new WaitForSeconds(.05f);

        }

    }

    IEnumerator PaintPath(PathTile t)
    {
        PathTile t1 = t;
        while (t1 != null)
        {
            graphgrid.SwitchTileStatus(t1.x, t1.y, PathTile.status.PATH);
            mixer.SetFloat("pitch", Mathf.Clamp(0.5f + t1.h * 1.0f / distance * 0.8f, 0.5f, 2));
            //source.pitch = 0.2f + t1.h * 1.0f / distance * 0.9f;
            if (!source.isPlaying)
            {
                source.Play();

            }

            t1 = t1.parent;
            yield return new WaitForSeconds(0.02f);
        }
        mixer.SetFloat("pitch", 1);
        source.loop = false;
    }

    IEnumerator AStarWayPoint()
    {
        while (openNodes.Count > 0 && work)
        {
            GameObject g = openNodes.First();
            Node n = g.GetComponent<Node>();
            openNodes.RemoveAt(0);
            Debug.Log("f = " + n.f);
            if (g != instantStart)
            {
                n.SwitchStatus(global::Node.status.CLOSED);
                //n.ConnectParent();
            }

            if (g == instantEnd)
            {
                
                StartCoroutine(DrawWay(g));
                work = false;
                StopCoroutine(AStarWayPoint());
            }
            int neighborscount = n.GetNumNeighors();
            for (int i = 0; i < neighborscount; i++)
            {
                GameObject g1 = n.GetNeighbor(i);
                if (g1 == null)
                {
                    continue;
                }

                Node n1 = g1.GetComponent<Node>();
                if (n1.s == global::Node.status.CLOSED)
                {
                    continue;
                }
                else if (n1.s == global::Node.status.OPEN)
                {
                    //continue;
                    int dist = 0;
                    if (Manhattan)
                    {
                        dist = ManhattanDistance(n.x, n.y, n1.x, n1.y);
                    }
                    else
                    {
                        dist = EuclideanDistance(n.x, n.y, n1.x, n1.y);
                    }
                    int g1g = dist + n.g;
                    int f1 = gWeight * g1g + heturisticsWeight * n1.h;
                    if (f1 < n1.f)
                    {
                        n1.parentNode = g;
                    }
                }
                else
                {
                    int dist = 0;
                    int h1 = 0;
                    if (Manhattan)
                    {
                        dist = ManhattanDistance(n.x, n.y, n1.x, n1.y);
                        h1 = ManhattanDistance(n1.x, n1.y, end.x, end.y);

                    }
                    else
                    {
                        dist = EuclideanDistance(n1.x, n1.y, n1.x, n1.y);
                        h1 = EuclideanDistance(n1.x, n1.y, end.x, end.y);

                    }

                    int g1g = dist + n.g;
                    int f1 = gWeight * g1g + heturisticsWeight * h1;
                    n1.g = g1g;
                    n1.h = h1;
                    n1.f = f1;
                    n1.SwitchStatus(global::Node.status.OPEN);
                    n1.parentNode = g;
                    openNodes.Add(g1);
                    openNodes = openNodes.OrderBy(gx => gx.GetComponent<Node>().f).ToList();
                }
                yield return new WaitForEndOfFrame();
            }

            

            mixer.SetFloat("pitch", Mathf.Clamp(0.5f + (distance - n.h) * 1.0f / distance * 0.5f, 0.5f, 2));
            //source.pitch = Mathf.Clamp(0.2f + (distance - tile.h)* 1.0f / distance * 0.8f, 0.1f, 2);
            if (!source.isPlaying)
            {
                source.Play();

            }

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator DrawWay(GameObject g)
    {
        GameObject gx = g;
        while (gx != instantStart)
        {
            Node n = gx.GetComponent<Node>();
            n.ConnectParent();
            
            gx = n.parentNode;
            mixer.SetFloat("pitch", Mathf.Clamp(0.5f + n.h * 1.0f / distance * 0.8f, 0.5f, 2));
            //n.Clear();
            yield return new WaitForSeconds(0.1f);
        }
        source.loop = false;
        source.Stop();
        mixer.SetFloat("pitch", 1);
        

    }

    void SetStartPos()
    {
        if (instantStart != null)
        {
            Destroy(instantStart);
        }
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = cam.nearClipPlane;
        Vector3 worldpos = cam.ScreenToWorldPoint(mousePos);
        Vector3 Startpos = new Vector3(Mathf.Floor(worldpos.x / 2.0f) * 2.0f + 1.0f, Mathf.Floor(worldpos.y / 2.0f) * 2.0f + 1.0f, 8);
        start = graphgrid.WorldPosToGraph(worldpos);
        instantStart = Instantiate(graphStart, Startpos, Quaternion.identity);
        Vector2Int pos = grid.WorldPosToGraph(worldpos);
        Node n = instantStart.GetComponent<Node>();
        n.parentNode = null;
        n.x = pos.x;
        n.y = pos.y;
        n.FindNeighbors();
        n.ConnectToNeighbor();
        n.PrintNumNeighbors();
        source.pitch = 1;
        source.Play();
    }

    void SetEndPos()
    {
        if (instantEnd != null)
        {
            Destroy(instantEnd);
        }
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = cam.nearClipPlane;
        Vector3 worldpos = cam.ScreenToWorldPoint(mousePos);
        Vector3 Startpos = new Vector3(Mathf.Floor(worldpos.x / 2.0f) * 2.0f + 1.0f, Mathf.Floor(worldpos.y / 2.0f) * 2.0f + 1.0f, 8);
        end = graphgrid.WorldPosToGraph(worldpos);
        instantEnd = Instantiate(graphEnd, Startpos, Quaternion.identity);
        Vector2Int pos = grid.WorldPosToGraph(worldpos);
        Node n = instantEnd.GetComponent<Node>();
        n.x = pos.x;
        n.y = pos.y;
        n.FindNeighbors();
        n.ConnectToNeighbor();
        n.PrintNumNeighbors();



        source.pitch = 1;
        source.Play();

    }

    public void StartSetStart()
    {
        setStart = true;
        setEnd = false;
    }

    public void StartSetEnd()
    {
        setEnd = true;
        setStart = false;
    }

    public void ReloadTiles()
    {
        StopAllCoroutines();
        source.loop = false;
        work = false;
        graphgrid.ResetTiles();
    }

    public void ReloadWayPoints()
    {
        StopAllCoroutines();
        source.loop = false;
        work = false;
        openNodes.Clear();
        foreach (GameObject g in Nodes)
        {
            if (g != null)
            {
                Node n = g.GetComponent<Node>();
                n.SwitchStatus(global::Node.status.UNREAD);
                n.parentNode = null;

            }
        }
    }

    public void ReloadAll()
    {
        ReloadTiles();
        ReloadWayPoints();
        instantEnd.GetComponent<Node>().parentNode = null;
        instantEnd.GetComponent<Node>().SwitchStatus(global::Node.status.UNREAD);
        instantEnd.GetComponent<SpriteRenderer>().color = Color.white;
        List<GameObject> lines = GameObject.FindGameObjectsWithTag("Line").ToList();
        foreach (GameObject line in lines)
        {
            Destroy(line);
        }
    }
    public void SwitchHeturistics()
    {
        if (!work)
        {
            Manhattan = !Manhattan;
            if (Manhattan)
            {
                textType.text = "Manhattan";
            }
            else
            {
                textType.text = "Eculidean";
            }

        }
    }

    public void SwitchAStar()
    {
        if (!work)
        {
            wayPoint = !wayPoint;
            if (wayPoint)
            {
                aStarText.text = "WayPoint";
            }
            else
            {
                aStarText.text = "Tiles";
            }

        }
    }


    public void SetGValue()
    {
        if (!work)
        {
            gWeight = Mathf.RoundToInt(gSlider.value);
            gtext.text = gWeight.ToString();
        }
    }

    public void SetHValue()
    {
        if (!work)
        {
            heturisticsWeight = Mathf.RoundToInt(heturisticsSlider.value);
            htext.text = heturisticsWeight.ToString();
        }
    }


    char ReadTile(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return mapstore[x, y];
        }
        else
        {
            return '@';
        }
    }
}
