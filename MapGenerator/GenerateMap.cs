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
    }

    public static void MapInspector()
    {
        int octaves = perlinGen.Octaves; // Store the value in a variable
        float persistance = perlinGen.Persistance; // Store the value in a variable

        ImGui.Begin("Map Inspector");

        if (ImGui.Checkbox("Color Gradient", ref isColored))
        {
            GenerateNoiseTexture();
        }
        if (ImGui.SliderInt("Octaves", ref perlinGen.Octaves, 1, 10))
        {
            GenerateNoiseTexture();
        }
        if (ImGui.SliderFloat("Persistance", ref perlinGen.Persistance, 0.01f, 1.0f))
        {
            GenerateNoiseTexture();
        }

        ImGui.End();
    }
}
