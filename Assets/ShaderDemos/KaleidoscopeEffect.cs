using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "Image Effects Ultra/Kaleidoscope", order = 1)]
public class KaleidoscopeEffect : ScriptableObject
{
    public int segments = 4;
    Material baseMaterial;

    // Find the Kaleidoscope shader source.
    public void OnCreate()
    {
        baseMaterial = new Material(Resources.Load<Shader>("Shaders/Kaleidoscope"));
        baseMaterial.SetFloat("_SegmentCount", segments);
    }

    public void Render(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst, baseMaterial);
    }
}
