Shader "Custom/Basic" {
    Properties {
        _Color("Albedo", Color)           = (1,1,1,1)
    }
    SubShader {
        Pass {
			Tags {
				"LightMode" = "ForwardBase"
			}

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityStandardBRDF.cginc"

            fixed4 _Color;

            struct v2f {
                float4 pos : SV_POSITION;
                fixed3 color : COLOR0;
                half3 normal : TEXCOORD1;
            };

            v2f vert (float4 vertex : POSITION, float3 normal : NORMAL) {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                o.normal = UnityObjectToWorldNormal(normal);
                o.color = _Color;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                half3 lightDir = _WorldSpaceLightPos0.xyz;
                half3 lightColor = _LightColor0.rgb;
                half3 diffuse = i.color * lightColor * DotClamped(lightDir, i.normal);
                return fixed4(diffuse, 1);
            }
            ENDCG
        }
    }
}