Shader "Unlit/PolarCoordinates"
{
    Properties
    {
        _BaseTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _RadialScale ("Radial Scale", Float) = 1
        _AngularScale ("Angular Scale", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"  "RenderPipeline" = "UniversalRenderPipeline" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragmentInput
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D(_BaseTex);
            SAMPLER(sampler_BaseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseTex_ST;
                float4 _BaseColor;
                float4 _Center;
                float _RadialScale;
                float _AngularScale;
            CBUFFER_END

          

            FragmentInput vert (VertexInput v)
            {
                FragmentInput fragment_input;
                fragment_input.vertex = TransformObjectToHClip(v.vertex);
                fragment_input.uv = TRANSFORM_TEX(v.uv, _BaseTex);
                return fragment_input;
            }

              
            float2 CartesianToPolar(float2 cartesian)
            {
                float2 offset = cartesian - _Center.xy;
                float r = length(offset) * 2;
                float theta = atan2(offset.y, offset.x) / (2*PI);
                float2 polarUV= float2(r , theta);
                polarUV.x *= _RadialScale;
                polarUV.y *= _AngularScale;
                return polarUV;
            }

            half4 frag(FragmentInput i) : SV_Target
            {
                float2 polar = CartesianToPolar(i.uv);
                half4 col = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, polar);
                col *= _BaseColor;
                return col;
                
            }
            ENDHLSL
        }
    }
}
