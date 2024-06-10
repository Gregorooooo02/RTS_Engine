#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

static const float PI = 3.14159265359;
static const float3 dirLightDirection = float3(0.5, -1, 0.5);
static const float3 dirLightColor = float3(1, 1, 0.6);
static const float dirLightIntesity = 6.5;
static const float ShadowMapSize = 4096;
static const float fogScale = 1.0 / 4096.0;
static const float DepthBias = 0.005;

struct VertexToPixel
{
    float4 Position   	: POSITION;    
    float4 Color		: COLOR0;
    float LightingFactor: TEXCOORD0;
    float2 TextureCoords: TEXCOORD1;
};

struct PixelToFrame
{
    float4 Color : COLOR0;
};

//------- Constants --------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;

matrix dirLightSpace;

float3 xLightDirection;
float xAmbient;
bool xEnableLighting;

bool xShowNormals;
float3 xCamPos;
float3 xCamUp;
float xPointSpriteSize;
float gamma;

//------- Additional effects' texture samlers -------- 

Texture2D ShadowMap;
sampler2D shadowMapSampler = sampler_state{Texture = <ShadowMap>;MinFilter = point;MagFilter = point;MipFilter = point;AddressU = clamp;AddressV = clamp;};

Texture2D visibility;
sampler2D FogVisibility = sampler_state{Texture = <visibility>;MinFilter = linear;MagFilter = linear;MipFilter = point;AddressU = clamp;AddressV = clamp;};

Texture2D discovery;
sampler2D FogDiscovery = sampler_state{Texture = <discovery>;MinFilter = linear;MagFilter = linear;MipFilter = point;AddressU = clamp;AddressV = clamp;};

//------- Texture Samplers --------

Texture xTexture;
sampler TextureSampler = sampler_state { texture = <xTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture xTexture0;
sampler TextureSampler0 = sampler_state { texture = <xTexture0>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

Texture xTexture1;
sampler TextureSampler1 = sampler_state { texture = <xTexture1>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

Texture xTexture2;
sampler TextureSampler2 = sampler_state { texture = <xTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture xTexture3;
sampler TextureSampler3 = sampler_state { texture = <xTexture3>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

//------- Technique: Pretransformed --------

VertexToPixel PretransformedVS( float4 inPos : POSITION, float4 inColor: COLOR)
{	
	VertexToPixel Output = (VertexToPixel)0;
	
	Output.Position = inPos;
	Output.Color = inColor;
    
	return Output;    
}

PixelToFrame PretransformedPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color = PSIn.Color;

	return Output;
}

technique Pretransformed
{
	pass Pass0
	{   
#if SM4
		VertexShader = compile vs_4_0_level_9_3 PretransformedVS();
		PixelShader  = compile ps_4_0_level_9_3 PretransformedPS();
#else
		VertexShader = compile vs_1_1 PretransformedVS();
		PixelShader  = compile ps_2_0 PretransformedPS();
#endif				
	}
}

//------- Technique: Colored --------

VertexToPixel ColoredVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float4 inColor: COLOR)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Color = inColor;
	
	float3 Normal = normalize(mul(normalize(inNormal), (float3x3)xWorld));	
	Output.LightingFactor = 1;
	if (xEnableLighting)
		Output.LightingFactor = dot(Normal, -xLightDirection);
    
	return Output;    
}

PixelToFrame ColoredPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
    
	Output.Color = PSIn.Color;
	Output.Color.rgb *= saturate(PSIn.LightingFactor) + xAmbient;

	return Output;
}

technique Colored
{
	pass Pass0
	{   
#if SM4
		VertexShader = compile vs_4_0_level_9_3 ColoredVS();
		PixelShader  = compile ps_4_0_level_9_3 ColoredPS();
#else
		VertexShader = compile vs_1_1 ColoredVS();
		PixelShader  = compile ps_2_0 ColoredPS();
#endif				
	}
}

//------- Technique: ColoredNoShading --------

VertexToPixel ColoredNoShadingVS( float4 inPos : POSITION, float4 inColor: COLOR)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Color = inColor;
    
	return Output;    
}

PixelToFrame ColoredNoShadingPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
    
	Output.Color = PSIn.Color;

	return Output;
}

