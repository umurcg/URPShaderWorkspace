using UnityEngine;
using UnityEngine.Rendering;

namespace PostProcessPlayground
{
    [CreateAssetMenu(menuName = "Rendering/MyRenderPipelineAsset")]
    public class MyRenderPipelineAsset: RenderPipelineAsset
    {
        // Unity calls this method before rendering the first frame.
        // If a setting on the Render Pipeline Asset changes, Unity destroys the current Render Pipeline Instance and calls this method again before rendering the next frame.
        protected override RenderPipeline CreatePipeline() {
            // Instantiate the Render Pipeline that this custom SRP uses for rendering.
            return new MyRenderPipeline();
        }
    }
}