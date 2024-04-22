using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostRenderFeature : ScriptableRendererFeature
{

    public Shader myBloomShader;
    public float scatter;
    public int clamp;
    public float threshold;
    Material myBloomMat;
    CustomPostPass myPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(myPass);
        }
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if(renderingData.cameraData.cameraType == CameraType.Game)
        {
            myPass.ConfigureInput(ScriptableRenderPassInput.Depth);
            myPass.ConfigureInput(ScriptableRenderPassInput.Color);
            myPass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
        }
    }

    public override void Create()
    {
        myBloomMat = CoreUtils.CreateEngineMaterial(myBloomShader);
        myPass = new CustomPostPass(myBloomMat);
        myPass.clamp = clamp;
        myPass.threshold = threshold;
        myPass.scatter = scatter;  
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(myBloomMat);
    }

}
