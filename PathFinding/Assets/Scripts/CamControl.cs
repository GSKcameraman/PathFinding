using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour
{

    public float sensitivity = 10f;
    public float sensitivityXY = 0.5f;
    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        float size = cam.orthographicSize;
        size -= Input.GetAxis("Mouse ScrollWheel") * sensitivity;
        size = Mathf.Clamp(size, 4, float.MaxValue);
        cam.orthographicSize = size;

        float x = this.transform.position.x;
        x += Input.GetAxis("Horizontal") * sensitivityXY;
        x = Mathf.Clamp(x, 0, float.MaxValue);

        float y = this.transform.position.y;
        y += Input.GetAxis("Vertical") * sensitivityXY;
        y = Mathf.Clamp(y, 0, float.MaxValue);

        this.transform.position = new Vector3(x, y, this.transform.position.z);
    }
}
