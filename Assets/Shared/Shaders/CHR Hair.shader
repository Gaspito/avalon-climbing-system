Shader "CHR/Hair"
{
	Properties
	{
		_DiffuseTex ("Diffuse", 2D) = "white" {}
		_NormalTex ("Normal", 2D) = "bump" {}
		_SpecularTex ("Specular", 2D) = "white" {}
		_Color_0("Color 0", COLOR)=(1,1,1,1)
		_Color_1("Color 0", COLOR)=(1,1,1,1)
		_Color_2("Color 0", COLOR)=(1,1,1,1)
		_Color_3("Color 0", COLOR)=(1,1,1,1)
		_Dist("Distance", FLOAT)=10
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "DisableBatching"="True"}
		LOD 100
		Cull Off

		Pass
		{

			Tags {
				"LightMode" = "ForwardBase"
			}

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile _ SHADOWS_SCREEN

			#pragma vertex LF_VertexProgram
			#pragma fragment frag
			
			#define FORWARD_BASE_PASS
			#define CUSTOM_SHADOWS

			//#include "geometries.cginc"
			#include "SHRLighting.cginc"

			fixed4 _Color_0;
			fixed4 _Color_1;
			fixed4 _Color_2;
			fixed4 _Color_3;
			float _Dist;

			fixed4 frag (Interpolator i) : SV_Target
			{
				InitializeFragmentNormal(i);

				i.normal = normalize(i.normal);
				fixed4 diffusemap = _DiffuseTex.Sample(sampler_DiffuseTex, i.uv);
				float d = GEOM_GetDist(i.worldPos.xyz);
				fixed a = saturate(d / _Dist);
				if (diffusemap.a < 0.5 - a * 0.5) GEOM_ClipTransparency(diffusemap.a, i);
				fixed3 col = _Color_3.rgb;
				col = lerp(col, _Color_0, diffusemap.r);
				col = lerp(col, _Color_1, diffusemap.b);
				col = lerp(col, _Color_2, diffusemap.g);
				fixed3 lighting = LF_OnlyShadows(i, col); 
				col.rgb *= lighting;
				return fixed4(col, 1);
			}

			ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "ForwardAdd"
			}

			Blend One One
			ZWrite Off

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile _ VERTEXLIGHT_ON
			#pragma multi_compile_fwdadd_fullshadows

			#pragma vertex LF_VertexProgram
			#pragma fragment frag

			#define CUSTOM_SHADOWS

			#include "SHRLighting.cginc"

			fixed4 _Color_0;
			fixed4 _Color_1;
			fixed4 _Color_2;
			fixed4 _Color_3;

			fixed4 frag (Interpolator i) : SV_Target
			{
				InitializeFragmentNormal(i);

				i.normal = normalize(i.normal);
				fixed4 diffusemap = _DiffuseTex.Sample(sampler_DiffuseTex, i.uv);
				if (diffusemap.a < 0.5) GEOM_ClipTransparency(diffusemap.a, i);
				fixed3 col = _Color_3.rgb;
				col = lerp(col, _Color_0, diffusemap.r);
				col = lerp(col, _Color_1, diffusemap.b);
				col = lerp(col, _Color_2, diffusemap.g);
				fixed3 lighting = LF_OnlyShadows(i, col); 
				col.rgb *= lighting;
				return fixed4(col, 1);
			}
			ENDCG
		}
		
		Pass {
			Tags {
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_shadowcaster

			#pragma vertex vert
			#pragma fragment frag

			#include "SHRShadows.cginc"

			Texture2D _DiffuseTex;
			SamplerState sampler_DiffuseTex;

			SHDW_Full_Interpolator vert (SHDW_Full_VertexData v) {
				SHDW_Full_Interpolator i;
				i.position =
					UnityClipSpaceShadowCasterPos(v.position.xyz, v.normal);
				i.position = UnityApplyLinearShadowBias(i.position);
				i.uv = v.uv;
				i.normal =
					mul(unity_ObjectToWorld, v.position).xyz - _LightPositionRange.xyz;
				return i;
			}

			half4 frag (SHDW_Full_Interpolator i) : SV_TARGET {
				fixed4 tex = _DiffuseTex.Sample(sampler_DiffuseTex, i.uv);
				if (tex.a < 0.5) discard;
				return 0;
			}

			ENDCG
		}
		
	}
}
