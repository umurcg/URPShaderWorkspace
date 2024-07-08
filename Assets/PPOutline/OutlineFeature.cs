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
        private RTHandle tempTexture;
        public Material outlineMaterial;
        private OutlineSettings settings;
        private string profilerTag;

        public OutlinePass(OutlineSettings settings, string profilerTag)
        {
            this.settings = settings;
            this.profilerTag = profilerTag;
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempOutlineTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (outlineMaterial == null)
            {
                Debug.LogError("Outline material is null. Please assign a material in the OutlineFeature settings.");
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            try
            {
                // Draw the outline effect
                Blitter.BlitCameraTexture(cmd, cameraColorTarget, tempTexture, outlineMaterial, 0);
                Blitter.BlitCameraTexture(cmd, tempTexture, cameraColorTarget);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during outline effect: {e.Message}\n{e.StackTrace}");
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // No need to release tempTexture here as it's handled by RTHandles system
        }
    }

    OutlinePass outlinePass;
    private const string k_ProfilerTag = "Outline Pass";

    public override void Create()
    {
        outlinePass = new OutlinePass(settings, k_ProfilerTag)
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
        // tempTexture is automatically disposed by the RTHandle system
    }
}