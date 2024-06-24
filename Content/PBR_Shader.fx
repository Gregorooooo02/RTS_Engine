#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

static const float PI = 3.14159265359;
static const float3 dirLightDirection = float3(0.5,-1,0.5);
static const float3 dirLightColor = float3(1, 1, 0.6);
static const float dirLightIntesity = 6.5;
static const float ShadowMapSize = 4096;
static const float fogScale = 1.0 / 4096.0;
static const float DepthBias = 0.005;


cbuffer ModelParameters : register(b0)
{
    matrix World;
    float3x3 normalMatrix;
};

cbuffer Globals : register(b1)
{
    matrix Projection;
    matrix View;
    
    matrix dirLightSpace;
    
    float3 viewPos;
};

cbuffer BoneTransforms : register(b2)
{
    matrix BoneTransforms[72];
};

float gamma;
bool applyFog;
//float DepthBias;
//float ShadowMapSize;
//float dirLightIntesity;
//float fogScale;

//--------------------------------------------------------------
//Textures and samplers
Texture2D albedo : register(t0);
Texture2D normal : register(t1);
Texture2D roughness : register(t2);
Texture2D metalness : register(t3);
Texture2D ao : register(t4);
Texture2D ShadowMap : register(t5);
Texture2D visibility : register(t6);
Texture2D discovery : register(t7);

sampler2D albedoSampler = sampler_state
{
    Texture = <albedo>;
};

sampler2D normalSampler = sampler_state
{
    Texture = <normal>;
};

sampler2D roughnessSampler = sampler_state
{
    Texture = <roughness>;
};

sampler2D metalnessSampler = sampler_state
{
    Texture = <metalness>;
};

sampler2D aoSampler = sampler_state
{
    Texture = <ao>;
};

sampler2D shadowMapSampler = sampler_state
{
    Texture = <ShadowMap>;
    MinFilter = point;
    MagFilter = point;
    MipFilter = point;
    AddressU = clamp;
    AddressV = clamp;
};

sampler2D FogVisibility = sampler_state
{
    Texture = <visibility>;
    MinFilter = point;
    MagFilter = point;
    MipFilter = point;
    AddressU = clamp;
    AddressV = clamp;
};

sampler2D FogDiscovery = sampler_state
{
    Texture = <discovery>;
    MinFilter = point;
    MagFilter = point;
    MipFilter = point;
    AddressU = clamp;
    AddressV = clamp;
};
//--------------------------------------------------------------

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoords : TEXCOORD0;
    int4 BoneIndices : BLENDINDICES0;
    float4 BoneWeights : BLENDWEIGHT0;
};

struct InstanceData
{
    float4x4 World : BLENDWEIGHT;
    float4x4 NormalMatrix : BLENDINDICES;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 TexCoords : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 WorldPosition : TEXCOORD2;
};

float3 getNormalFromMap(float2 TexCoords, float3 worldPos, float3 Normal)
{
    float3 tangentNormal = tex2D(normalSampler, TexCoords).xyz * 2.0 - 1.0;

    float3 Q1 = ddx(worldPos);
    float3 Q2 = ddy(worldPos);
    float2 st1 = ddx(TexCoords);
    float2 st2 = ddy(TexCoords);
    
    float3 N = normalize(Normal);
    float3 T = normalize(Q1 * st2.x - Q2 * st1.x);
    float3 B = -normalize(cross(N, T));
    float3x3 TBN = float3x3(T, B, N);
    
    return normalize(mul(tangentNormal, TBN));
}

float DistributionGGX(float3 N, float3 H, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0);
    float NdotH2 = NdotH * NdotH;
    
    float nom = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
    
    return nom / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;
    
    float nom = NdotV;
    float denom = NdotV * (1.0 - k) + k;
    
    return nom / denom;
}

float GeometrySpitch(float3 N, float3 V, float3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = clamp(dot(N, L), 0.0,1.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);
    
    return ggx1 * ggx2;
}

