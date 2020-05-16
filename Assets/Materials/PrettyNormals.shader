Shader "Custom/PrettyNormals" {

    SubShader {
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" // to use UnityObjectToClipPos and UnityObjectToWorldNormal

            struct v2f {
                float4 pos : SV_POSITION;
                fixed3 color : COLOR0;
            };

            v2f vert (float4 vertex : POSITION, float3 normal : NORMAL)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                half3 worldNormal = UnityObjectToWorldNormal(normal);
                o.color = worldNormal * 0.5 + 0.5; // normals are [-1, 1] - convert to rgb [0, 1]
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