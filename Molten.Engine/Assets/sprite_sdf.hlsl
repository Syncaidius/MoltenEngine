#include "sprite.hlsl"

float pxRange = 2;

float median(float r, float g, float b) {
	return max(min(r, g), min(max(r, g), b));
}

float screenPxRange(float2 uv, float2 texSize) {
    float2 unitRange = float2(pxRange,pxRange) / texSize;
    float2 screenTexSize = float2(1.0, 1.0) / fwidth(uv);
    return max(0.5*dot(unitRange, screenTexSize), 1.0);
}

float4 PS_MSDF( PS_IN input) : SV_Target
{            
    SpriteData sd = spriteData[input.id];
    
    float3 msd = mapDiffuse.Sample(diffuseSampler, input.uv).rgb;
    float s = median(msd.r, msd.g, msd.b);
    float screenPxDistance = screenPxRange(input.uvLocal, sd.size)*(s - 0.5);
    float opacity = clamp(screenPxDistance + 0.5, 0.0, 1.0);
    
    return float4(sd.col.rgb, sd.col.a * opacity);
}