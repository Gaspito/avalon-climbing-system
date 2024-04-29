Shader "SHR/Idol Metal"
{
	Properties
	{
		_DiffuseTex ("Diffuse", 2D) = "white" {}
		_NormalTex ("Normal", 2D) = "bump" {}
		_SpecularTex ("Specular", 2D) = "white" {}
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		_BumpScale ("Bump Scale", Float) = 1
		[Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
		_Tex1 ("Energy", 2D) = "black" {}
		_Energy("Energy Level", RANGE(0,1))=0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "DisableBatching"="True"}
		LOD 100

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
			
			#include "SHRLighting.cginc"

			Texture2D _Tex1;
			SamplerState sampler_Tex1;
			float _Energy;

			fixed4 frag (Interpolator i) : SV_Target
			{
				InitializeFragmentNormal(i);

				i.normal = normalize(i.normal);
				fixed4 diffusemap = _DiffuseTex.Sample(sampler_DiffuseTex, i.uv);
				fixed4 specularmap = _SpecularTex.Sample(sampler_DiffuseTex, i.uv);
				fixed3 lighting = LF_Default(i, diffusemap.rgb, specularmap.rgb); 
				diffusemap.rgb *= lighting;
				float2 coords = i.uv * 3 + float2(_Time.y * 5, _Time.y * 2);
				fixed4 map1 = _Tex1.Sample(sampler_Tex1, coords);
				diffusemap.rgb += max(0, map1.rgb - (1-_Energy));
				return diffusemap;
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

			#define FORWARD_BASE_PASS

			#include "SHRLighting.cginc"

			Texture2D _Tex1;
			SamplerState sampler_Tex1;

			fixed4 frag (Interpolator i) : SV_Target
			{
				InitializeFragmentNormal(i);

				i.normal = normalize(i.normal);
				fixed4 diffusemap = _DiffuseTex.Sample(sampler_DiffuseTex, i.uv);
				fixed4 specularmap = _SpecularTex.Sample(sampler_DiffuseTex, i.uv);
				fixed3 lighting = LF_Default(i, diffusemap.rgb, specularmap.rgb); 
				diffusemap.rgb *= lighting;
				return diffusemap;
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

			#pragma vertex MyShadowVertexProgram
			#pragma fragment MyShadowFragmentProgram

			#include "SHRShadows.cginc"

			ENDCG
		}
	}
}
