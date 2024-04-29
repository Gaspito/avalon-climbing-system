#if !defined(MY_LIGHTING_INCLUDED)
#define MY_LIGHTING_INCLUDED

#include "Assets/Shared/Shaders/geometries.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"



#include "UnityCG.cginc"
#include "UnityPBSLighting.cginc"

struct LF_VertexData
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
};
/*
struct LF_Interpolator
{
	float2 uv : TEXCOORD0;
	float4 vertex : SV_POSITION;
	float3 normal : TEXCOORD1;
	float4 tangent : TEXCOORD2;
	float3 worldPos : TEXCOORD3;
	#if defined(SHADOWS_SCREEN)
		float4 shadowCoordinates : TEXCOORD5;
	#endif
	#if defined(VERTEXLIGHT_ON)
		float3 vertexLightColor : TEXCOORD6;
	#endif
};
*/

Interpolator ComputeVertexLightColor (Interpolator i) {
	#if defined(VERTEXLIGHT_ON)
		i.vertexLightColor = Shade4PointLights(
			unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
			unity_LightColor[0].rgb, unity_LightColor[1].rgb,
			unity_LightColor[2].rgb, unity_LightColor[3].rgb,
			unity_4LightAtten0, i.worldPos, i.normal
		);
	#endif
	return i;
}

Interpolator LF_VertexProgram (GEOM_VertexData v)
{
	Interpolator i=(Interpolator)0;
	i.vertex = UnityObjectToClipPos(v.vertex);
	i.worldPos = mul(unity_ObjectToWorld, v.vertex);
	i.uv = v.uv.xy;
	i.normal = UnityObjectToWorldNormal(v.normal);
	i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
	i.screenpos = ComputeScreenPos(i.vertex);
	#if defined(SHADOWS_SCREEN)
		i.shadowCoordinates = ComputeScreenPos(i.vertex);
	#endif
	i = ComputeVertexLightColor(i);
	return i;
}
Interpolator LF_VertexProgramHair (GEOM_VertexData v)
{
	Interpolator i;
	i.vertex = UnityObjectToClipPos(v.vertex);
	i.worldPos = mul(unity_ObjectToWorld, v.vertex);
	i.uv = v.uv.xy;
	float4 origin = mul(unity_ObjectToWorld, float4(0,0,0,1));
	i.normal = normalize(i.worldPos.xyz - origin.xyz);
	i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
	i.screenpos = ComputeScreenPos(i.vertex);
	#if defined(SHADOWS_SCREEN)
		i.shadowCoordinates = ComputeScreenPos(i.vertex);
	#endif
	i = ComputeVertexLightColor(i);
	return i;
}

Texture2D _DiffuseTex;
Texture2D _NormalTex;
Texture2D _SpecularTex;
SamplerState sampler_DiffuseTex;
float _Smoothness;
float _Metallic;
float _BumpScale;


/*
LF_Interpolator LF_VertexProgram (LF_VertexData v)
{
	LF_Interpolator i;
	i.vertex = UnityObjectToClipPos(v.vertex);
	i.worldPos = mul(unity_ObjectToWorld, v.vertex);
	i.uv = v.uv;
	i.normal = UnityObjectToWorldNormal(v.normal);
	i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

	#if defined(SHADOWS_SCREEN)
		i.shadowCoordinates = ComputeScreenPos(i.vertex);
	#endif
	ComputeVertexLightColor(i);
	return i;
}
*/

sampler2D _CameraDepthTexture;
#if defined(CUSTOM_SHADOWS)
	Texture2D _CustomShadowsTex;
	float _CustomShadowsStrength = 0;
#endif

UnityLight CreateLight (Interpolator i) {
	UnityLight light;
	#if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
		light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
	#else
		light.dir = _WorldSpaceLightPos0.xyz;
	#endif
	#if defined(SHADOWS_SCREEN)
		float attenuation = tex2D(_ShadowMapTexture, i.shadowCoordinates.xy / i.shadowCoordinates.w);
		
	#else
		UNITY_LIGHT_ATTENUATION(attenuation, 0, i.worldPos);
	#endif
	#if defined(CUSTOM_SHADOWS)
		if (_CustomShadowsStrength > 0){
			fixed4 custom_shadows = _CustomShadowsTex.Sample(sampler_DiffuseTex, i.screenpos.xy / i.screenpos.w);
			float t = (1.0/3.0);
			float cs = custom_shadows.r; // * t + custom_shadows.g *(t*2.0) + custom_shadows.b * (t*3.0);
			float depth = tex2D(_CameraDepthTexture, i.screenpos.xy / i.screenpos.w).r;
			if (depth < cs){
				attenuation = -0.1;
			}
			//attenuation *= custom_shadows.r;
		}
	#endif
	light.color = _LightColor0.rgb * attenuation;
	light.ndotl = DotClamped(i.normal, light.dir);
	return light;
}

