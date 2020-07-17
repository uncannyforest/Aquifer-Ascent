Shader "Custom/SplatMap" {
    Properties {
        _MainTex ("Splat Map", 2D) = "white" {}
        _Texture1 ("Texture 1", 2D) = "white" {}
        _Texture2 ("Texture 2", 2D) = "white" {}
		_Texture3 ("Texture 3", 2D) = "white" {}
        _BumpMap1 ("Normal Map 1", 2D) = "bump" {}
        _BumpMap2 ("Normal Map 2", 2D) = "bump" {}
        _BumpMap3 ("Normal Map 3", 2D) = "bump" {}
        _SpecularColor ("Specular", Color) = (0.0,0.0,0.0,1)
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 1.0
    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf StandardSpecular fullforwardshadows noambient novertexlights
        #pragma require interpolators15
        struct Input {
            float2 uv2_MainTex;
            float2 uv_Texture1;
            float2 uv_Texture2;
            float2 uv_Texture3;
            float2 uv_BumpMap1;
            float2 uv_BumpMap2;
            float2 uv_BumpMap3;
        };
        sampler2D _MainTex;
        sampler2D _Texture1;
        sampler2D _Texture2;
        sampler2D _Texture3;
        sampler2D _BumpMap1;
        sampler2D _BumpMap2;
        sampler2D _BumpMap3;
        fixed4 _SpecularColor;
        half _Smoothness;
        void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
            float3 splat = tex2D (_MainTex, IN.uv2_MainTex).rgb;
            o.Albedo = tex2D (_Texture1, IN.uv_Texture1).rgb * splat.r
                    + tex2D (_Texture2, IN.uv_Texture2).rgb * splat.g
                    + tex2D (_Texture3, IN.uv_Texture3).rgb * splat.b;
            o.Normal = 
                UnpackNormal (tex2D (_BumpMap1, IN.uv_BumpMap1)) * splat.r
                + UnpackNormal (tex2D (_BumpMap2, IN.uv_BumpMap2)) * splat.g
                + UnpackNormal (tex2D (_BumpMap3, IN.uv_BumpMap3)) * splat.b;
            o.Specular = _SpecularColor;
            o.Smoothness = _Smoothness;
        }
        ENDCG
    }
    Fallback "Diffuse"
}