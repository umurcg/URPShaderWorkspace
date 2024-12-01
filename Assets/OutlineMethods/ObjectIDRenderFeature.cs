using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ObjectIDRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material objectIDMaterial;
    }

    public Settings settings = new Settings();
    private ObjectIDRenderPass objectIDPass;


    public override void Create()
    {
        objectIDPass = new ObjectIDRenderPass(settings.objectIDMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.objectIDMaterial == null)
        {
            Debug.LogWarningFormat(
                "Missing Object ID Material. {0} render pass will not execute. Check for missing reference in the assigned renderer.",
                GetType().Name);
            return;
        }

        renderer.EnqueuePass(objectIDPass);
    }
}