using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ObjectIDRenderPass : ScriptableRenderPass
{
    private RTHandle objectIDTexture;
    private Material objectIDMaterial;
    private ProfilingSampler profilingSampler;

    public ObjectIDRenderPass(Material material)
    {
        objectIDMaterial = material;
        renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        profilingSampler = new ProfilingSampler("Object ID Pass");
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;
        RenderingUtils.ReAllocateIfNeeded(ref objectIDTexture, descriptor, FilterMode.Point, TextureWrapMode.Clamp,
            name: "_ObjectIDTexture");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, profilingSampler))
        {
            var sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            var drawingSettings =
                CreateDrawingSettings(new ShaderTagId("UniversalForward"), ref renderingData, sortingCriteria);
            drawingSettings.overrideMaterial = objectIDMaterial;
            drawingSettings.overrideMaterialPassIndex = 0;

            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

            cmd.SetRenderTarget(objectIDTexture);
            cmd.ClearRenderTarget(true, true, Color.clear);


            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        // No need to release objectIDTexture here as it's managed by RTHandles system
    }

    public RTHandle GetObjectIDTexture()
    {
        return objectIDTexture;
    }
}