﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#if !defined(MY_SHADOWS_INCLUDED)
#define MY_SHADOWS_INCLUDED

#include "UnityCG.cginc"

struct VertexData {
	float4 position : POSITION;
	float3 normal : NORMAL;
};

struct SHDW_Full_VertexData {
	float4 position : POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
};

struct SHDW_Full_Interpolator {
		float4 position : SV_POSITION;
		float3 normal : NORMAL;
		float2 uv : TEXCOORD0;
};

struct SHDW_Uni_Interpolator {
		float4 vertex : SV_POSITION;
		float3 normal : NORMAL;
		float2 uv : TEXCOORD0;
		float3 worldPos : TEXCOORD1;
		float3 worldNormal : TEXCOORD2;
		float4 screenpos : TEXCOORD4;
};


#if defined(SHADOWS_CUBE)
	struct Interpolators {
		float4 position : SV_POSITION;
		float3 lightVec : TEXCOORD0;
	};

	Interpolators MyShadowVertexProgram (VertexData v) {
		Interpolators i;
		i.position = UnityObjectToClipPos(v.position);
		i.lightVec =
			mul(unity_ObjectToWorld, v.position).xyz - _LightPositionRange.xyz;
		return i;
	}
	
	float4 MyShadowFragmentProgram (Interpolators i) : SV_TARGET {
		float depth = length(i.lightVec) + unity_LightShadowBias.x;
		depth *= _LightPositionRange.w;
		return UnityEncodeCubeShadowDepth(depth);
	}
#else
	float4 MyShadowVertexProgram (VertexData v) : SV_POSITION {
		float4 position =
			UnityClipSpaceShadowCasterPos(v.position.xyz, v.normal);
		return UnityApplyLinearShadowBias(position);
	}

	half4 MyShadowFragmentProgram () : SV_TARGET {
		return 0;
	}
#endif

#endif