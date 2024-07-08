using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostProcessRenderFeature : ScriptableRendererFeature
{
    private CustomPostProcessPass _customPostProcessPass;
    public Shader bloomShader;
    public Shader compositeShader;
    
    private Material _bloomMaterial;
    private Material _compositeMaterial;
    
    public override void Create()
    {
        _bloomMaterial = CoreUtils.CreateEngineMaterial(bloomShader);
        _compositeMaterial = CoreUtils.CreateEngineMaterial(compositeShader);
        
        _customPostProcessPass = new CustomPostProcessPass(_bloomMaterial, _compositeMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        
        renderer.EnqueuePass(_customPostProcessPass);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        CoreUtils.Destroy(_bloomMaterial);
        CoreUtils.Destroy(_compositeMaterial);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        base.SetupRenderPasses(renderer, in renderingData);
        // if (renderingData.cameraData.camera.cameraType == CameraType.Game)
        // {
        //     _customPostProcessPass.ConfigureInput(ScriptableRenderPassInput.Depth);
        //     _customPostProcessPass.ConfigureInput(ScriptableRenderPassInput.Color);
        //     _customPostProcessPass.ConfigureTarget(renderer.cameraColorTarget, renderer.cameraDepthTarget);
        //     
        // }
    }
}