float3 fresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

float CalcdirectionalShadowsPCF(float light_space_depth, float NdotL, float2 shadowCoords)
{
    float shadowTerm = 0;
    
    float variableBias = clamp(0.0005 * tan(acos(NdotL)), 0.00001, DepthBias);
    
    float size = 1.0 / ShadowMapSize;
   
    float samples[4];
    
    samples[0] = (light_space_depth - variableBias < tex2D(shadowMapSampler, shadowCoords).r);
    samples[1] = (light_space_depth - variableBias < tex2D(shadowMapSampler, shadowCoords + float2(size, 0)).r);
    samples[2] = (light_space_depth - variableBias < tex2D(shadowMapSampler, shadowCoords + float2(0, size)).r);
    samples[3] = (light_space_depth - variableBias < tex2D(shadowMapSampler, shadowCoords + float2(size, size)).r);
    
    shadowTerm = (samples[0] + samples[1] + samples[2] + samples[3]) / 4.0;
    
    return shadowTerm;

}

float CalcDirectionalShadowsSoftPCF(float lightDepth, float NdotL, float2 shadowCoords, int iSqrtSamples)
{
    float shadowTerm = 0;
    
    float variableBias = clamp(0.0005 * tan(acos(NdotL)), 0.00001, DepthBias);
    
    float radius = iSqrtSamples - 1;
    
    for (float y = -radius; y <= radius; y++)
    {
        for (float x = -radius; x <= radius; x++)
        {
            float2 offset = float2(x, y);
            offset /= ShadowMapSize;
            float2 SamplePoint = shadowCoords + offset;
            float Depth = tex2D(shadowMapSampler, SamplePoint).r;
            float sample = (lightDepth <= Depth + variableBias);

            float xWeight = 1;
            float yWeight = 1;
            
            if (x == -radius)
                xWeight = 1 - frac(shadowCoords.x * ShadowMapSize);
            else if (x == radius)
                xWeight = frac(shadowCoords.x * ShadowMapSize);
            
            if (y == -radius)
                xWeight = 1 - frac(shadowCoords.y * ShadowMapSize);
            else if (y == radius)
                xWeight = frac(shadowCoords.y * ShadowMapSize);
            
            shadowTerm += sample * xWeight * yWeight;
        }

    }
    
    shadowTerm /= (radius * radius * 4);
    return shadowTerm;
}

float3 CalculateDirectionalLight(float3 worldPosition, float3 N, float3 albedo, float metallic, float roughness)
{
    float3 V = normalize(viewPos - worldPosition);
    
    float3 F0 = float3(0.04, 0.04, 0.04);
    F0 = lerp(F0, albedo, metallic);
    
    float3 L = normalize(-dirLightDirection);
    float3 H = normalize(V + L);
    float3 radiance = dirLightColor * dirLightIntesity;
    
    float NDF = DistributionGGX(N, H, roughness);
    float G = GeometrySpitch(N, V, L, roughness);
    float3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);
    
    float3 kS = F;
    float3 kD = float3(1, 1, 1) - kS;
    kD *= 1.0 - metallic;
    
    float3 numerator = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0);
    float3 specular = numerator / max(denominator,0.001);
    
    float NdotL = max(dot(N, L), 0.0);
    
    float shadowContribution = 1.0;
    
    float4 lightPosition = mul(float4(worldPosition, 1), dirLightSpace);
    float2 shadowTexCoord = mad(0.5, lightPosition.xy / lightPosition.w, float2(0.5, 0.5));
    shadowTexCoord.y = 1.0 - shadowTexCoord.y;
    float ourDepth = (lightPosition.z / lightPosition.w);
    if (shadowTexCoord.x <= 1 && shadowTexCoord.y <= 1 && shadowTexCoord.x >= 0 && shadowTexCoord.y >= 0)
    {
        //shadowContribution = CalcdirectionalShadowsPCF(ourDepth, NdotL, shadowTexCoord);
        shadowContribution = CalcDirectionalShadowsSoftPCF(ourDepth, NdotL, shadowTexCoord,3);
    }
    return (kD * albedo / PI + specular) * radiance * NdotL * shadowContribution;   
}

