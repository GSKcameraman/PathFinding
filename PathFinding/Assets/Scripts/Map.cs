using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public class Map : MonoBehaviour
{
    public List<string> files;
    TileGrid grid;

    [Dropdown("files")]
    public string filename;


    public Camera cam;

    public int height;
    public int width;

    public GameObject tileGround;
    public GameObject tileWall;
    public GameObject tileTree;

    string[] mapstore;
    // Start is called before the first frame update
    void Start()
    {
        readfile();
        grid.tileGround = tileGround;
        grid.tileWall = tileWall;
        grid.tileTree = tileTree;
        grid.CreateTiles();
        //Debug.Log(mapstore[0]);
        cam.transform.position = new Vector3(width/2, height/2, -20);
        cam.orthographicSize = Mathf.Max(height,width / 1.6f) / 2.0f;

    }

    // Update is called once per frame
    void Update()
    {
        
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

        grid = new TileGrid(width, height, 10f, 1f, Vector3.zero);
        mapstore = new string[height];
        //map (the line)
        reader.ReadLine() ;
        //rest of the file
        for (int y = 0; y < height; y++)
        {
            string line = reader.ReadLine();
            mapstore[y] = line;
            for(int x = 0; x < width; x++)
            {
                char c = line[x];
                grid.SetValue(x, y, c);
            }
        }

        grid.PrintValue();
        reader.Close();
    }
}
