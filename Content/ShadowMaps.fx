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

cbuffer BoneTransforms : register(b0)
{
    matrix BoneTransforms[72];
};

struct CreateShadowMap_VSIn
{
    float4 Position : POSITION0;
    int4 BoneIndices : BLENDINDICES0;
    float4 BoneWeights : BLENDWEIGHT0;
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

CreateShadowMap_VSOut ShadowMap_Instanced_VS(CreateShadowMap_VSIn input,float4x4 world : BLENDWEIGHT)
{
    CreateShadowMap_VSOut Out = (CreateShadowMap_VSOut) 0;
    Out.Position = mul(input.Position, mul(transpose(world), LightViewProj));
    Out.Depth = Out.Position.zw;
    
    return Out;
}

CreateShadowMap_VSOut ShadowMap_Skinned_VS(in CreateShadowMap_VSIn input)
{
    CreateShadowMap_VSOut Out = (CreateShadowMap_VSOut)0;
    
    float4 skinnedPosition = float4(0.0, 0.0, 0.0, 0.0);
    for (int i = 0; i < 4; i++)
    {
        skinnedPosition += mul(input.Position, BoneTransforms[input.BoneIndices[i]]) * input.BoneWeights[i];
    }
    
    Out.Position = mul(skinnedPosition, mul(World, LightViewProj));
    Out.Depth = Out.Position.zw;
    
    return Out;
}

CreateShadowMap_VSOut SFVS(float3 inPos : POSITION)
{
    CreateShadowMap_VSOut Output = (CreateShadowMap_VSOut) 0;

    Output.Position = mul(float4(inPos,1), mul(World, LightViewProj));
    Output.Depth = Output.Position.zw;
    
    return Output;
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

//Technique for creating the shadow map for instanced objects
technique ShadowInstanced
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL ShadowMap_Instanced_VS();
        PixelShader = compile PS_SHADERMODEL CreateShadowMap_PixelShader();
    }
}

// Technique for creating the shadow map for terrain
technique TerrainShadow
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL SFVS();
        PixelShader = compile PS_SHADERMODEL CreateShadowMap_PixelShader();
    }
}

//Technique for creating the shadow map for skinned objects
technique ShadowSkinned
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL ShadowMap_Skinned_VS();
        PixelShader = compile PS_SHADERMODEL CreateShadowMap_PixelShader();
    }
}