technique ColoredNoShading
{
	pass Pass0
	{   
#if SM4
		VertexShader = compile vs_4_0_level_9_3 ColoredNoShadingVS();
		PixelShader  = compile ps_4_0_level_9_3 ColoredNoShadingPS();
#else
		VertexShader = compile vs_1_1 ColoredNoShadingVS();
		PixelShader  = compile ps_2_0 ColoredNoShadingPS();
#endif				
	}
}


//------- Technique: Textured --------

VertexToPixel TexturedVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);	
	Output.TextureCoords = inTexCoords;
	
	float3 Normal = normalize(mul(normalize(inNormal), (float3x3)xWorld));	
	Output.LightingFactor = 1;
	if (xEnableLighting)
		Output.LightingFactor = dot(Normal, -xLightDirection);
    
	return Output;    
}

PixelToFrame TexturedPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color = tex2D(TextureSampler, PSIn.TextureCoords);
	Output.Color.rgb *= saturate(PSIn.LightingFactor) + xAmbient;

	return Output;
}

technique Textured
{
	pass Pass0
	{   
#if SM4
		VertexShader = compile vs_4_0_level_9_3 TexturedVS();
		PixelShader  = compile ps_4_0_level_9_3 TexturedPS();
#else
		VertexShader = compile vs_1_1 TexturedVS();
		PixelShader  = compile ps_2_0 TexturedPS();
#endif				
	}
}

//------- Technique: TexturedNoShading --------

VertexToPixel TexturedNoShadingVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);	
	Output.TextureCoords = inTexCoords;
    
	return Output;    
}

PixelToFrame TexturedNoShadingPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color = tex2D(TextureSampler, PSIn.TextureCoords);

	return Output;
}

technique TexturedNoShading
{
	pass Pass0
	{   
#if SM4
		VertexShader = compile vs_4_0_level_9_3 TexturedNoShadingVS();
		PixelShader  = compile ps_4_0_level_9_3 TexturedNoShadingPS();
#else
		VertexShader = compile vs_1_1 TexturedNoShadingVS();
		PixelShader  = compile ps_2_0 TexturedNoShadingPS();
#endif		
	}
}

//------- Technique: PointSprites --------

VertexToPixel PointSpriteVS(float3 inPos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
    VertexToPixel Output = (VertexToPixel)0;

    float3 center = mul(inPos, (float3x3)xWorld);
    float3 eyeVector = center - xCamPos;

    float3 sideVector = cross(eyeVector,xCamUp);
    sideVector = normalize(sideVector);
    float3 upVector = cross(sideVector,eyeVector);
    upVector = normalize(upVector);

    float3 finalPosition = center;
    finalPosition += (inTexCoord.x-0.5f)*sideVector*0.5f*xPointSpriteSize;
    finalPosition += (0.5f-inTexCoord.y)*upVector*0.5f*xPointSpriteSize;

    float4 finalPosition4 = float4(finalPosition, 1);

    float4x4 preViewProjection = mul (xView, xProjection);
    Output.Position = mul(finalPosition4, preViewProjection);

    Output.TextureCoords = inTexCoord;

    return Output;
}

PixelToFrame PointSpritePS(VertexToPixel PSIn)
{
    PixelToFrame Output = (PixelToFrame)0;
    Output.Color = tex2D(TextureSampler, PSIn.TextureCoords);
    return Output;
}

technique PointSprites
{
	pass Pass0
	{   
#if SM4
		VertexShader = compile vs_4_0_level_9_3 PointSpriteVS();
		PixelShader  = compile ps_4_0_level_9_3 PointSpritePS();
#else
		VertexShader = compile vs_1_1 PointSpriteVS();
		PixelShader  = compile ps_2_0 PointSpritePS();
#endif
	}
}

//------- Technique: Multitexturing --------
struct MTVertexToPixel
{
    float4 Position   	 : POSITION;
    float4 Color		 : COLOR0;
    float3 Normal		 : TEXCOORD0;
    float2 TextureCoords : TEXCOORD1;
    float4 LightDirection: TEXCOORD2;
    float4 TextureWeights: TEXCOORD3;
};

struct MTPixelToFrame
{
    float4 Color : COLOR0;
};

