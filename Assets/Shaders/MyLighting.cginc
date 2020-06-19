#if !defined(MY_LIGHTING_INCLUDED)
#define MY_LIGHTING_INCLUDED

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

fixed4 _Color;
fixed4 _SpecularTint;
fixed _Smoothness;

struct VertexData {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
};

struct v2f {
    float4 pos : SV_POSITION;
    fixed3 color : COLOR0;
    half3 normal : TEXCOORD1;
    float3 worldPos : TEXCOORD2;

    SHADOW_COORDS(5)
};

v2f vert (VertexData v) {
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.color = _Color;

    TRANSFER_SHADOW(o);

    return o;
}

UnityLight CreateLight (v2f i) {
	UnityLight light;

	#if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
		light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
	#else
		light.dir = _WorldSpaceLightPos0.xyz;
	#endif

	UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);

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