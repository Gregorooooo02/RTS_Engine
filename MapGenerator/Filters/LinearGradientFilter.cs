using System;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class LinearGradientFilter : INoiseFilter<float, Color>
{
    public Color StartColor { get; set; }
    public Color EndColor { get; set; }
    public float StartPercentage { get; set; }

    public LinearGradientFilter()
    {
        StartColor = Color.Black;
        EndColor = Color.White;
        StartPercentage = 1.0f;
    }

    public NoiseField<Color> Filter(NoiseField<float> noiseField)
    {
        NoiseField<Color> result = new NoiseField<Color>(noiseField.Width, noiseField.Height);

        for (int x = 0; x < noiseField.Width; x++)
        {
            for (int y = 0; y < noiseField.Height; y++)
            {
                float t = noiseField.Field[x,y];
                float u = StartPercentage - t;
                t *= StartPercentage;

                result.Field[x,y] = new Color(
                    (int)(u * StartColor.R + t * EndColor.R),
                    (int)(u * StartColor.G + t * EndColor.G),
                    (int)(u * StartColor.B + t * EndColor.B)
                );
            }
        }

        return result;
    }
}