MTVertexToPixel MultitexturedVS(float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0, float4 inTexWeights: TEXCOORD1)
{	
    MTVertexToPixel Output = (MTVertexToPixel)0;
    float4x4 preViewProjection = mul (xView, xProjection);
    float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
    Output.Position = mul(inPos, preWorldViewProjection);
    Output.Normal = mul(normalize(inNormal), (float3x3)xWorld);
    Output.TextureCoords = inTexCoords;
    Output.LightDirection.xyz = -xLightDirection;
    Output.TextureWeights = inTexWeights;
    
    return Output;
}

MTPixelToFrame MultitexturedPS(MTVertexToPixel PSIn) 
{
    MTPixelToFrame Output = (MTPixelToFrame)0;		
    
    float lightingFactor = 1;
    if (xEnableLighting)
        lightingFactor = dot(PSIn.Normal, PSIn.LightDirection.xyz);
    
    Output.Color = tex2D(TextureSampler0, PSIn.TextureCoords) * PSIn.TextureWeights.x;
    Output.Color += tex2D(TextureSampler1, PSIn.TextureCoords) * PSIn.TextureWeights.y;
    Output.Color += tex2D(TextureSampler2, PSIn.TextureCoords) * PSIn.TextureWeights.z;
    Output.Color += tex2D(TextureSampler3, PSIn.TextureCoords) * PSIn.TextureWeights.w;
    
    Output.Color.rgb *= saturate(lightingFactor) + xAmbient;
    
    return Output;
}

technique Multitextured
{
    pass Pass0
    {
#if SM4
        VertexShader = compile vs_4_0_level_9_3 MultitexturedVS();
        PixelShader  = compile ps_4_0_level_9_3 MultitexturedPS();
#else
        VertexShader = compile vs_1_1 MultitexturedVS();
        PixelShader  = compile ps_2_0 MultitexturedPS();
#endif
    }
}


//------- Technique: WaterWaves --------
float xWaveMapScale;
float2 xWaveMapOffset;
float4 xWaterColor;

Texture xWaveNormalMap;
sampler TextureSamplerWave = sampler_state
{
    texture = <xWaveNormalMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

struct WaterVertexToPixel
{
    float4 Position    : POSITION;
    float2 TextureCoords : TEXCOORD0;
    float2 WaveMapCoords : TEXCOORD1;
};

struct WaterPixelToFrame
{
    float4 Color : COLOR0;
};

WaterVertexToPixel WaterWavesVS( float4 inPos : POSITION, float2 inTexCoords: TEXCOORD0)
{
    WaterVertexToPixel Output = (WaterVertexToPixel)0;
    float4x4 preViewProjection = mul (xView, xProjection);
    float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);

    Output.Position = mul(inPos, preWorldViewProjection);
    Output.TextureCoords = inTexCoords;
    Output.WaveMapCoords = inTexCoords * xWaveMapScale + xWaveMapOffset;

    return Output;
}

WaterPixelToFrame WaterWavesPS(WaterVertexToPixel PSIn)
{
    WaterPixelToFrame Output = (WaterPixelToFrame)0;
    
    //float3 normal = tex2D(TextureSamplerWave, PSIn.WaveMapCoords).xyz;

    //normal = 2.0f * normal - 1.0f;

    //float3 blendedNormal = normalize(normal);
    
    //float3 blendedNormal = normalize(float3(0, 0, 1));

    //float lightingFactor = dot(blendedNormal, normalize(dirLightDirection));
    
    //float lightingFactor = 0.8;

    //Output.Color = xWaterColor * saturate(lightingFactor + xAmbient);
    
    Output.Color = xWaterColor * saturate(0.8 + xAmbient);
    
    return Output;
}

technique WaterWaves
{
    pass Pass0
    {
#if SM4
        VertexShader = compile vs_4_0_level_9_3 WaterWavesVS();
        PixelShader  = compile ps_4_0_level_9_3 WaterWavesPS();
#else
        VertexShader = compile vs_1_1 WaterWavesVS();
        PixelShader  = compile ps_2_0 WaterWavesPS();
#endif
    }
}

//------- Technique: ShadowFog --------
struct SFVertexToPixel
{
    float4 Position : POSITION;
    float3 Normal : TEXCOORD0;
    float2 TextureCoords : TEXCOORD1;
    float4 TextureWeights : TEXCOORD2;
    float3 WorldPosition : TEXCOORD3;
};

