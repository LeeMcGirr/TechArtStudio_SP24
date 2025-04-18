using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SkinnedMeshtoMesh : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedmesh;
    public VisualEffect VFXGraph;
    public float refreshRate;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UpdateVFXGraph());
    }

    IEnumerator UpdateVFXGraph()
    {
        while (gameObject.activeSelf)
        {
            Mesh m = new Mesh();
            skinnedmesh.BakeMesh(m);

            Vector3[] vertices = m.vertices;
            Mesh m2 = new Mesh();
            m2.vertices = vertices;

            VFXGraph.SetMesh("Mesh", m2);

            yield return new WaitForSeconds(refreshRate);
        }
    }
}
