using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayMarchingControl : MonoBehaviour
{
    public Material rayMarchingMaterial;
    public RenderTexture renderTexture;

    void Start()
    {
        if (rayMarchingMaterial == null)
        {
            Debug.LogError("Ray marching material is not set.");
        }

        // Initialize the RenderTexture
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }
    }

    void Update()
    {
        // Pass the camera's position to the shader
        rayMarchingMaterial.SetVector("_CamPos", Camera.main.transform.position);

        // Convert the camera's rotation to a Matrix4x4 and pass it to the shader
        Matrix4x4 camRot = Matrix4x4.Rotate(Camera.main.transform.rotation);
        rayMarchingMaterial.SetMatrix("_CamRot", camRot);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (rayMarchingMaterial != null)
        {
            // Draw the ray marching shader output to the RenderTexture
            Graphics.Blit(null, renderTexture, rayMarchingMaterial);

            // Optionally, display the RenderTexture on the screen
            Graphics.Blit(renderTexture, dest);
        }
    }
}
