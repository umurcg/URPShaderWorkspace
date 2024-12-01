using UnityEngine;
using UnityEngine.Rendering.Universal;

public class OutlineFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class OutlineSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material outlineMaterial = null;
        public bool useObjectIDMask;
        public Material objectIDMaterial;
    }

    public OutlineSettings settings = new OutlineSettings();

 
    protected ObjectIDRenderPass objectIDPass;
    private OutlinePass outlinePass;

    public override void Create()
    {
        objectIDPass = new ObjectIDRenderPass(settings.objectIDMaterial);
        outlinePass = new OutlinePass()
        {
            settings = settings,
            objectIDPass = objectIDPass,
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(objectIDPass);
        renderer.EnqueuePass(outlinePass);
    }

    protected override void Dispose(bool disposing)
    {
        outlinePass?.tempTexture?.Release();
    }
}