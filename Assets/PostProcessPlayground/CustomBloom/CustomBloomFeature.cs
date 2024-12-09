using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PostProcessPlayground.CustomBloom
{
    public class CustomBloomFeature: ScriptableRendererFeature
    {
        class CustomBloomPass: ScriptableRenderPass
        {
            
            public CustomBloomPass()
            {
                
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                base.Execute(context, ref renderingData);
            }
        }
        
        public override void Create()
        {
            throw new System.NotImplementedException();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            throw new System.NotImplementedException();
        }
    }
}