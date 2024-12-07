Shader "Custom/StandartLit"
{
    Properties
    {
        _BaseTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
    }
    SubShader
    {

        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalRenderPipeline"
        }
        LOD 200

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct app
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D(_BaseTex);
            SAMPLER(sampler_BaseTex);

            CBUFFER_START(UnityPerDraw)
                float4 _BaseColor;
            CBUFFER_END

            v2f vert(app v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, i.uv) * _BaseColor;
                return col;
            }
            ENDHLSL
        }
    }
}