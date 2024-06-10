#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D MainRender;

sampler2D MainSampler = sampler_state
{
    Texture = <MainRender>;
};

Texture2D AllyOutlineMask;

sampler2D AllyOutlineSampler = sampler_state
{
    Texture = <AllyOutlineMask>;
};

Texture2D EnemyOutlineMask;

sampler2D EnemyOutlineSampler = sampler_state
{
    Texture = <EnemyOutlineMask>;
};

float4 AllyColor;
float4 EnemyColor;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MergePS(VertexShaderOutput input) : COLOR
{
    float4 mask = tex2D(AllyOutlineSampler, input.TextureCoordinates);
    if (mask.r > 0.05)
        return AllyColor;
    
    mask = tex2D(EnemyOutlineSampler, input.TextureCoordinates);
    if (mask.r > 0.05)
        return EnemyColor;
	
    return tex2D(MainSampler, input.TextureCoordinates);
}

technique MergeOutlines
{
	pass P0
	{
        PixelShader = compile PS_SHADERMODEL MergePS();
    }
};