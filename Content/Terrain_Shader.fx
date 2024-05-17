//------- Constants --------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
float3 xLightDirection;
float xAmbient;
bool xEnableLighting;

float3 xCameraPosition;
float4x4 xWorldInverseTranspose;

//------- Texture Samplers --------
Texture xTexture;
sampler TextureSampler = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture xTexture0;
sampler TextureSampler0 = sampler_state { texture = <xTexture0> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture xTexture1;
sampler TextureSampler1 = sampler_state { texture = <xTexture1> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture xTexture2;
sampler TextureSampler2 = sampler_state { texture = <xTexture2> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture xTexture3;
sampler TextureSampler3 = sampler_state { texture = <xTexture3> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
//------- Technique: Textured --------
struct TVertexToPixel
{
    float4 Position     : POSITION;
    float4 Color        : COLOR0;
    float LightingFactor: TEXCOORD0;
    float2 TextureCoords: TEXCOORD1;
};

struct TPixelToFrame
{
    float4 Color : COLOR0;
};

TVertexToPixel TexturedVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0)
{
    TVertexToPixel Output = (TVertexToPixel)0;
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

TPixelToFrame TexturedPS(TVertexToPixel PSIn)
{
    TPixelToFrame Output = (TPixelToFrame)0;

    Output.Color = tex2D(TextureSampler, PSIn.TextureCoords);
    Output.Color.rgb *= saturate(PSIn.LightingFactor) + xAmbient;

    return Output;
}

technique Textured_2_0
{
    pass Pass0
#if SM4
    {
        VertexShader = compile vs_4_0_level_9_3 TexturedVS();
        PixelShader = compile ps_4_0_level_9_3 TexturedPS();
    }
#else
    {
        VertexShader = compile vs_1_1 TexturedVS();
        PixelShader = compile ps_2_0 TexturedPS();
    }
}

technique Textured
{
    pass Pass0
#if SM4
    {
        VertexShader = compile vs_4_0_level_9_3 TexturedVS();
        PixelShader = compile ps_4_0_level_9_3 TexturedPS();
    }
#else
    {
        VertexShader = compile vs_1_1 TexturedVS();
        PixelShader = compile ps_2_0 TexturedPS();
    }
}

//------- Technique: Multitextured --------
struct MTVertexToPixel
{
    float4 Position         : POSITION;    
    float4 Color            : COLOR0;
    float3 Normal           : TEXCOORD0;
    float2 TextureCoords    : TEXCOORD1;
    float4 LightDirection   : TEXCOORD2;
    float4 TextureWeights   : TEXCOORD3;
    float Depth             : TEXCOORD4;
};

struct MTPixelToFrame
{
    float4 Color : COLOR0;
};

MTVertexToPixel MultiTexturedVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0, float4 inTexWeights: TEXCOORD1)
{    
    MTVertexToPixel Output = (MTVertexToPixel)0;
    float4x4 preViewProjection = mul (xView, xProjection);
    float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
    Output.Position = mul(inPos, preWorldViewProjection);
    Output.Normal = mul(normalize(inNormal), (float3x3)xWorld);

    Output.TextureCoords = inTexCoords;

    Output.LightDirection.xyz = -xLightDirection;
    Output.LightDirection.w = 1;

    Output.TextureWeights = inTexWeights;

    Output.Depth = Output.Position.z / Output.Position.w;
    
    return Output;
}

MTPixelToFrame MultiTexturedPS(MTVertexToPixel PSIn)
{
    MTPixelToFrame Output = (MTPixelToFrame)0;        
            
    float lightningFactor = 1;
    if (xEnableLighting)
        lightningFactor = saturate(saturate(dot(PSIn.Normal, PSIn.LightDirection.xyz)) + xAmbient);

    float blendDistance = 0.99f;
    float blendWidth = 0.005f;
    float blendFactor = clamp((PSIn.Depth - blendDistance) / blendWidth, 0, 1);    
    
    float4 farColor;
    farColor = tex2D(TextureSampler0, PSIn.TextureCoords) * PSIn.TextureWeights.x;
    farColor += tex2D(TextureSampler1, PSIn.TextureCoords) * PSIn.TextureWeights.y;
    farColor += tex2D(TextureSampler2, PSIn.TextureCoords) * PSIn.TextureWeights.z;
    farColor += tex2D(TextureSampler3, PSIn.TextureCoords) * PSIn.TextureWeights.w;

    float4 nearColor;
    float2 nearTextureCoords = PSIn.TextureCoords * 3;
    nearColor = tex2D(TextureSampler0, nearTextureCoords) * PSIn.TextureWeights.x;
    nearColor += tex2D(TextureSampler1, nearTextureCoords) * PSIn.TextureWeights.y;
    nearColor += tex2D(TextureSampler2, nearTextureCoords) * PSIn.TextureWeights.z;
    nearColor += tex2D(TextureSampler3, nearTextureCoords) * PSIn.TextureWeights.w;

    Output.Color = lerp(nearColor, farColor, blendFactor);

    Output.Color *= lightningFactor;

    return Output;
}

technique MultiTextured
{
    pass Pass0
#if SM4
    {
        VertexShader = compile vs_4_0_level_9_3 MultiTexturedVS();
        PixelShader = compile ps_4_0_level_9_3 MultiTexturedPS();
    }
#else
    {
        VertexShader = compile vs_1_1 MultiTexturedVS();
        PixelShader = compile ps_2_0 MultiTexturedPS();
    }
}

//------- Technique: Reflection --------

Texture xReflectionTexture;
samplerCUBE SkyboxSampler = sampler_state
{
    texture = <xReflectionTexture>;
    minfilter = LINEAR;
    magfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};

struct RVertexToPixel
{
    float4 Position   : POSITION;
    float4 Normal     : TEXCOORD0;
};

struct RPixelToFrame
{
    float4 Position   : POSITION;
    float3 Reflection : TEXCOORD0;
};

RPixelToFrame ReflectionVS(RVertexToPixel PSIn)
{
    RPixelToFrame Output = (RPixelToFrame)0;

    float4 worldPosition = mul(PSIn.Position, xWorld);
    float4 viewPosition = mul(worldPosition, xView);
    Output.Position = mul(viewPosition, xProjection);

    float4 VertexPosition = mul(PSIn.Position, xWorld);
    float3 ViewDirection = normalize(VertexPosition.xyz - xCameraPosition);

    float3 Normal = normalize(mul(PSIn.Normal.xyz, (float3x3)xWorldInverseTranspose));
    Output.Reflection = reflect(-normalize(ViewDirection), normalize(Normal));

    return Output;
}

float4 ReflectionPS(RPixelToFrame PSIn) : COLOR0
{
    return texCUBE(SkyboxSampler, normalize(PSIn.Reflection));
}

technique Reflection
{
    pass Pass0
#if SM4
    {
        VertexShader = compile vs_4_0_level_9_3 ReflectionVS();
        PixelShader = compile ps_4_0_level_9_3 ReflectionPS();
    }
#else
    {
        VertexShader = compile vs_1_1 ReflectionVS();
        PixelShader = compile ps_2_0 ReflectionPS();
    }
}