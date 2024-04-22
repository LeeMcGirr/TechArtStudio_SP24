using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostRenderFeature : ScriptableRendererFeature
{

    public Shader myBloomShader;
    CustomPostPass myPass;
    Material myBloomMat;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //add passes to queue
        renderer.EnqueuePass(myPass);
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
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(myBloomMat);
    }

}
