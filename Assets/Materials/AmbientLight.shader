Shader "Custom/AmbientLight" {
    Properties {
        _SkyColor("Sky Color", Color)           = (0.8,0.8,0.8,1)
        _EquatorColor ("Equator Color", Color)  = (0.2,0.2,0.2,1)
        _GroundColor ("Ground Color", Color)    = (0.0,0.0,0.0,0)
    }
    SubShader {
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" // to use UnityObjectToClipPos and UnityObjectToWorldNormal

            fixed4 _SkyColor;
            fixed4 _EquatorColor;
            fixed4 _GroundColor;


            struct v2f {
                float4 pos : SV_POSITION;
                fixed3 color : COLOR0;
            };

            v2f vert (float4 vertex : POSITION, float3 normal : NORMAL)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                half3 worldNormal = UnityObjectToWorldNormal(normal);

                o.color = lerp(_EquatorColor, _SkyColor, worldNormal.y) * step(0, worldNormal.y);
                o.color += lerp(_EquatorColor, _GroundColor, abs(worldNormal.y)) * (1 - step(0, worldNormal.y));

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4 (i.color, 1);
            }
            ENDCG
        }
    }
}