using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node : MonoBehaviour
{
    // Start is called before the first frame update

    public enum status { OPEN, CLOSED, UNREAD, PATH};

    public GameObject parentNode = null;
    public int x;
    public int y;

    public int h = int.MaxValue;
    public int g = int.MaxValue;
    public int f = int.MaxValue;

    public GameObject LinePrefab;

    public LayerMask mask;

    public status s = status.UNREAD;

    GameObject Line;

    LinkedList<GameObject> neighbors = new LinkedList<GameObject>();

    SpriteRenderer srenderer;

    void Start()
    {
        //neighbors = new List<GameObject>();
        srenderer = GetComponent<SpriteRenderer>();
    }


    public void FindNeighbors()
    {
        
        List<GameObject> l = GameObject.FindGameObjectsWithTag("Node").ToList();
        foreach (GameObject go in l)
        {
            
            Vector2 dir = (go.transform.position - this.transform.position).normalized;
            Vector2 pos = transform.position;
            RaycastHit2D hit = Physics2D.Raycast(pos, dir, float.PositiveInfinity, mask);
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.tag == "Node")
                {
                    if (!neighbors.Contains(hit.collider.gameObject))
                    {
                        neighbors.AddLast(hit.collider.gameObject);
                        hit.collider.gameObject.GetComponent<Node>().neighbors.AddLast(this.gameObject);
                    }
                    
                }
            }
        }
    }

    public void ConnectParent()
    {
        if (parentNode != null)
        {
            if (Line != null)
            {
                Line.GetComponent<SetLine>().SetLineDir(this.transform.position, parentNode.transform.position);
            }
            else
            {
                Line = Instantiate(LinePrefab,transform);
                Line.GetComponent<SetLine>().SetLineDir(this.transform.position, parentNode.transform.position);

            }
        }
    }

    public void Clear()
    {
        parentNode = null;
        h = int.MaxValue;
        g = int.MaxValue;
        f = int.MaxValue;
        SwitchStatus(status.UNREAD);
        if (Line != null)
        {
            Line.GetComponent<SetLine>().SetLineDir(Vector3.zero, Vector3.zero);

        }


    }

    public void PrintNumNeighbors()
    {
        Debug.Log("neighbors: " + neighbors.Count);
    }

    public void ConnectToNeighbor()
    {
        foreach (GameObject n in neighbors)
        {
            n.GetComponent<Node>().neighbors.AddLast(this.gameObject);
        }
        
    }

    public int GetNumNeighors()
    {
        return neighbors.Count;
    }


    public GameObject[] GetNeighbors()
    {
        return neighbors.ToArray();
    }

    public void SwitchStatus(status s1)
    {
        s = s1;
        if (s == status.UNREAD)
        {
            srenderer.color = Color.black;
        }
        else if (s == status.OPEN)
        {
            srenderer.color = Color.cyan;
        }
        else if (s == status.CLOSED)
        {
            srenderer.color = Color.red;
        }
        else
        {
            srenderer.color = Color.green;
        }
    }
    private void OnDestroy()
    {
        Destroy(Line);

    }
}
