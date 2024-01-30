#include "sprite.hlsl"

Texture2D mapLighting;
Texture2D mapEmissive;

float4 PS_Compose( PS_IN input ) : SV_Target
{
  float4 col = mapDiffuse.Sample(diffuseSampler, input.uv);
  float4 light = mapLighting.Sample(diffuseSampler, input.uv.xy);
  float4 emissive = mapEmissive.Sample(diffuseSampler, input.uv.xy);
  
  return (col * light) + emissive;
}
