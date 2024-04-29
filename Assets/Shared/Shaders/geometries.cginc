// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
#if !defined(GEOM_INCLUDED)

#include "UnityCG.cginc"



struct GEOM_VertexData
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float3 color : COLOR;
};

struct Interpolator
{
	float2 uv : TEXCOORD0;
	float4 vertex : SV_POSITION;
	float3 color : COLOR; 
	float3 normal : TEXCOORD1;
	float4 tangent : TEXCOORD2;
	float3 worldPos : TEXCOORD3;
	float4 screenpos : TEXCOORD4;
	#if defined(SHADOWS_SCREEN)
		float4 shadowCoordinates : TEXCOORD5;
	#endif
	#if defined(VERTEXLIGHT_ON)
		float3 vertexLightColor : TEXCOORD6;
	#endif
	#if defined(LIGHTMAP_ON)
		float2 lightmapUV : TEXCOORD6;
	#endif
};

Interpolator GEOM_VertexProgram (GEOM_VertexData v)
{
	Interpolator i = (Interpolator)0;
	i.vertex = UnityObjectToClipPos(v.vertex);
	i.worldPos = mul(unity_ObjectToWorld, v.vertex);
	i.uv = v.uv.xy;
	i.normal = UnityObjectToWorldNormal(v.normal);
	i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
	i.screenpos = ComputeScreenPos(i.vertex);
	i.color = v.color;
	#if defined(SHADOWS_SCREEN)
		i.shadowCoordinates = ComputeScreenPos(i.vertex);
	#endif
	#if defined(LIGHTMAP_ON)
		i.lightmapUV = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
	#endif
	//ComputeVertexLightColor(i);
	return i;
}

Interpolator initv2f(appdata_full i) {
	Interpolator v = (Interpolator)0;
	v.uv = i.texcoord.xy;
	v.vertex = UnityObjectToClipPos(i.vertex);
	v.worldPos = mul(unity_ObjectToWorld, i.vertex);
	v.color = i.color;
	v.normal = mul(unity_ObjectToWorld, i.normal);
	v.tangent = float4(UnityObjectToWorldDir(i.tangent.xyz), i.tangent.w);
	v.screenpos = ComputeScreenPos(v.vertex);
	#if defined(SHADOWS_SCREEN)
		v.shadowCoordinates = ComputeScreenPos(v.vertex);
	#endif
	//ComputeVertexLightColor(v);
	return v;
}


Interpolator GEOM_ApplyWorldPosShadows(Interpolator i) {
	float4 localpos = mul(unity_WorldToObject, float4(i.worldPos, 1));
	i.vertex = UnityClipSpaceShadowCasterPos(localpos.xyz, i.normal);
	i.vertex = UnityApplyLinearShadowBias(i.vertex);
	i.normal = i.worldPos.xyz - _LightPositionRange.xyz;
	i.screenpos = ComputeScreenPos(i.vertex);
	#if defined(SHADOWS_SCREEN)
		i.shadowCoordinates = ComputeScreenPos(i.vertex);
	#endif
	return i;
}

Interpolator GEOM_ApplyWorldPos(Interpolator i) {
	#if defined(SHDPRG)
		return GEOM_ApplyWorldPosShadows(i);
	#endif
	float4 localpos = mul(unity_WorldToObject, float4(i.worldPos, 1));
	i.vertex = UnityObjectToClipPos(localpos);
	i.screenpos = ComputeScreenPos(i.vertex);
	#if defined(SHADOWS_SCREEN)
		i.shadowCoordinates = ComputeScreenPos(i.vertex);
	#endif
	return i;
}

float _GEOM_WindStrength;

Interpolator GEOM_ApplyWind(Interpolator i){
	float3 pos = i.worldPos.xyz;
	float2 local_wind = float2( cos(_Time.y + pos.x), sin(_Time.y + pos.y)) * _GEOM_WindStrength;
	float3 x = float3(1,0,0);
	float3 y = float3(0,0,1);
	i.worldPos.xyz += (normalize(x)*local_wind.x + normalize(y)*local_wind.y) * i.color.x;
	i = GEOM_ApplyWorldPos(i);
	return i;
}

Interpolator GEOM_Null(Interpolator i){
	return i;
}
#define GENGEOM(i) GEOM_Null(i)

#if defined(GENGEOM_WIND)

	#undef GENGEOM
	Interpolator GenerateGeometry(Interpolator i){
		i = GEOM_ApplyWind(i);
		//i.worldPos.y += 1;
		//i = GEOM_ApplyWorldPos(i);
		return i;
	}
	#define GENGEOM(i) GenerateGeometry(i)
