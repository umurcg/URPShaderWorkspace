Shader "Custom/SilhouetteEffect"
{
    Properties
    {
        _ForegroundColor ("Foreground Color", Color) = (1,1,1,1)
        _BackgroundColor ("Background Color", Color) = (0,0,0,0)
        _ToneScale ("Tone Scale", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "RenderPipeline" = "UniversalRenderPipeline" "Queue"="Transparent"
        }
        LOD 200

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #pragma vertex vert
            #pragma fragment frag

            struct VertexInput
            {
                float4 positionCS : POSITION;
            };

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
                float4 positionSS : TEXCOORD0;
            };


            CBUFFER_START(UnityPerMaterial)
                float4 _ForegroundColor;
                float4 _BackgroundColor;
                float _ToneScale;
            CBUFFER_END

            FragmentInput vert(VertexInput v)
            {
                FragmentInput o;
                o.positionCS = TransformObjectToHClip(v.positionCS);
                o.positionSS = ComputeScreenPos(o.positionCS);
                return o;
            }

            half4 frag(FragmentInput i) : SV_Target
            {
                const float2 screenUV = i.positionSS.xy / i.positionSS.w;

                //Sample depth texture with screenUV
                const float rawDepth= SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV);
                float depth = Linear01Depth(rawDepth, _ZBufferParams);
                depth = saturate(depth * _ToneScale);
                
                return lerp(_ForegroundColor, _BackgroundColor, depth);
            }
            ENDHLSL
        }
    }
}