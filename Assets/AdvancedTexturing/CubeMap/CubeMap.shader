Shader "Custom/CubeMap"
{
    Properties
    {
        _CubeMap("CubeMap", CUBE) = "" {}

    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalRenderPipeline" "Queue"="Geometry"
        }
        LOD 200

        Pass
        {

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            samplerCUBE _CubeMap;


            struct VertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
                float3 normal : TEXCOORD0;
            };

            FragmentInput vert(VertexInput v)
            {
                FragmentInput o;
                o.positionCS = TransformObjectToHClip(v.vertex);
                o.normal = TransformObjectToWorldNormal(v.normal);
                return o;
            }

            half4 frag(FragmentInput i) : SV_Target
            {
                float4 color = texCUBE(_CubeMap, i.normal);
                return color;
            }
            ENDHLSL
        }
    }
}