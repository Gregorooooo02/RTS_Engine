using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class CustomGradientFilter : INoiseFilter<float, Color>
{
    public struct ColorPair
    {
        public float Start;
        public float End;
        public Color Color;

        public ColorPair(float start, float end, Color color)
        {
            this.Start = start;
            this.End = end;
            this.Color = color;
        }

        public bool IsInRange(float value)
        {
            return value < End && value >= Start;
        }

        public override string ToString()
        {
            return Color + "[" + Start + ", " + End + "]";
        }
    }

    public List<ColorPair> Colors { get; set; }

    public CustomGradientFilter()
    {
        Colors = new List<ColorPair>();
    }

    public NoiseField<Color> Filter(NoiseField<float> noiseField)
    {
        NoiseField<Color> result = new NoiseField<Color>(noiseField.Width, noiseField.Height);

        for (int x = 0; x < noiseField.Width; x++)
        {
            for (int y = 0; y < noiseField.Height; y++)
            {
                float fieldValue = noiseField.Field[x, y];

                foreach (ColorPair colorPair in Colors)
                {
                    if (colorPair.IsInRange(fieldValue))
                    {
                        float colorMultiplier = fieldValue / colorPair.End;

                        result.Field[x, y] = new Color(
                            (int)(colorPair.Color.R * colorMultiplier),
                            (int)(colorPair.Color.G * colorMultiplier),
                            (int)(colorPair.Color.B * colorMultiplier)
                        );
                    }
                }
            }
        }

        return result;
    }

    public void AddColorPoint(float start, float end, Color color)
    {
        ColorPair pair = new ColorPair(start, end, color);

        Colors.Add(pair);
    }
}
