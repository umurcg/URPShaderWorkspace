using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class OutlineSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material outlineMaterial = null;
    }

    public OutlineSettings settings = new OutlineSettings();

    class OutlinePass : ScriptableRenderPass
    {
        public RTHandle tempTexture;
        public Material outlineMaterial;

        public OutlinePass(string profilerTag)
        {
            profilingSampler = new ProfilingSampler(profilerTag);
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            if (tempTexture == null || tempTexture.rt == null || 
                tempTexture.rt.width != descriptor.width || 
                tempTexture.rt.height != descriptor.height)
            {
                RenderingUtils.ReAllocateHandleIfNeeded(ref tempTexture, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempOutlineTexture");
                // Debug.Log($"Allocated tempTexture: {tempTexture.name}, Size: {descriptor.width}x{descriptor.height}");
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (outlineMaterial == null)
            {
                Debug.LogError("Outline material is null. Please assign a material in the OutlineFeature settings.");
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get();

            var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            if (cameraColorTarget.rt == null)
            {
                // Debug.LogError("Camera color target is null. Skipping outline pass.");
                return;
            }
            
            outlineMaterial.SetTexture("_MainText", cameraColorTarget);
            
            cmd.Blit(cameraColorTarget, tempTexture, outlineMaterial);
            cmd.Blit(tempTexture, cameraColorTarget);
            context.ExecuteCommandBuffer(cmd);
            
            CommandBufferPool.Release(cmd);
        }


    }

    private OutlinePass outlinePass;
    private const string k_ProfilerTag = "Outline Pass";

    public override void Create()
    {
        outlinePass = new OutlinePass(k_ProfilerTag)
        {
            renderPassEvent = settings.renderPassEvent,
            outlineMaterial = settings.outlineMaterial
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (outlinePass.outlineMaterial == null)
        {
            Debug.LogWarning($"Missing Outline Material. {GetType().Name} render pass will not execute. Check for missing reference in the assigned renderer.");
            return;
        }
        
        renderer.EnqueuePass(outlinePass);
    }

    protected override void Dispose(bool disposing)
    {
        outlinePass?.tempTexture?.Release();
    }
}