Shader "Custom/GoochShadingModel"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _HighlightColor ("Highlight Color", Color) = (1, 1, 1, 1)
        
    }
    SubShader
    {
        Tags {"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags {"LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float3 positionWS   : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _HighlightColor;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 color = _BaseColor;
                
                
                
                float3 colorCool = .25f*color + float3(0.0f, 0.0f, 0.55f);
                float3 colorWarm = .25f*color + float3(0.3f, 0.3f, 0.0f);

                float3 normalWS = normalize(IN.normalWS);
                float3 positionWS = IN.positionWS;

                // Calculate lighting
                float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                float3 lightDir = mainLight.direction;

                float t = (dot(normalWS, lightDir) + 1) / 2;
                // float reflection = 2 * (dot(normalWS, lightDir)) * normalWS - lightDir;
                float3 reflection = reflect(-lightDir, normalWS);
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.positionWS);
                // float s = saturate(100 * dot(reflection, viewDir) - 97);
                 float s = saturate( dot(reflection, viewDir));

                float3 finalColor = lerp(colorCool, colorWarm, t);
                // finalColor = lerp(_HighlightColor, finalColor, s);
                
                return float4(finalColor, 1);
            }
            ENDHLSL
        }
        
    }
}