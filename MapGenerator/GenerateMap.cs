using System;
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
        perlinGen.Interpolation = Helpers.CosInterpolation;

        perlinNoise = perlinGen.GeneratePerlinNoise(128, 128);

        if (!isColored)
        {
            LinearGradientFilter filter = new LinearGradientFilter();    

            Texture2DTransformer transformer = new Texture2DTransformer(Globals.GraphicsDevice);

            noiseTexture = transformer.Transform(filter.Filter(perlinNoise));
        } else {
            CustomGradientFilter filter = new CustomGradientFilter();

            filter.AddColorPoint(0.0f, 0.4f, Color.RoyalBlue);
            filter.AddColorPoint(0.4f, 0.5f, new Color(255, 223, 135));
            filter.AddColorPoint(0.5f, 0.7f, new Color(117, 255, 89));
            filter.AddColorPoint(0.7f, 0.9f, new Color(117, 105, 89));
            filter.AddColorPoint(0.9f, 1.0f, Color.White);

            Texture2DTransformer transformer = new Texture2DTransformer(Globals.GraphicsDevice);

            noiseTexture = transformer.Transform(filter.Filter(perlinNoise));
        }

        SaveTextureData(noiseTexture, "Content/heightmap.bmp");
    }

    private static void SaveTextureData(Texture2D texture, string filename)
    {
        Color[] data = new Color[texture.Width * texture.Height];
        texture.GetData(data);
        Texture2D newTexture = new Texture2D(Globals.GraphicsDevice, texture.Width, texture.Height);
        newTexture.SetData(data);
        newTexture.SaveAsPng(new System.IO.FileStream(filename, System.IO.FileMode.Create), texture.Width, texture.Height);
    }
}
