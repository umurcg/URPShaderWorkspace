using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostProcessPass : ScriptableRenderPass
{
    Material bloomMaterial;
    Material compositeMaterial;
    RenderTextureDescriptor _descriptor;


    RTHandle m_CameraColorTarget;
    RTHandle m_CameraDepthTarget;

    private int[] _BloomMipUp;
    private int[] _BloomMipDown;
    private RTHandle[] m_BloomMipUp;
    private RTHandle[] m_BloomMipDown;
    private GraphicsFormat hdrFormat;
    private const int k_MaxPyramidSize = 16;

    private BanDayBloomEffect bloomEffect;
    


    public CustomPostProcessPass(Material bloomMaterial, Material compositeMaterial)
    {
        this.bloomMaterial = bloomMaterial;
        this.compositeMaterial = compositeMaterial;

        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        _BloomMipUp = new int [k_MaxPyramidSize];
        _BloomMipDown = new int [k_MaxPyramidSize];
        m_BloomMipUp = new RTHandle [k_MaxPyramidSize];
        m_BloomMipDown = new RTHandle [k_MaxPyramidSize];

        for (int i = 0; i < k_MaxPyramidSize; i++)
        {
            _BloomMipUp[i] = Shader.PropertyToID("_BloomMipUp" + i);
            _BloomMipDown[i] = Shader.PropertyToID("_BloomMipDown" + i);
            m_BloomMipUp[i] = RTHandles.Alloc(_BloomMipUp[i], name: "_BloomMipUp" + i);
            m_BloomMipDown[i] = RTHandles.Alloc(_BloomMipDown[i], name: "_BloomMipDown" + 1);
        }

        const FormatUsage usage = FormatUsage.Linear | FormatUsage.Render;
        if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, usage)){
            //HDRfallback
            hdrFormat = GraphicsFormat.B10G11R11_UFloatPack32;
        }
        else

        {
            hdrFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
                ? GraphicsFormat.R8G8B8A8_SRGB
                : GraphicsFormat.R8G8B8A8_UNorm;
        }
        
        
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        VolumeStack stack = VolumeManager.instance.stack;
        bloomEffect = stack.GetComponent<BanDayBloomEffect>();

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd,new ProfilingSampler("CustomPostProcessPass")))
        {
            SetupBloom(cmd, m_CameraColorTarget);
        }
        
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        
        CommandBufferPool.Release(cmd);
    }

    private void SetupBloom(CommandBuffer cmd, RTHandle source)
    {
        // Start at half-res
        int downres = 1;
        int tw = _descriptor.width >> downres;
        int th = _descriptor.height >> downres;
        // Determine the iteration count
        
        int maxSize = Mathf. Max(tw,
            th);
        int iterations = Mathf .FloorToInt (Mathf.Log(maxSize, 2f) - 1);
        int mipCount = Mathf .Clamp(iterations, 1, bloomEffect.maxIterations. value);
        
        // Pre-filtering parameters
        float clamp = bloomEffect.clamp. value;
        float threshold = Mathf.GammaToLinearSpace(bloomEffect.threshold.value);
        float thresholdKnee = threshold * 0.5f; // Hardcoded soft knee
        
        // Material setup
        float scatter = Mathf. Lerp(0.05f, 0.95f, bloomEffect.scatter .value);
        
        bloomMaterial.SetVector("_Params", new Vector4(scatter, clamp, threshold, thresholdKnee));
        
        //Prefilter
        var desc = GetCompatibleDescriptor(tw, th, hdrFormat);
        for (int i = 0; i < mipCount; i++)
        {
            RenderingUtils.ReAllocateIfNeeded(ref m_BloomMipUp[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp,
                name: m_BloomMipUp[i].name);
            RenderingUtils.ReAllocateIfNeeded(ref m_BloomMipDown[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp,
                name: m_BloomMipDown[i].name);
                
            desc.width = Mathf.Max(1, desc.width >> 1);
            desc.height = Mathf.Max(1, desc.height >> 1);
        }

        Blitter.BlitCameraTexture(cmd, source , m_BloomMipUp[0], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 0);
        
            // Downsample - gaussian pyramid
        var lastDown = m_BloomMipDown [0];
        for (int i = 1; i < mipCount; i++)
        {
            // Classic two pass gaussian blur - use mipUp as a temporary target
            //First pass does 2x downsampling + 9 - tap gaussian
            //Second pass does 9 - tap gaussian using a  5 - tap filter + bilinear filtering
            Blitter.BlitCameraTexture(cmd, lastDown, m_BloomMipUp[i], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 1);
            Blitter.BlitCameraTexture(cmd, m_BloomMipUp[i], m_BloomMipDown[i], RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store, bloomMaterial, 2);
            
            lastDown = m_BloomMipDown[i];
        }
        
        // Upsample - gaussian pyramid
        for (int i = mipCount - 2; i >= 0; i--)
        {  
            var lowMip= (i==mipCount-1)? m_BloomMipDown[i+1]: m_BloomMipUp[i+1];
            var highMip = m_BloomMipUp[i];
            var dst = m_BloomMipUp[i];
            
            cmd.SetGlobalTexture("_SourceTexLowMip", lowMip);
            Blitter.BlitCameraTexture(cmd, highMip, dst, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 3);
        }
        
        cmd.SetGlobalTexture("_BloomTex", m_BloomMipUp[0]);
        cmd.SetGlobalFloat("_BloomIntensity", bloomEffect.intensity.value);
    }

    

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        base.OnCameraSetup(cmd, ref renderingData);
        _descriptor = renderingData.cameraData.cameraTargetDescriptor;
    }

    public void SetTarget(RTHandle cameraColorTargetHandle, RTHandle cameraDepthTargetHandle)
    {
        m_CameraColorTarget = cameraColorTargetHandle;
        m_CameraDepthTarget = cameraDepthTargetHandle;
    }
    
    RenderTextureDescriptor GetCompatibleDescriptor() => GetCompatibleDescriptor(_descriptor.width, _descriptor.height, _descriptor. graphicsFormat);
    RenderTextureDescriptor GetCompatibleDescriptor(int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits. None) => GetCompatibleDescriptor(_descriptor, width, height, format, depthBufferBits);

    internal static RenderTextureDescriptor GetCompatibleDescriptor(RenderTextureDescriptor desc, int width, int height,
        GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None)
    {
        desc.depthBufferBits = (int)depthBufferBits;
        desc.msaaSamples = 1;
        desc.width = width;
        desc.height = height;
        desc.graphicsFormat = format;
        return desc;
    }
    
}