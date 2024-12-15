Shader "Custom/CubeReflections"
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
                float3 reflectionWS : TEXCOORD0;
            };

            FragmentInput vert(VertexInput v)
            {
                FragmentInput o;

                o.positionCS = TransformObjectToHClip(v.vertex);
                float3 positionWS = TransformObjectToWorld(v.vertex);
                float3 normalWS = TransformObjectToWorldNormal(v.normal);
                float3 viewDirWS=GetWorldSpaceViewDir(positionWS);
                o.reflectionWS = reflect(-viewDirWS, normalWS);
                return o;
            }

            half4 frag(FragmentInput i) : SV_Target
            {
                float4 color = texCUBE(_CubeMap, i.reflectionWS);
                return color;
            }
            ENDHLSL
        }
    }
}