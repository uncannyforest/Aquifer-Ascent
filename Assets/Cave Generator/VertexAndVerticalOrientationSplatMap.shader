// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/VertexAndVerticalOrientationSplatMap" {
    Properties {
        _MainTex ("Splat Map", 2D) = "white" {}
        _Color1 ("Color 1", Color) = (1.0,1.0,1.0,1)
        _Color2 ("Color 2", Color) = (1.0,1.0,1.0,1)
        _Color3 ("Color 3", Color) = (1.0,1.0,1.0,1)
        _Color4 ("Color 4", Color) = (1.0,1.0,1.0,1)
        _Walls1 ("Walls 1", Color) = (1.0,1.0,1.0,1)
        _Walls2 ("Walls 2", Color) = (1.0,1.0,1.0,1)
        _Walls3 ("Walls 3", Color) = (1.0,1.0,1.0,1)
        _Walls4 ("Walls 4", Color) = (1.0,1.0,1.0,1)
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
			float3 vertColor : COLOR;
        };
        sampler2D _MainTex;
        fixed4 _Color1;
        fixed4 _Color2;
        fixed4 _Color3;
        fixed4 _Color4;
        fixed4 _Walls1;
        fixed4 _Walls2;
        fixed4 _Walls3;
        fixed4 _Walls4;
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
            float3 texSplat1 = _Color1.rgb * splat.r + _Walls1.rgb * (1 - splat.r);
            float3 texSplat2 = _Color2.rgb * splat.r + _Walls2.rgb * (1 - splat.r);
            float3 texSplat3 = _Color3.rgb * splat.r + _Walls3.rgb * (1 - splat.r);
            float3 texSplat4 = _Color4.rgb * splat.r + _Walls4.rgb * (1 - splat.r);
            float3 vs4 = (IN.vertColor.r + IN.vertColor.g + IN.vertColor.b - 1) / 2;
            o.Albedo = texSplat1 * (IN.vertColor.r - vs4)
                + texSplat2 * (IN.vertColor.g - vs4)
                + texSplat3 * (IN.vertColor.b - vs4)
                + texSplat4 * vs4;
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