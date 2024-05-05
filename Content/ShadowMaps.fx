#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix LightViewProj;
matrix World;

struct CreateShadowMap_VSIn
{
    float4 Position : POSITION0;
};

struct CreateShadowMap_VSOut
{
    float4 Position : POSITION;
    float2 Depth : TEXCOORD0;

};

CreateShadowMap_VSOut CreateShadowMap_VertexShader(in CreateShadowMap_VSIn input)
{
    CreateShadowMap_VSOut Out = (CreateShadowMap_VSOut)0;
    Out.Position = mul(input.Position, mul(World, LightViewProj));
    Out.Depth = Out.Position.zw;
    
    return Out;
}

float4 CreateShadowMap_PixelShader(CreateShadowMap_VSOut input) : COLOR
{
    return float4(input.Depth.x / input.Depth.y, 1, 1, 1);
}

// Technique for creating the shadow map
technique CreateShadowMap
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL CreateShadowMap_VertexShader();
        PixelShader = compile PS_SHADERMODEL CreateShadowMap_PixelShader();
    }
}