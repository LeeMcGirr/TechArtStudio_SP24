using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostPass : ScriptableRenderPass
{
    Material myBloomMat;
    BloomComponent myBloomEffect;
    const int MaxpyramidSize = 16;
    int[] _BloomMipUp;
    int[] _BloomMipDown;
    RTHandle[] myBloomMipUp;
    RTHandle[] myBloomMipDown;
    GraphicsFormat hdrFormat;

    RenderTextureDescriptor myDescriptor;
    RTHandle myCamColorTarget;
    RTHandle myCamDepthTarget;





    public CustomPostPass(Material bloomMat)
    {
        myBloomMat = bloomMat;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;


        _BloomMipUp = new int[MaxpyramidSize];
        _BloomMipDown = new int[MaxpyramidSize];
        //RTHandles are basically just scaling render textures attached to the camera
        myBloomMipUp = new RTHandle[MaxpyramidSize];
        myBloomMipDown = new RTHandle[MaxpyramidSize];

        //this is just ripped from the bloom pass in packages more or less
        for (int i = 0; i < MaxpyramidSize; i++)
        {
            _BloomMipUp[i] = Shader.PropertyToID("_BloomMipUp" + i);
            _BloomMipDown[i] = Shader.PropertyToID("_BloomMipDown" + i);
            myBloomMipUp[i] = RTHandles.Alloc(_BloomMipUp[i], name: "_BloomMipUp" + i);
            myBloomMipDown[i] = RTHandles.Alloc(_BloomMipDown[i], name: "_BloomMipDown" + i);
        }

        //this is also ripped, just handles color space
        const FormatUsage usage = FormatUsage.Linear | FormatUsage.Render;
        if(SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, usage)) //HDR fallback
        {
            hdrFormat = GraphicsFormat.B10G11R11_UFloatPack32;
        }
        else
        {
            hdrFormat = QualitySettings.activeColorSpace == ColorSpace.Linear ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm; 
        }

    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        VolumeStack stack = VolumeManager.instance.stack;
        myBloomEffect = stack.GetComponent<BloomComponent>();
        //command buffer is a list of rendering tasks
        CommandBuffer cmd = CommandBufferPool.Get();

        //writing inside a profiling scope lets us control how we visualize our new pass in the SRP frame debugger
        using(new UnityEngine.Rendering.ProfilingScope(cmd, new ProfilingSampler("Custom Post Process Bloom")))
        {
            SetupBloom(cmd, myCamColorTarget);
        }
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        myDescriptor = renderingData.cameraData.cameraTargetDescriptor;
    }

    RenderTextureDescriptor GetCompatibleDescriptor() 
                            => GetCompatibleDescriptor(myDescriptor.width, myDescriptor.height, myDescriptor.graphicsFormat);

    RenderTextureDescriptor GetCompatibleDescriptor(int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None) 
                            => GetCompatibleDescriptor(myDescriptor, width, height, format, depthBufferBits);

    internal static RenderTextureDescriptor GetCompatibleDescriptor(RenderTextureDescriptor desc, int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None)
    {
        desc.depthBufferBits = (int)depthBufferBits;
        desc.msaaSamples = 1;
        desc.width = width;
        desc.height = height;
        desc.graphicsFormat = format;
        return desc;
    }

    public void SetTarget(RTHandle camColorTargetHandle, RTHandle camDepthTargetHandle)
    {
        myCamColorTarget = camColorTargetHandle;
        myCamDepthTarget = camDepthTargetHandle;
    }

    //ripped from bloom shader
    void SetupBloom(CommandBuffer cmd, RTHandle source)
    {
        int downres = 1;
        //bitshift to halve the values
        int tw = myDescriptor.width >> downres;
        int th = myDescriptor.height >> downres;

        //iteration count
        int maxSize = Mathf.Max(tw, th);
        int iterations = Mathf.FloorToInt(Mathf.Log(maxSize, 2f) - 1f);

        //pull values from the bloom volume component class
        int mipCount = Mathf.Clamp(iterations, 1, myBloomEffect.maxIterations.value);
        float clamp = myBloomEffect.clamp.value;
        float threshold = Mathf.GammaToLinearSpace(myBloomEffect.threshold.value);
        float thresholdKnee = threshold * .5f;
        float scatter = Mathf.Lerp(0.05f, 0.95f, myBloomEffect.scatter.value);
        var bloomMaterial = myBloomMat;

        bloomMaterial.SetVector("_Params", new Vector4(scatter, clamp, threshold, thresholdKnee));

        //pre
        var desc = GetCompatibleDescriptor(tw, th, hdrFormat);
        for(int i = 0; i < mipCount; i++)
        {
            RenderingUtils.ReAllocateIfNeeded(ref myBloomMipUp[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: myBloomMipUp[i].name);
            RenderingUtils.ReAllocateIfNeeded(ref myBloomMipDown[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: myBloomMipDown[i].name);
            desc.width = Mathf.Max(1, desc.width >> 1);
            desc.height = Mathf.Max(1, desc.height >> 1);
        }

        Blitter.BlitCameraTexture(cmd, source, myBloomMipDown[0], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 0);

        //downsample
        var lastDown = myBloomMipDown[0];
        for (int i = 1; i < mipCount; i++)
        {
            //gaussian blur
            Blitter.BlitCameraTexture(cmd, lastDown, myBloomMipUp[i], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 1);
            Blitter.BlitCameraTexture(cmd, myBloomMipUp[i], myBloomMipDown[i], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 2);
            lastDown = myBloomMipDown[i];
        }

        //upsample
        for(int i = mipCount - 2; i >= 0; i--)
        {
            var lowMip = (i == mipCount -2) ? myBloomMipDown[i+1] : myBloomMipUp[i+1];
            var highMip = myBloomMipDown[i];
            var dst = myBloomMipUp[i];

            cmd.SetGlobalTexture("_SourceTexLowMip", lowMip);
            Blitter.BlitCameraTexture(cmd, highMip, dst, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 3);
        }

        //store the tex 
        cmd.SetGlobalTexture("_Bloom_Texture", myBloomMipUp[0]);
        cmd.SetGlobalFloat("_BloomIntensity", myBloomEffect.intensity.value);
    }
}
