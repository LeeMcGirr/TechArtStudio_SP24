using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class cubeMesh : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CreateCube();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateCube()
    {
        Vector3[] vertices =
        {
            new Vector3 (0.0f, 0.0f, 0.0f),
            new Vector3 (1.0f, 0.0f, 0.0f),
            new Vector3 (1.0f, 1.0f, 0.0f),
            new Vector3 (0,1,0),
            new Vector3 (0,1,1),
            new Vector3 (1,1,1),
            new Vector3 (1,0,1),
            new Vector3 (0,0,1),
        };

        int[] triangles =
        {
            0,2,1, // front
            0,3,2,
            2,3,4, //top
            2,4,5,
            1,2,5, //right
            1,5,6,
            0,7,4,
            0,4,3,
            5,4,7,
            0,6,7,
            0,1,6
        };

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.Optimize();
        mesh.RecalculateNormals();
    }
}
