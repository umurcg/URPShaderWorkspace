using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class MyRenderPipeline : RenderPipeline
{
    public MyRenderPipeline()
    {
        // This method is called when the Render Pipeline is created.
        // You can use it to initialize the Render Pipeline.
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        // This method is called once per frame for each camera.
        // You can use it to implement your rendering logic.

        var cmd = new CommandBuffer();
        cmd.ClearRenderTarget(true, true, Color.black);
        context.ExecuteCommandBuffer(cmd);
        cmd.Release();
        
        context.Submit();

        foreach (Camera camera in cameras)
        {
            camera.TryGetCullingParameters(out var cullingParameters);
            context.Cull(ref cullingParameters);
        }
        
        context.Submit();

    }
}