Shader "Hidden/DepthOfField"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Assets/Shared/Shaders/PostProcessing.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			uniform Texture2D _DepthTex;
			Texture2D _MainTex;
			SamplerState sampler_MainTex;
			uniform float _Focus;
			uniform float _Size;
			uniform float _Aperture;
			uniform float _BlurIntensity;
			uniform float _Debug;

			fixed3 _blur(Texture2D iTexture, Texture2D iDepthTexture, SamplerState iState, float iAmmount, float iSteps, float2 iCoords, float iBaseDepth) {
				fixed3 result = iTexture.Sample(iState, iCoords).rgb;
				fixed4 blurred = (fixed4)0;
				float angle = 3.14 / iSteps;
				for (int i = 0; i < iSteps; i++) {
					float2 coords = iCoords + float2(cos(i * angle), sin(i * angle)) * iAmmount;
					float depth = iDepthTexture.Sample(iState, coords).r  * _ProjectionParams.z;
					float depthblur = min(1, max(0, abs(depth - _Focus) - _Aperture) / _Size);
					if (depthblur > iBaseDepth) {
						blurred += iTexture.Sample(iState, coords) / iSteps;
					}
				}
				result = lerp(result, blurred.rgb, blurred.a);
				return result;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _MainTex.Sample(sampler_MainTex, i.uv);
				float blurammount = _BlurIntensity * 0.001;
				float blurgradient = 6;
				float bluriter = 6;
				float3 ftBlur = (float3)0;
				//float depth = tex2D(_CameraDepthTexture, i.uv).r;
				float depth = _DepthTex.Sample(sampler_MainTex, i.uv).r * _ProjectionParams.z;
				float depthblur = min(1, max(0, abs(depth - _Focus) - _Aperture) / _Size);
				//float depthblur = min(1, max(_Aperture / _Size, abs(depth - _Focus) / _Size));
				for (int k = 0; k < blurgradient; k++) {
					//ftBlur += _blur(_MainTex, _DepthTex, sampler_MainTex, blurammount * k * depthblur, bluriter, i.uv, depthblur).rgb / blurgradient;
					//ftBlur += radialblur(_MainTex, sampler_MainTex, blurammount * k * depthblur, bluriter, i.uv).rgb / blurgradient;
				}
				if (_Debug > 0) {
					ftBlur = (float3)depthblur;
				}
				col.rgb = ftBlur.rgb;
				return col;
			}
			ENDCG
		}
	}
}
