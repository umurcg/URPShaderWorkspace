Shader "Custom/TriplanarMapping"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainColor ("Main Color", Color) = (1,1,1,1)
        _Tile ("Tile", Float) = 1
        _Blend ("Blend", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalRenderPipeline" "Queue"="Geometry"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float3 normal : NORMAL;
            };

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 positionOS : TEXCOORD2;
                float3 normal : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainColor;
                float _Tile;
                float _Blend;
            CBUFFER_END

            FragmentInput vert(VertexInput v)
            {
                FragmentInput o;
                o.positionOS = v.positionOS;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionWS = TransformObjectToWorld(v.positionOS);
                o.normal = TransformWorldToObjectNormal(v.normal);
                
                return o;
            }

            half4 frag(FragmentInput i) : SV_Target
            {
                float2 uvX = i.positionWS.yz * _Tile;
                float2 uvY = i.positionWS.zx * _Tile;
                float2 uvZ = i.positionWS.xy * _Tile;

                float3 colorX = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvX).rgb;
                float3 colorY = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvY).rgb;
                float3 colorZ = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvZ).rgb;

                float3 weight=pow(abs(i.normal),_Blend);

                float3 color = colorX * weight.x + colorY * weight.y + colorZ * weight.z;
                return float4(color, 1);
            }
            ENDHLSL
        }
    }
}