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
float3 xLightDirection;
float xAmbient;
bool xEnableLighting;
bool xShowNormals;
float3 xCamPos;
float3 xCamUp;
float xPointSpriteSize;

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

MTVertexToPixel MultitexturedVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0, float4 inTexWeights: TEXCOORD1)
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
        lightingFactor = dot(PSIn.Normal, PSIn.LightDirection);
    
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