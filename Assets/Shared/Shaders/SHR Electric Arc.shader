Shader "SHR/Electric Arc"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Magnitude("Magnitude", FLOAT) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 100

		Blend One One
		Cull Off

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
				float3 color : COLOR;
				float3 normal : NORMAL;
				float3 tangent : TANGENT;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Magnitude;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				float3 n = normalize(cross(UnityObjectToWorldNormal(v.normal), UnityObjectToWorldNormal(v.tangent)));
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.worldPos.xyz += n * sin(_Time.y * 34 + o.worldPos.x * v.color.r + o.worldPos.z * (1-v.color.r)) * _Magnitude * v.color.g;
				float4 localPos = mul(unity_WorldToObject, float4(o.worldPos.xyz, 1));
				o.vertex = UnityObjectToClipPos(localPos);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv + float2(_Time.y * 3.4, 0));
				return col;
			}
			ENDCG
		}
	}
}
