float4x4 xViewProjection;


 Texture xTexture;

sampler TextureSampler = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = clamp; AddressV = clamp;};

struct VertexToPixel
{
    float4 Position     : POSITION;
    
    float4 Color        : COLOR;

     float2 TexCoords    : TEXCOORD0;

};

struct PixelToFrame
{
    float4 Color        : COLOR0;
};


 VertexToPixel SimplestVertexShader( float4 inPos : POSITION, float4 color : COLOR, float2 inTexCoords : TEXCOORD0)

{
    VertexToPixel Output = (VertexToPixel)0;
    
    Output.Position =mul(inPos, xViewProjection);
    Output.Color = color;

     Output.TexCoords = inTexCoords;


    return Output;
}

PixelToFrame OurFirstPixelShader(VertexToPixel PSIn)
{
    PixelToFrame Output = (PixelToFrame)0;    


     Output.Color = tex2D(TextureSampler, PSIn.TexCoords);
     float a = PSIn.Color[3]*Output.Color[0];
     Output.Color = Output.Color * PSIn.Color;
     Output.Color[3] = a;



    return Output;
}

technique Simplest
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 SimplestVertexShader();
        PixelShader = compile ps_2_0 OurFirstPixelShader();
    }
}