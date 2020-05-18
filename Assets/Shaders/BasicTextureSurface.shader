Shader "Custom/BasicTextureSurface" {
    Properties {
        _Color ("Main Color", Color) = (1,0.5,0.5,1)
        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _SpecularColor ("Specular", Color) = (0.0,0.0,0.0,1)
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 1.0
    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf StandardSpecular fullforwardshadows noambient novertexlights
        struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
        };
        fixed4 _Color;
        sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4 _SpecularColor;
        half _Smoothness;
        void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
            o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb * _Color.rgb;
            o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
            o.Specular = _SpecularColor;
            o.Smoothness = _Smoothness;
        }
        ENDCG
    }
    Fallback "Diffuse"
}