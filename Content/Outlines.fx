#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

//static const float4 black = float3(0, 0, 0);

matrix World;
matrix Projection;
matrix View;

float4 parameters;
float StepWidth;

cbuffer BoneTransforms : register(b2) 
{
    matrix BoneTransforms[72];
};

Texture2D workTexture;

sampler2D workSampler = sampler_state
{
    Texture = <workTexture>;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	int4 BoneIndices : BLENDINDICES0;
	float4 BoneWeights : BLENDWEIGHT0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
};

VertexShaderOutput SilVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    output.Position = mul(mul(mul(input.Position, World), View), Projection);
    
    return output;
}

VertexShaderOutput Sil_SkinnedVS(in VertexShaderInput input) 
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    
    float4 skinnedPosition = float4(0, 0, 0, 0);
   
    for (int i = 0; i < 4; i++)
    {
        skinnedPosition += mul(input.Position, BoneTransforms[input.BoneIndices[i]]) * input.BoneWeights[i];
    }
    
    output.Position = mul(mul(mul(skinnedPosition, World), View), Projection);
    
    return output;
}

float4 SilPS(VertexShaderOutput input) : COLOR
{
    return float4(1, 1, 1, 1);
}

technique Silhouettes
{
	pass P0
	{
        VertexShader = compile VS_SHADERMODEL SilVS();
        PixelShader = compile PS_SHADERMODEL SilPS();
    }
};

technique Silhouettes_Skinned
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL Sil_SkinnedVS();
        PixelShader = compile PS_SHADERMODEL SilPS();
    }
};


struct SpriteShaderInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 UVExtraction(SpriteShaderInput input) : COLOR
{
    float4 color = tex2D(workSampler, input.TextureCoordinates) * input.Color;  
    return color > 0 ? float4(input.TextureCoordinates, 0.01, 1) : color;
}

technique UVExtract
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL UVExtraction();
    }
};

float4 InitPS(SpriteShaderInput input) : COLOR
{
    float2 pos = input.TextureCoordinates;
    float4 originalColor = tex2D(workSampler, input.TextureCoordinates);
    
    half3x3 values;
    
    for (int i = 0; i < 3; i++)
    {
        for (int j = 0; j < 3; j++)
        {
            float2 sampleUV = clamp(pos + float2(parameters.z * (i - 1), parameters.w * (j - 1)), float2(0, 0), float2(1 - parameters.z, 1 - parameters.w));
            values[i][j] = tex2D(workSampler, sampleUV).r;
        }
    }
   
    float2 dir = float2(
                    values[0][0] + values[0][1] * 2.0 + values[0][2] - values[2][0] - values[2][1] * 2.0 - values[2][2],
                    values[0][0] + values[1][0] * 2.0 + values[2][0] - values[0][2] - values[1][2] * 2.0 - values[2][2]
                    );
    
    float sobel = sqrt(dir.x * dir.x + dir.y * dir.y);
       
    return sobel > 0.15 ? originalColor : float4(0,0,0,1);
}

technique Init
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL InitPS();
    }
};



float4 JumpFloodPS(SpriteShaderInput input) : COLOR
{
    
    float2 pos = input.TextureCoordinates;
    
    float bestDist = 1500000;
    float2 bestCoord;
    
    for (int u = -1; u <= 1; u++)
    {
        for (int v = -1; v <= 1;v++)
        {
            float2 offsetUV = pos + float2(u, v) * StepWidth * parameters.zw;
        
            offsetUV = clamp(offsetUV, float2(0, 0), float2(1 - parameters.z, 1 - parameters.w));
        
            float2 offsetPos = tex2D(workSampler, offsetUV).rg;
        
            float2 disp = pos - offsetPos;
        
            float dist2 = disp.x * disp.x + disp.y * disp.y;
        
            if (offsetPos.x > 0.01 && dist2 < bestDist)
            {
                bestDist = dist2;
                bestCoord = offsetPos;
            }
        }
    }
    return bestDist > 1000000 ? float4(0, 0, 0, 0) : float4(bestCoord,0.01,1);
}

technique JumpFlood
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL JumpFloodPS();
    }
};