#endif


Interpolator GEOM_GenQuadPoint(float3 center, float2 uv, float2 size, float3 axis, float3 tan, float3 normal) {
	Interpolator v = (Interpolator)0;
	float2 coords = uv - float2(0.5, 0);
	float3 pos = center + normalize(axis) * coords.y * size.y + normalize(tan) * coords.x * size.x;
	v.worldPos.xyz = pos;
	v.uv = uv;
	v.normal = normalize(normal);
	v = GEOM_ApplyWorldPos(v);
	if (uv.y < 0.5){
		v.color.x = 0;
	} else {
		v.color.x = 1;
	}
	#if defined(GENGEOM)
		v = GENGEOM(v); 
	#endif
	return v;
}


float _GEOM_Sprite_X;
float _GEOM_Sprite_Y;
float _GEOM_Sprite_Dist;

TriangleStream<Interpolator> GEOM_GenQuadUV(inout TriangleStream<Interpolator> OutputStream, float3 pos, float3 up, float3 tan, float3 normal, float2 size) {
	float d = distance(pos, _WorldSpaceCameraPos);
	if (d > _GEOM_Sprite_Dist + 5) return OutputStream;
	OutputStream.Append(GEOM_GenQuadPoint(pos, float2(0, 0), size, up, tan, normal));
	OutputStream.Append(GEOM_GenQuadPoint(pos, float2(1, 0), size, up, tan, normal));
	OutputStream.Append(GEOM_GenQuadPoint(pos, float2(0, 1), size, up, tan, normal));
	OutputStream.Append(GEOM_GenQuadPoint(pos, float2(1, 1), size, up, tan, normal));
	//OutputStream.Append(GEOM_GenQuadPoint(pos, float2(0, 0), up, tan, normal));
	//OutputStream.Append(GEOM_GenQuadPoint(pos, float2(1, 1), up, tan, normal));
	OutputStream.RestartStrip();
	return OutputStream;
}

bool GEOM_PointInCameraFrustrum(float3 p, float margin){
	float4 localpos = mul(unity_WorldToObject, float4(p, 1));
	float4 v = UnityObjectToClipPos(localpos);
	if (abs(v.x)-margin < v.w && abs(v.y)-margin < v.w && v.z >= 0){
		return true;
	}
	return false;
}

[maxvertexcount(12)]
void GEOM_GeneratedPlants (triangle Interpolator input[3], inout TriangleStream<Interpolator> OutputStream)
{
	float2 size = float2(_GEOM_Sprite_X, _GEOM_Sprite_Y);

	float margin = 0.1;

	float3 pos0 = input[0].worldPos.xyz;
	if (GEOM_PointInCameraFrustrum(pos0, margin)){
		float3 up0 = normalize(input[0].normal.xyz + input[1].normal.xyz); 
		float3 normal0 = normalize(input[1].worldPos.xyz - input[0].worldPos.xyz);
		float3 tan0 = normalize(cross(normal0, up0));
		OutputStream = GEOM_GenQuadUV(OutputStream, pos0, up0, tan0, normal0, size);
	}

	float3 pos1 = input[1].worldPos.xyz;
	if (GEOM_PointInCameraFrustrum(pos1, margin)){
		float3 up1 = normalize(input[1].normal.xyz + input[2].normal.xyz);
		float3 normal1 = normalize(input[2].worldPos.xyz - input[1].worldPos.xyz);
		float3 tan1 = normalize(cross(normal1, up1));
		OutputStream = GEOM_GenQuadUV(OutputStream, pos1, up1, tan1, normal1, size);
	}

	float3 pos2 = input[2].worldPos.xyz;
	if (GEOM_PointInCameraFrustrum(pos2, margin)){
		float3 up2 = normalize(input[2].normal.xyz + input[0].normal.xyz);
		float3 normal2 = normalize(input[0].worldPos.xyz - input[2].worldPos.xyz);
		float3 tan2 = normalize(cross(normal2, up2));
		OutputStream = GEOM_GenQuadUV(OutputStream, pos2, up2, tan2, normal2, size);
	}
}




float3 GEOM_Wind(float3 center) {
	return float3(cos((_Time.y + center.x) * 0.4) * 0.5 + sin((_Time.y + center.y) * 0.3) * 0.5 ,
		0, 
		sin((_Time.y + center.z) * 0.4) * 0.5 + cos((_Time.y + center.x) * 0.6) * 0.5)
		* 0.25;
}



float4 WorldToProj(float4 worldPos) {
	float4 result = mul(unity_WorldToObject, worldPos);
	result = UnityObjectToClipPos(result);
	return result;
}