VertexShaderOutput PBR_VS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    output.TexCoords = input.TexCoords;
    
    output.WorldPosition = mul(input.Position, World).xyz;
    output.Normal = mul(input.Normal, normalMatrix);
    
    output.Position = mul(mul(float4(output.WorldPosition, 1), View), Projection);
    
    return output;
}

VertexShaderOutput PBR_Skinned_VS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    output.TexCoords = input.TexCoords;
    
    float4 skinnedPosition = float4(0.0, 0.0, 0.0, 0.0);
    float3 skinnedNormal = float3(0.0, 0.0, 0.0);
    
    for (int i = 0; i < 4; i++)
    {
        skinnedPosition += mul(input.Position, BoneTransforms[input.BoneIndices[i]]) * input.BoneWeights[i];
        skinnedNormal += mul(input.Normal, (float3x3)BoneTransforms[input.BoneIndices[i]]) * input.BoneWeights[i];
    }
    
    output.WorldPosition = mul(skinnedPosition, World).xyz;
    output.Normal = mul(normalize(skinnedNormal), normalMatrix);
    
    output.Position = mul(mul(float4(output.WorldPosition, 1), View), Projection);
    
    return output;
}

VertexShaderOutput PBR_Instanced_VS(VertexShaderInput input, InstanceData data)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    
    output.TexCoords = input.TexCoords;
    
    output.WorldPosition = mul(input.Position, transpose(data.World)).xyz;
    //output.WorldPosition = input.Position.xyz;
    output.Normal = mul(input.Normal, (float3x3)transpose(data.NormalMatrix));
    
    output.Position = mul(mul(float4(output.WorldPosition, 1), View), Projection);
    
    return output;
}

float4 PBR_PS(VertexShaderOutput input) : COLOR
{
    float fogValue = 1.0f;
    if (applyFog == true)
    {
        float2 fogCoords = input.WorldPosition.xz * fogScale;
        fogValue = tex2D(FogDiscovery, fogCoords).r + tex2D(FogVisibility, fogCoords).r;
        if (fogValue < 0.0001f)
        {
            return float4(0, 0, 0, 1);
        }
    }   
    float3 albedo = pow(tex2D(albedoSampler, input.TexCoords).rgb, gamma);
    float metallic = tex2D(metalnessSampler, input.TexCoords).r;
    float roughness = tex2D(roughnessSampler, input.TexCoords).r;
    float ao = tex2D(aoSampler, input.TexCoords).r;

    float3 N = getNormalFromMap(input.TexCoords, input.WorldPosition, input.Normal);
    float3 V = normalize(viewPos - input.WorldPosition);
    
    float3 F0 = float3(0.04,0.04,0.04);
    F0 = lerp(F0, albedo, metallic);
    
    float3 Lo = CalculateDirectionalLight(input.WorldPosition, N, albedo, metallic, roughness);
    
    float3 ambient = 0.03 * albedo * ao;
    
    float3 color = (ambient + Lo) * fogValue;
    
    color = color / (color + 1);
    
    color = pow(color, 1.0 / gamma);

    return float4(color, 1.0);
}

technique PBR
{
	pass P0
	{
        VertexShader = compile VS_SHADERMODEL PBR_VS();
        PixelShader = compile PS_SHADERMODEL PBR_PS();
    }
};


technique Instancing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL PBR_Instanced_VS();
        PixelShader = compile PS_SHADERMODEL PBR_PS();
    }
}

technique PBR_Skinned
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL PBR_Skinned_VS();
        PixelShader = compile PS_SHADERMODEL PBR_PS();
    }
}
