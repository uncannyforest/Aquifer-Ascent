// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/VerticalOrientation2SplatMapSolid" {
    Properties {
        _MainTex ("Splat Map", 2D) = "white" {}
        _Color1 ("Color 1", Color) = (1.0,1.0,1.0,1)
        _Color2 ("Color 2", Color) = (1.0,1.0,1.0,1)
        _BumpMap1 ("Normal Map 1", 2D) = "bump" {}
        _BumpMap2 ("Normal Map 2", 2D) = "bump" {}
        _SpecularColor ("Specular", Color) = (0.0,0.0,0.0,1)
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 1.0
    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf StandardSpecular fullforwardshadows noambient novertexlights vertex:vert
        #pragma require interpolators15
        struct Input {
            float2 uv2_MainTex;
            float2 uv_BumpMap1;
            float2 uv_BumpMap2;
            float3 wNormal;
        };
        sampler2D _MainTex;
        fixed4 _Color1;
        fixed4 _Color2;
        sampler2D _BumpMap1;
        sampler2D _BumpMap2;
        fixed4 _SpecularColor;
        half _Smoothness;
        void vert (inout appdata_full v, out Input data) {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            data.wNormal = UnityObjectToWorldNormal(v.normal);
        }
        void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
            float3 normal = IN.wNormal * 0.5 + 0.5; // normals are [-1, 1] - convert to [0, 1]
            float3 splat = tex2D (_MainTex, float2(IN.uv2_MainTex.x, normal.y)).rgb;
            o.Albedo =  _Color1.rgb * splat.r
                    + _Color2.rgb * (1 - splat.r);
            o.Normal = 
                UnpackNormal (tex2D (_BumpMap1, IN.uv_BumpMap1)) * splat.r
                + UnpackNormal (tex2D (_BumpMap2, IN.uv_BumpMap2)) * (1 - splat.r);
            o.Specular = _SpecularColor;
            o.Smoothness = _Smoothness;
        }
        ENDCG
    }
    Fallback "Diffuse"
}