UnityIndirect CreateIndirectLight (Interpolator i) {
	UnityIndirect indirectLight;
	indirectLight.diffuse = 0;
	indirectLight.specular = 0;

	#if defined(VERTEXLIGHT_ON)
		indirectLight.diffuse = i.vertexLightColor;
	#endif

	#if defined(FORWARD_BASE_PASS)
		#if defined(LIGHTMAP_ON)
			indirectLight.diffuse = DecodeLightmap( UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV));
		#else
			indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
		#endif
	#endif

	return indirectLight;
}

float3 CreateBinormal (float3 normal, float3 tangent, float binormalSign) {
	return cross(normal, tangent.xyz) *
		(binormalSign * unity_WorldTransformParams.w);
}

void InitializeFragmentNormal(inout Interpolator i) {
	float3 normal = UnpackScaleNormal(_NormalTex.Sample(sampler_DiffuseTex, i.uv), _BumpScale);
	float3 tangentSpaceNormal = normal;

	float binormal = cross(i.normal, i.tangent.xyz) * i.tangent.w;

	i.normal = normalize(
		tangentSpaceNormal.x * i.tangent +
		tangentSpaceNormal.y * binormal +
		tangentSpaceNormal.z * i.normal
	);
}

float3 LF_GetNormal(Interpolator i, Texture2D map, float2 mapping, float scale){
	float3 normal = UnpackScaleNormal(map.Sample(sampler_DiffuseTex, i.uv * mapping), scale);
	float3 tangentSpaceNormal = normal;

	float binormal = cross(i.normal, i.tangent.xyz) * i.tangent.w;

	float3 r = normalize(
		tangentSpaceNormal.x * i.tangent +
		tangentSpaceNormal.y * binormal +
		tangentSpaceNormal.z * i.normal
	);
	return r;
}

fixed3 LF_Default(Interpolator i, fixed3 albedo, fixed3 specular){
	fixed3 result = (fixed3)0;
	float3 specularTint = fixed3(1,1,1);
	float oneMinusReflectivity;

	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
	specularTint *= specular;

	albedo = DiffuseAndSpecularFromMetallic(
		albedo, _Metallic, specularTint, oneMinusReflectivity
	);


	result = UNITY_BRDF_PBS(
		albedo, specularTint,
		oneMinusReflectivity, _Smoothness,
		i.normal, viewDir,
		CreateLight(i), CreateIndirectLight(i)
	);

	return result;
}

fixed3 LF_Default(Interpolator i, fixed3 albedo, fixed3 specular, float specular_intensity){
	fixed3 result = (fixed3)0;
	float3 specularTint = fixed3(1,1,1);
	float oneMinusReflectivity;

	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
	specularTint *= specular;

	albedo = DiffuseAndSpecularFromMetallic(
		albedo, _Metallic, specularTint, oneMinusReflectivity
	);


	result = UNITY_BRDF_PBS(
		albedo, specularTint * specular_intensity,
		oneMinusReflectivity, _Smoothness,
		i.normal, viewDir,
		CreateLight(i), CreateIndirectLight(i)
	);

	return result;
}

fixed3 LF_OnlyShadows(Interpolator i, fixed3 albedo){
	fixed3 result = (fixed3)0;
	float3 specularTint;
	float oneMinusReflectivity;

	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

	albedo = DiffuseAndSpecularFromMetallic(
		albedo, _Metallic, specularTint, oneMinusReflectivity
	);

	UnityLight light = CreateLight(i);
	UnityIndirect indirect = CreateIndirectLight(i);

	fixed3 diffuse = saturate(light.color + indirect.diffuse);

	result = albedo * diffuse;

	return result;
}

fixed3 LF_HairModel(Interpolator i, fixed3 albedo){
	fixed3 result = (fixed3)0;
	float3 specularTint = (fixed3)0;
	float oneMinusReflectivity;

	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

	albedo = DiffuseAndSpecularFromMetallic(
		albedo, _Metallic, specularTint, oneMinusReflectivity
	);

	UnityLight light = CreateLight(i);
	UnityIndirect indirect = CreateIndirectLight(i);
	light.ndotl = 1;

	result = UNITY_BRDF_PBS(
		albedo, specularTint,
		oneMinusReflectivity, _Smoothness,
		i.normal, -i.normal,
		CreateLight(i), CreateIndirectLight(i)
	);

	return result;
}
			
fixed4 LF_FragmentProgram (Interpolator i) : SV_Target
{
	InitializeFragmentNormal(i);

	i.normal = normalize(i.normal);
	fixed4 diffusemap = _DiffuseTex.Sample(sampler_DiffuseTex, i.uv);
	fixed4 specularmap = _SpecularTex.Sample(sampler_DiffuseTex, i.uv);
	fixed3 lighting = LF_Default(i, diffusemap.rgb, specularmap.rgb); 
	diffusemap.rgb *= lighting;
	return diffusemap;
}

#endif
