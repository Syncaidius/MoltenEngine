struct VS_IN
{
    float4 pos : POSITION;
    float4 col : COLOR;
    row_major matrix wvp : WORLD;
};

struct PS_IN
{
    float4 pos : SV_POSITION;
    float4 col : COLOR;
};

cbuffer Common : register(b0)
{
    float4x4 viewProjection : packoffset(c0);
}

PS_IN VS(VS_IN input)
{
    PS_IN output = (PS_IN) 0;
    output.pos = mul(input.pos, mul(input.wvp, viewProjection));
    output.col = input.col;
    return output;
}

float4 PS(PS_IN input) : SV_Target
{
    return input.col;
}