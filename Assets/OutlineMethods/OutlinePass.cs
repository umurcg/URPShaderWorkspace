using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlinePass : ScriptableRenderPass
{
    public OutlineFeature.OutlineSettings settings;
    public RTHandle tempTexture;
    public ObjectIDRenderPass objectIDPass;
        
    // Reference to the ObjectIDRenderFeature
    [SerializeField] private ObjectIDRenderFeature objectIDFeature;
        
        
    public OutlinePass()
    {
        profilingSampler = new ProfilingSampler("Outline_Pass");
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
        if (settings.outlineMaterial == null)
        {
            Debug.LogError("Outline material is null. Please assign a material in the OutlineFeature settings.");
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get();


        using (new ProfilingScope(cmd, profilingSampler))
        {

            var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            if (cameraColorTarget.rt == null)
            {
                // Debug.LogError("Camera color target is null. Skipping outline pass.");
                return;
            }

            if (objectIDPass != null && settings.useObjectIDMask)
            {
                var texture = objectIDPass.GetObjectIDTexture();
                settings.outlineMaterial.SetTexture("_Mask", texture);
                settings.outlineMaterial.SetTexture("_MainTex", cameraColorTarget);
                Blitter.BlitTexture(cmd, cameraColorTarget, tempTexture, settings.outlineMaterial, 0);
            }
            else
            {
                settings.outlineMaterial.SetTexture("_MainTex", cameraColorTarget);
                Blitter.BlitTexture(cmd, cameraColorTarget, tempTexture, settings.outlineMaterial, 0);
            }

            Blitter.BlitTexture(cmd, tempTexture, cameraColorTarget, settings.outlineMaterial, 0);
        }
        
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }


}