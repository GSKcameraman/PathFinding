using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLine : MonoBehaviour
{

    public LineRenderer line;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetLineDir(Vector3 start, Vector3 end)
    {
        line.SetPosition(0,start);
        line.SetPosition(1,end);
    }
}
