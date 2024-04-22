using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostPass : ScriptableRenderPass
{
    RenderTextureDescriptor myDescriptor;
    CameraData myCamData;
    public Material myBloomMat;

    BloomComponent myBloomEffect;

    const int MaxPyramidSize = 16;
    int[] _BloomMipUp;
    int[] _BloomMipDown;
    RTHandle[] myBloomMipUp;
    RTHandle[] myBloomMipDown;
    GraphicsFormat hdrFormat;

    RTHandle myCamColorTarget;
    RTHandle myCamDepthTarget;


    public int clamp;
    public float threshold;
    public float scatter;


    public CustomPostPass(Material bloomMat)
    {
        myBloomMat = bloomMat;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        _BloomMipUp = new int[MaxPyramidSize];
        _BloomMipDown = new int[MaxPyramidSize];
        myBloomMipUp = new RTHandle[MaxPyramidSize];
        myBloomMipDown = new RTHandle[MaxPyramidSize];

        for (int i = 0; i < MaxPyramidSize; i++)
        {
            _BloomMipUp[i] = Shader.PropertyToID("_BloomMipUp" + i);
            _BloomMipDown[i] = Shader.PropertyToID("_BloomMipDown" + i);
            // Get name, will get Allocated with descriptor later
            myBloomMipUp[i] = RTHandles.Alloc(_BloomMipUp[i], name: "_BloomMipUp" + i);
            myBloomMipDown[i] = RTHandles.Alloc(_BloomMipDown[i], name: "_BloomMipDown" + i);
        }

        const FormatUsage usage = FormatUsage.Linear | FormatUsage.Render;
        if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, usage)) // HDR fallback
        {
            hdrFormat = GraphicsFormat.B10G11R11_UFloatPack32;
        }
        else
        {
            hdrFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
                ? GraphicsFormat.R8G8B8A8_SRGB
                : GraphicsFormat.R8G8B8A8_UNorm;

        }
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureTarget(myCamColorTarget);
        myDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        myCamData = renderingData.cameraData;
    }

    public void SetTarget(RTHandle camColorTargetHandle, RTHandle camDepthTargetHandle)
    {
        myCamColorTarget = camColorTargetHandle;
        myCamDepthTarget = camDepthTargetHandle;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        VolumeStack stack = VolumeManager.instance.stack;
        myBloomEffect = stack.GetComponent<BloomComponent>();
        //command buffer is a list of rendering tasks
        CommandBuffer cmd = CommandBufferPool.Get();

        //writing inside a profiling scope lets us control how we visualize our new pass in the SRP frame debugger
        using (new UnityEngine.Rendering.ProfilingScope(cmd, new ProfilingSampler("Custom Post Process Bloom")))
        {
            SetupBloom(cmd, myCamColorTarget);
            Blitter.BlitCameraTexture(cmd, myCamColorTarget, myCamColorTarget, myBloomMat, 0);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }

    //ripped from bloom shader
    private void SetupBloom(CommandBuffer cmd, RTHandle source)
    {
        // Start at half-res
        int downres = 1;
        int tw = myDescriptor.width >> downres;
        int th = myDescriptor.height >> downres;

        // Determine the iteration count
        int maxSize = Mathf.Max(tw, th);
        int iterations = Mathf.FloorToInt(Mathf.Log(maxSize, 2f) - 1);
        int mipCount = Mathf.Clamp(iterations, 1, myBloomEffect.maxIterations.value);

        // Pre-filtering parameters
        float newClamp = clamp;
        float newThreshold = Mathf.GammaToLinearSpace(threshold);
        float thresholdKnee = threshold * 0.5f; // Hardcoded soft knee

        float newScatter = Mathf.Lerp(0.05f, 0.95f, scatter);
        var bloomMaterial = myBloomMat;

        bloomMaterial.SetVector("_Params", new Vector4(newScatter, newClamp, newThreshold, thresholdKnee));

        //Prefilter
        var desc = GetCompatibleDescriptor(tw, th, hdrFormat);
        for (int i = 0; i < mipCount; i++)
        {
            RenderingUtils.ReAllocateIfNeeded(ref myBloomMipUp[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: myBloomMipUp[i].name);
            RenderingUtils.ReAllocateIfNeeded(ref myBloomMipDown[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: myBloomMipDown[i].name);
            desc.width = Mathf.Max(1, desc.width >> 1);
            desc.height = Mathf.Max(1, desc.height >> 1);
        }

        Blitter.BlitCameraTexture(cmd, source, myBloomMipDown[0], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 0);


        // Downsample - gaussian pyramid
        var lastDown = myBloomMipDown[0];
        for (int i = 1; i < mipCount; i++)
        {
            // Classic two pass gaussian blur - use mipUp as a temporary target
            //   First pass does 2x downsampling + 9-tap gaussian
            //   Second pass does 9-tap gaussian using a 5-tap filter + bilinear filtering
            Blitter.BlitCameraTexture(cmd, lastDown, myBloomMipUp[i], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 1);
            Blitter.BlitCameraTexture(cmd, myBloomMipUp[i], myBloomMipDown[i], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 2);

            lastDown = myBloomMipDown[i];
        }

        // Upsample (bilinear by default, HQ filtering does bicubic instead
        for (int i = mipCount - 2; i >= 0; i--)
        {
            var lowMip = (i == mipCount - 2) ? myBloomMipDown[i + 1] : myBloomMipUp[i + 1];
            var highMip = myBloomMipDown[i];
            var dst = myBloomMipUp[i];

            cmd.SetGlobalTexture("_SourceTexLowMip", lowMip);
            Blitter.BlitCameraTexture(cmd, highMip, dst, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 3);
        }

        cmd.SetGlobalTexture("_Bloom_Texture", myBloomMipUp[0]);
        cmd.SetGlobalFloat("_BloomIntensity", myBloomEffect.intensity.value);
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
}
