Shader "Custom/FullscreenMaskedOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 1
        _OutlineThreshold ("Outline Threshold", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        ENDHLSL

        Pass
        {
            Name "Outline"
            ZTest Always
            ZWrite Off
            Cull Off

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
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                float4 _OutlineColor;
                float _OutlineWidth;
                float _OutlineThreshold;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float4 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                float mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv).r;

                float2 texelSize = _MainTex_TexelSize.xy * _OutlineWidth;
                float maxDifference = 0;

                // Check neighboring pixels
                for (int x = -1; x <= 1; ++x)
                {
                    for (int y = -1; y <= 1; ++y)
                    {
                        if (x == 0 && y == 0) continue; // Skip center pixel
                        float2 offsetUV = uv + float2(x, y) * texelSize;
                        float neighborMask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, offsetUV).r;
                        maxDifference = max(maxDifference, abs(neighborMask - mask));
                    }
                }

                // Calculate outline intensity
                float outlineIntensity = step(_OutlineThreshold, maxDifference);

                // Blend outline with main color
                float3 finalColor = lerp(mainColor.rgb, _OutlineColor.rgb, outlineIntensity * _OutlineColor.a);
                
                return float4(finalColor, 1);
            }
            ENDHLSL
        }
    }
}