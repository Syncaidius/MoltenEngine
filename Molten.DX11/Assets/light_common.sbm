struct LIGHT
{
	float4x4 Transform;
	float3 Position;
	float RangeRcp;
	float3 Color;
	float Intensity;
	float3 Forward;
	float Tess; // Tessellation factor
	float Length;
	float HalfLength;
};

StructuredBuffer<LIGHT> LightData : register(t8);

//=========VERTEX SHADER=====================================================
float4 VS() : SV_Position
{
	return float4(0.0, 0.0, 0.0, 1.0);
}

//=========HULL SHADER ======================================================
struct HS_CONSTANT_DATA_OUTPUT
{
	float Edges[4] : SV_TessFactor;
	float Inside[2] : SV_InsideTessFactor;
};

HS_CONSTANT_DATA_OUTPUT LightConstantHS()
{
	HS_CONSTANT_DATA_OUTPUT Output;

	float tessFactor = 18.0; //TODO make this configurable as a light "quality" option
	Output.Edges[0] = Output.Edges[1] = Output.Edges[2] = Output.Edges[3] = tessFactor;
	Output.Inside[0] = Output.Inside[1] = tessFactor;

	return Output;
}