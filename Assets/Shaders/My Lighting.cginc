#if !defined(MY_LIGHTING_INCLUDED)
#define MY_LIGHTING_INCLUDED

#include "AutoLight.cginc"
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

UnityLight CreateLight (v2f i) {
	UnityLight light;
	light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
    UNITY_LIGHT_ATTENUATION(attenuation, 0, i.worldPos);
	light.color = _LightColor0.rgb * attenuation;
	light.ndotl = DotClamped(i.normal, light.dir);
	return light;
}

fixed4 frag (v2f i) : SV_Target {
    i.normal = normalize(i.normal);

    float oneMinusReflectivity;

    half3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

    half3 diffuse = EnergyConservationBetweenDiffuseAndSpecular(
        diffuse, _SpecularTint.rgb, oneMinusReflectivity
    );

    UnityIndirect indirectLight;
    indirectLight.diffuse = 0;
    indirectLight.specular = 0;

    return UNITY_BRDF_PBS(
        i.color, _SpecularTint,
        oneMinusReflectivity, _Smoothness,
        i.normal, viewDir,
        CreateLight(i), indirectLight
        );
}
#endif