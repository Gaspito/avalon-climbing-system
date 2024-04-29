



sampler2D _CameraDepthTexture;

float POSTPROD_GetDepth(float2 uv) {
	return Linear01Depth(tex2D(_CameraDepthTexture, uv).r);
}
