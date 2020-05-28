Shader "Custom/Specular" {
    Properties {
        _Color("Albedo", Color)           = (1,1,1,1)
		_SpecularTint ("Specular", Color) = (0.5, 0.5, 0.5)
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
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
			#include "UnityStandardUtils.cginc"

            fixed4 _Color;
            fixed4 _SpecularTint;
			fixed _Smoothness;

            struct v2f {
                float4 pos : SV_POSITION;
                fixed3 color : COLOR0;
                half3 normal : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
            };

            v2f vert (float4 vertex : POSITION, float3 normal : NORMAL) {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
				o.worldPos = mul(unity_ObjectToWorld, vertex);
                o.normal = UnityObjectToWorldNormal(normal);
                o.color = _Color;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
				i.normal = normalize(i.normal);
                half3 lightDir = _WorldSpaceLightPos0.xyz;
                half3 lightColor = _LightColor0.rgb;

                float oneMinusReflectivity;

                half3 diffuse = i.color * lightColor * DotClamped(lightDir, i.normal);                
				diffuse = EnergyConservationBetweenDiffuseAndSpecular(
					diffuse, _SpecularTint.rgb, oneMinusReflectivity
				);

                half3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                half3 halfVector = normalize(lightDir + viewDir);
				half3 specular = _SpecularTint.rgb * lightColor * pow(
                    DotClamped(halfVector, i.normal),
                    1 / (1 - _Smoothness)
                );

                return fixed4(diffuse + specular, 1);
            }
            ENDCG
        }
    }
}