using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

public class newCustomPass : ScriptableRenderPass
{
    public Material myBloomMat;

    public int clamp;
    public float threshold;
    public float scatter;

    public newCustomPass(Material bloomMat)
    {
        //come back
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {

    }
}