float CalcDirectionalShadowsSoftPCF(float lightDepth, float NdotL, float2 shadowCoords, int iSqrtSamples)
{
    float shadowTerm = 0;
    
    //float variableBias = clamp(0.0005 * tan(acos(NdotL)), 0.001, DepthBias);
    float variableBias = max(0.005 * (1.0 - NdotL), 0.001);
    
    float radius = iSqrtSamples - 1;
    
    for (float y = -radius; y <= radius; y++)
    {
        for (float x = -radius; x <= radius; x++)
        {
            float2 offset = float2(x, y);
            offset /= ShadowMapSize;
            float2 SamplePoint = shadowCoords + offset;
            float Depth = tex2D(shadowMapSampler, SamplePoint).r;
            float sample1 = (lightDepth <= Depth + variableBias);

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
            
            shadowTerm += sample1 * xWeight * yWeight;
        }

    }
    
    shadowTerm /= (radius * radius * 4);
    return shadowTerm;
}

float3 CalculateDirectionalLight(float3 worldPosition, float3 N)
{
    float3 L = normalize(dirLightDirection);
    float NdotL = max(dot(N, L), 0.0);
    float3 radiance = dirLightColor * dirLightIntesity;
    float shadowContribution = 1.0;
    
    float4 lightPosition = mul(float4(worldPosition, 1), dirLightSpace);
    float2 shadowTexCoord = mad(0.5, lightPosition.xy / lightPosition.w, float2(0.5, 0.5));
    shadowTexCoord.y = 1.0 - shadowTexCoord.y;
    float ourDepth = (lightPosition.z / lightPosition.w);
    if (shadowTexCoord.x <= 1 && shadowTexCoord.y <= 1 && shadowTexCoord.x >= 0 && shadowTexCoord.y >= 0)
    {
        shadowContribution = CalcDirectionalShadowsSoftPCF(ourDepth, NdotL, shadowTexCoord, 3);
    }
    //return radiance * NdotL * shadowContribution;
    //return shadowContribution;
    //return radiance;
    return NdotL * shadowContribution * radiance;
}


SFVertexToPixel SFVS(float4 inPos : POSITION, float3 inNormal : NORMAL, float2 inTexCoords : TEXCOORD0, float4 inTexWeights : TEXCOORD1)
{
    SFVertexToPixel Output = (SFVertexToPixel) 0;
    float4x4 preViewProjection = mul(xView, xProjection);
    float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
    
    Output.Position = mul(inPos, preWorldViewProjection);
    Output.Normal = mul(normalize(inNormal), (float3x3) xWorld);
    Output.TextureCoords = inTexCoords;
    Output.TextureWeights = inTexWeights;
    Output.WorldPosition = mul(inPos, xWorld).xyz;
    
    return Output;
}


float4 SFPS(SFVertexToPixel input) : COLOR
{
    float2 fogCoords = input.WorldPosition.xz * fogScale;
    float fogValue = tex2D(FogDiscovery, fogCoords).r + tex2D(FogVisibility, fogCoords).r;
    if (fogValue < 0.0001f)
    {
        return float4(0, 0, 0, 1);
    }
       
    float3 albedo = pow(tex2D(TextureSampler0, input.TextureCoords) * input.TextureWeights.x,gamma).xyz;
    albedo += pow(tex2D(TextureSampler1, input.TextureCoords) * input.TextureWeights.y, gamma).xyz;
    albedo += pow(tex2D(TextureSampler2, input.TextureCoords) * input.TextureWeights.z, gamma).xyz;
    albedo += pow(tex2D(TextureSampler3, input.TextureCoords) * input.TextureWeights.w, gamma).xyz;
    
    float3 Lo = CalculateDirectionalLight(input.WorldPosition, input.Normal) * albedo / PI;
    
    float3 ambient = 0.03 * albedo;
    
    float3 color = (ambient + Lo) * fogValue;
    
    color = color / (color + 1);
    
    color = pow(color, 1.0 / gamma);

    return float4(color, 1.0);
}

technique ShadowFog
{
    pass Pass0
    {
#if SM4
        VertexShader = compile vs_4_0_level_9_3 MultitexturedVS();
        PixelShader  = compile ps_4_0_level_9_3 MultitexturedPS();
#else
        VertexShader = compile vs_3_0 SFVS();
        PixelShader = compile ps_3_0 SFPS();
#endif
    }
}