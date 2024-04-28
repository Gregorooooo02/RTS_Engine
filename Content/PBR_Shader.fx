#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif
static const float PI = 3.14159265359;


matrix Projection;
matrix View;
matrix World;
float3x3 normalMatrix;

float3 lightPositions[2];



//--------------------------------------------------------------
//Textures and samplers
Texture2D albedo : register(t0);
Texture2D normal : register(t1);
Texture2D roughness : register(t2);
Texture2D metalness : register(t3);
Texture2D ao : register(t4);

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
//--------------------------------------------------------------


struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoords : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 TexCoords : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 WorldPosition : TEXCOORD2;
};

cbuffer Parameters : register(b0)
{
    float3 viewPos;
}

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
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);
    
    return ggx1 * ggx2;
}

float3 fresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
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

float4 PBR_PS(VertexShaderOutput input) : COLOR
{
    float3 albedo = pow(tex2D(albedoSampler, input.TexCoords).rgb, float3(2.2,2.2,2.2));
    float metallic = tex2D(metalnessSampler, input.TexCoords).r;
    float roughness = tex2D(roughnessSampler, input.TexCoords).r;
    float ao = tex2D(aoSampler, input.TexCoords).r;

    float3 N = getNormalFromMap(input.TexCoords, input.WorldPosition, input.Normal);
    float3 V = normalize(viewPos - input.WorldPosition);
    
    float3 F0 = float3(0.04,0.04,0.04);
    F0 = lerp(F0, albedo, metallic);
    
    float3 Lo = float3(0.0, 0.0, 0.0);
    
    for (int i = 0; i < 1; i++)
    {
        float3 L = normalize(lightPositions[i] - input.WorldPosition);
        float3 H = normalize(V + L);
        float distance = length(lightPositions[i] - input.WorldPosition);
        float attenuation = 1.0 / (distance * distance);
        float3 radiance = float3(1, 1, 1) * attenuation;

        float NDF = DistributionGGX(N, H, roughness);
        float G = GeometrySpitch(N, V, L, roughness);
        float3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);
        
        float numerator = NDF * G * F;
        float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
        float3 specular = numerator / denominator;
        
        
        float3 kS = F;
        float3 kD = float3(1, 1, 1) - kS;
        kD *= 1.0 - metallic;
        
        float NdotL = max(dot(N, L), 0.0);
        
        Lo += (kD * albedo / PI + specular) * radiance * NdotL;
    }
    
    
    float3 ambient = float3(0.03, 0.03, 0.03) * albedo * ao;
    
    float3 color = ambient + Lo;
    
    color = color / (color + float3(1.0, 1.0, 1.0));
    
    float gamma = 1.0 / 2.2;
    color = pow(color, float3(gamma,gamma,gamma));

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