float3 getviewdir(float3 pos) {
	return normalize(pos - _WorldSpaceCameraPos);
}

float3 getscreentangent(float3 pos, float3 axis) {
	return normalize(cross(getviewdir(pos), axis));
}

void billboardaxis(Interpolator p, inout TriangleStream<Interpolator> OutputStream, float3 axis, float width, float height) {
	float2 uvarray[4] = { float2(0, 0), float2(1, 0), float2(0, 1), float2(1, 1) };
	//float4 origin = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
	float3 tangent = getscreentangent(p.worldPos.xyz, axis);
	axis = normalize(axis) * height;
	tangent = normalize(tangent) * width;
	float3 normal = normalize(cross(axis, tangent));
	for (int i = 0; i < 4; i++) {
		float2 uv = uvarray[i];
		float3 pos = p.worldPos.xyz + uv.x * tangent + uv.y * axis;
		Interpolator v = (Interpolator)0;
		v.uv = uv;
		v.normal = mul(unity_WorldToObject, normal);
		float4 localpos = mul(unity_WorldToObject, pos);
		v.vertex = UnityObjectToClipPos(localpos);
		OutputStream.Append(v);
	}
}

void vert_billboard(appdata_full v, inout Interpolator o) {
	float3 objectCenter = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
	float3 view = normalize(objectCenter - _WorldSpaceCameraPos);
	float3 axis = float3(0, -1, 0);

	float3 viewTangent = normalize(cross(view, axis));
	float3 flatView = normalize(float3(view.x, 0, view.z));

	float3 viewUp = normalize(cross(view, viewTangent));
	axis = viewUp;

	float3 worldVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
	float3 localVertex = worldVertex - objectCenter;

	float3 newLocalVertex = viewTangent * localVertex.x + axis * localVertex.y + view * localVertex.z;
	float3 newWorldVertex = objectCenter + newLocalVertex;
	float4 newVertex = mul(unity_WorldToObject, float4(newWorldVertex, 1));

	o.vertex = UnityObjectToClipPos(newVertex);
}

void vert_billboard_axis(appdata_full v, inout Interpolator o, float3 axis) {
	float3 objectCenter = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
	float3 view = normalize(objectCenter - _WorldSpaceCameraPos);

	float3 viewTangent = normalize(cross(view, axis));
	//float3 flatView = normalize(float3(view.x, 0, view.z));

	//float3 viewUp = cross(view, viewTangent);
	//axis = viewUp;
	axis = normalize(axis);

	float3 worldVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
	float3 localVertex = worldVertex - objectCenter;

	float3 newLocalVertex = viewTangent * localVertex.x + axis * localVertex.y + view * localVertex.z;
	float3 newWorldVertex = objectCenter + newLocalVertex;
	float4 newVertex = mul(unity_WorldToObject, float4(newWorldVertex, 1));

	o.vertex = UnityObjectToClipPos(newVertex);
}

float3 TransformDirection(float3 vect) {
	float3 objectCenter = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
	float3 target = mul(unity_ObjectToWorld, float4(vect.x, vect.y, vect.z, 1)).xyz;
	float3 result = target - objectCenter;
	return normalize(result);
}

float3 TransformPoint(float3 coords) {
	float3 result = mul(unity_ObjectToWorld, float4(coords.x, coords.y, coords.z, 1)).xyz;
	return result;
}

float GEOM_GetDepth(float3 p) {
	float dist = distance(p, _WorldSpaceCameraPos);
	return clamp(1 - dist / (_ProjectionParams.z - _ProjectionParams.y), 0.0, 1.0);
}

float GEOM_GetDist(float3 p) {
	float dist = distance(p, _WorldSpaceCameraPos);
	return dist;
}

float3 GEOM_ToNormalMap(float3 n) {
	float3 proj = WorldToProj(float4(n, 1));
	proj = normalize(proj);
	return proj *0.5 + float3(0.5, 0.5, 0.5);
}

float3 GEOM_FromNormalMap(float3 c) {
	float3 n = c * 2 - float3(1, 1, 1);
	return normalize(n);
}

void GEOM_ClipTransparency(float alpha, Interpolator i){
	float4x4 thresholdMatrix =
	{  1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
		13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
		4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
		16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
	};
	float4x4 _RowAccess = { 1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1 };

	float2 screen_coords = i.screenpos.xy / i.screenpos.w;
	screen_coords *= _ScreenParams.xy;
	clip(alpha  - thresholdMatrix[fmod(screen_coords.x, 4)] * _RowAccess[fmod(screen_coords.y, 4)]);
}

#define GEOM_INCLUDED
#endif
