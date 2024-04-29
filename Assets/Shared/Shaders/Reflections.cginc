//this file contains all usefull functions regarding Reflections implementation in shaders
//import it with: #include "Assets/Data_PC/Shared/Shaders/Reflections.cginc"
//requires geometries.cginc

uniform Texture2D RFLX_DepthTexture;
uniform Texture2D RFLX_PreviousRender;
SamplerState samplerRFLX_DepthTexture;

float4 RFLX_getSSR(v2f i, float dist, float bias) {
	float max_iter = 200;
	float3 normal = normalize(i.normal);
	float3 view = normalize(getviewdir(i.worldpos));
	float3 dir = reflect(view, normal);
	float step = dist / max_iter;
	for (int iter = 0; iter < max_iter; iter++) {
		float3 pos = i.worldpos.xyz + dir * step * iter;
		float4 coords = WorldToProj(float4(pos,1));
		float2 screen_coords = coords.xy / coords.w / 2;
		screen_coords.y = -screen_coords.y;
		screen_coords += float2(1, 1) / 2;
		if (screen_coords.x < 1 && screen_coords.x > 0 && screen_coords.y < 1 && screen_coords.y > 0) {
			float depth = (1 - RFLX_DepthTexture.Sample(samplerRFLX_DepthTexture, screen_coords).r) * (_ProjectionParams.z - _ProjectionParams.y);
			float dist_to_cam = distance(pos, _WorldSpaceCameraPos);
			if (dist_to_cam + step/2 > depth) {
				float3 render = RFLX_PreviousRender.Sample(samplerRFLX_DepthTexture, screen_coords).rgb;
				return float4(render * (1 - distance(pos, i.worldpos) / dist), 1);
			}
			else if (depth < dist_to_cam && step > 0) {
				step = -step / 2;
			}
			else if (depth > dist_to_cam && step < 0) {
				step = -step / 2;
			}
		}
	}
	return float4(0, 0, 0, 1);
}