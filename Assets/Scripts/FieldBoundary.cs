using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldBoundary
{
    public float xMin;
    public float zMin;
    public float xMax;
    public float zMax;

    public Color planeColor;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    GameObject plane;


    // Start is called before the first frame update
    /*void Start()
    {
        boundaryPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        boundaryPlane.transform.parent = this.transform;
    }*/

    void CreatePlane()
    {
        plane = new GameObject("Plane");
        //plane.transform.parent = this.transform;
        meshRenderer = plane.AddComponent<MeshRenderer>();
        meshFilter = plane.AddComponent<MeshFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        return;
        if (plane == null)
        {
            CreatePlane();
        }

        var mesh = new Mesh();

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(xMin, 0.1f, zMin);
        vertices[1] = new Vector3(xMin, 0.1f, zMax);
        vertices[2] = new Vector3(xMax, 0.1f, zMin);
        vertices[3] = new Vector3(xMax, 0.1f, zMax);

        mesh.vertices = vertices;
        mesh.triangles = new int[6] { 0, 1, 2, 1, 3, 2 };
        mesh.normals = new Vector3[4] 
        {
            Vector3.up, 
            Vector3.up,
            Vector3.up,
            Vector3.up 
        };
        mesh.uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        meshFilter.mesh = mesh;
    }
}
