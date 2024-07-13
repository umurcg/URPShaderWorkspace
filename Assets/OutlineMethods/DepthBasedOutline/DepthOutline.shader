Shader "Custom/DepthOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0.1, 10)) = 1
        _DepthThreshold ("Depth Threshold", Range(0.0001, 0.1)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        ENDHLSL

        Pass
        {
            Name "Depth Outline"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewSpacePos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _OutlineThickness;
            float _DepthThreshold;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.viewSpacePos = TransformWorldToView(TransformObjectToWorld(input.positionOS.xyz));
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 originalColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                float2 screenUV = input.positionHCS.xy / _ScaledScreenParams.xy;
                float depth = LoadSceneDepth(screenUV);
                float linear01Depth = Linear01Depth(depth, _ZBufferParams);
                
                float2 uvOffset = _MainTex_TexelSize.xy * _OutlineThickness;
                
                float depthDifference = 0;
                
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        
                        float2 offset = float2(x, y) * uvOffset;
                        float neighborDepth = LoadSceneDepth(screenUV + offset);
                        float neighborLinearDepth = Linear01Depth(neighborDepth, _ZBufferParams);
                        
                        depthDifference = max(depthDifference, abs(linear01Depth - neighborLinearDepth));
                    }
                }
                
                float outline = step(_DepthThreshold, depthDifference);
                
                return lerp(originalColor, _OutlineColor, outline);
            }
            ENDHLSL
        }
    }
}