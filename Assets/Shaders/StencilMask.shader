Shader "Custom/IntersectionMaskShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}   
        _StencilRef ("Stencil Ref", Int) = 1
    }
    SubShader
    {
        
        ColorMask 0
        ZWrite Off

        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }
//
//
//        Stencil
//        {
//            Ref [_StencilRef]
//            Comp always
//            Pass replace
//        }
//        
        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;

        
        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = float4(1,1,1,1);
            o.Alpha = 1;
            
        }

        ENDCG
        

    }
    Fallback "Unlit/Texture"
}