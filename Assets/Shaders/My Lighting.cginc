#if !defined(MY_LIGHTING_INCLUDED)
#define MY_LIGHTING_INCLUDED

#include "UnityPBSLighting.cginc"

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

    half3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

    half3 diffuse = EnergyConservationBetweenDiffuseAndSpecular(
        diffuse, _SpecularTint.rgb, oneMinusReflectivity
    );

    UnityLight light;
    light.color = lightColor;
    light.dir = lightDir;
    light.ndotl = DotClamped(i.normal, lightDir);
    UnityIndirect indirectLight;
    indirectLight.diffuse = 0;
    indirectLight.specular = 0;

    return UNITY_BRDF_PBS(
        i.color, _SpecularTint,
        oneMinusReflectivity, _Smoothness,
        i.normal, viewDir,
        light, indirectLight
        );
}
#endif