Shader "Custom/ToonyShaderCode"
{
    Properties
    {
        _BaseTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _ShadeColor ("Shade Color", Color) = (0,0,0,1)
        _BandCount ("Band Count", Float) = 4
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct FragmentInput
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            SAMPLER(sampler_BaseTex);
            TEXTURE2D(_BaseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _ShadeColor;
                float3 _MainLightDirection;
                float _BandCount;
            CBUFFER_END

            FragmentInput vert(const VertexInput v)
            {
                FragmentInput o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                o.normal = TransformObjectToWorldNormal(v.normal);
                return o;
            }

            half4 frag(const FragmentInput i) : SV_Target
            {
                // Sample the texture and apply the base color
                const half4 col = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, i.uv) * _BaseColor;

                const Light mainLight = GetMainLight();

                // Normalize the normal and light direction
                const float3 normal = normalize(i.normal);
                const float3 lightDir = normalize(mainLight.direction);

                // Calculate the dot product (N·L) for lighting
                float n_dot_l = dot(normal, lightDir);

                // Map dot product to [0, 1]
                n_dot_l = (n_dot_l + 1.0) / 2.0;

                // Calculate banded shading
                const float shade = floor(n_dot_l * _BandCount) / _BandCount;
                
                // Blend between base color and shade color based on bands
                const half4 shadedColor = lerp(_ShadeColor, col, shade);

                // Return the final color
                return shadedColor;
            }
            ENDHLSL

        }

    }
}