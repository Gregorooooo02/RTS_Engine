using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;

namespace RTS_Engine;

public class GenerateMap
{
    public static NoiseField<float> perlinNoise;
    public static Texture2D noiseTexture;
    public static PerlinNoiseGenerator perlinGen = new PerlinNoiseGenerator();

    private static bool isColored = false;

    public static void GenerateNoiseTexture()
    {
        perlinGen.OctaveCount = 8;
        perlinGen.Persistence = .5f;
        perlinGen.Interpolation = Helpers.CosInterpolation;

        perlinNoise = perlinGen.GeneratePerlinNoise(512, 512);

        
        LinearGradientFilter filter = new LinearGradientFilter();    
        Texture2DTransformer transformer = new Texture2DTransformer(Globals.GraphicsDevice);

        noiseTexture = transformer.Transform(filter.Filter(perlinNoise));
        
        SaveTextureData(noiseTexture, "Content/heightmap");
        // ExtractSubTextures(noiseTexture, "Content/heightmap");
    }

    private static void SaveTextureData(Texture2D texture, string filename)
    {
        Color[] data = new Color[texture.Width * texture.Height];
        texture.GetData(data);
        Texture2D newTexture = new Texture2D(Globals.GraphicsDevice, texture.Width, texture.Height);
        newTexture.SetData(data);
        newTexture.SaveAsPng(new System.IO.FileStream(filename, System.IO.FileMode.Create), texture.Width, texture.Height);
    }

    private static void ExtractSubTextures(Texture2D texture, string baseFilename)
    {
        int subTextureSize = 128;
        Color[] data = new Color[texture.Width * texture.Height];
        
        texture.GetData(data);

        Point[] positions =
        {
            new Point(0, 0),
            new Point(subTextureSize , 0),
            new Point(0, subTextureSize ),
            new Point(subTextureSize, subTextureSize)
        };

        for (int i = 0; i < 4; i++)
        {
            Point pos = positions[i];
            Texture2D subTexture = new Texture2D(Globals.GraphicsDevice, subTextureSize, subTextureSize);
            Color[] subData = new Color[subTextureSize * subTextureSize];
            
            for (int x = 0; x < subTextureSize; x++)
            {
                for (int y = 0; y < subTextureSize; y++)
                {
                    subData[x + y * subTextureSize] = data[(x + pos.X) + (y + pos.Y) * texture.Width];
                }
            }
            
            subTexture.SetData(subData);

            using (FileStream stream = new FileStream($"{baseFilename}_{i}.bmp", FileMode.Create))
            {
                subTexture.SaveAsPng(stream, subTexture.Width, subTexture.Height);
            }
        }
    }
}
