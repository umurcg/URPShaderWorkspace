Shader "Custom/SimpleOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineThickness ("Outline Thickness", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        ENDHLSL

        Pass
        {
            Name "Outline"

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
            };

            TEXTURE2D(_MainText);
            SAMPLER(sampler_MainText);

            float4 _MainText_TexelSize;
            float4 _OutlineColor;
            float _OutlineThickness;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 texelSize = _MainText_TexelSize.xy * _OutlineThickness;
                float4 originalColor = SAMPLE_TEXTURE2D(_MainText, sampler_MainText, input.uv);
                
                float4 upColor = SAMPLE_TEXTURE2D(_MainText, sampler_MainText, input.uv + float2(0, texelSize.y));
                float4 downColor = SAMPLE_TEXTURE2D(_MainText, sampler_MainText, input.uv - float2(0, texelSize.y));
                float4 leftColor = SAMPLE_TEXTURE2D(_MainText, sampler_MainText, input.uv - float2(texelSize.x, 0));
                float4 rightColor = SAMPLE_TEXTURE2D(_MainText, sampler_MainText, input.uv + float2(texelSize.x, 0));

                float4 edgeColor = abs(upColor - downColor) + abs(leftColor - rightColor);
                float edgeStrength = length(edgeColor.rgb);

                return lerp(originalColor, _OutlineColor, edgeStrength);
            }
            ENDHLSL
        }
    }
}