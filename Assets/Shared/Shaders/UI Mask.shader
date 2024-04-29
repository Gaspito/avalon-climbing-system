Shader "UI/Mask"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Texture_1 ("Texture 1", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

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

			SamplerState sampler_MainTex;
			Texture2D _MainTex;
			Texture2D _Texture_1;
			
			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _MainTex.Sample(sampler_MainTex, i.uv);
				fixed4 mask = _Texture_1.Sample(sampler_MainTex, i.uv);
				if (mask.a < 0.5){
					discard;
				}
				return col;
			}
			ENDCG
		}
	}
}
