using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Liquid : MonoBehaviour
{

    public float fillAmount = 0.5f;
    Mesh myMesh;
    Renderer myRend;
    Vector3 myPos;
    // Start is called before the first frame update
    void Start()
    {
        myMesh = GetComponent<MeshFilter>().sharedMesh;
        myRend = GetComponent<Renderer>();
    }

    private void OnValidate()
    {
        myMesh = GetComponent<MeshFilter>().sharedMesh;
        myRend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        FillPos();
    }

    void FillPos()
    {
        Vector3 worldPos = transform.TransformPoint(myMesh.bounds.center);

        myPos = worldPos - transform.position - new Vector3(0, fillAmount, 0);

        myRend.sharedMaterial.SetVector("_FillAmount", worldPos);
        Debug.DrawRay(myPos, Vector3.left*5f, Color.magenta);
        Debug.DrawRay(worldPos, Vector3.left * 5f, Color.cyan);


    }
}
