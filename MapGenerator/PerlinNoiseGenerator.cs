using System;

namespace RTS_Engine;

public delegate float InterpolationAlgorithm(float a, float b, float t);

public class PerlinNoiseGenerator
{
    public int Octaves { get; set; }
    public float Persistance { get; set; }
    public InterpolationAlgorithm Interpolation { get; set; }
    public Random Random { get; set; }

    public PerlinNoiseGenerator()
    {
        Octaves = 4;
        Persistance = 0.5f;
        Interpolation = Helpers.LinearInterpolation;
        Random = new Random();
    }

    public NoiseField<float> GenerateWhiteNoise(int width, int height)
    {
        NoiseField<float> noiseField = new NoiseField<float>(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                noiseField.Field[x, y] = (float)Random.NextDouble() % 1;
            }
        }

        return noiseField;
    }

    public NoiseField<float> SmoothNoiseField(NoiseField<float> whiteNoise, int octaves)
    {
        NoiseField<float> smoothNoise = new NoiseField<float>(whiteNoise.Width, whiteNoise.Height);

        int samplePeriod = 1 << octaves;
        float sampleFrequency = 1.0f / samplePeriod;

        for (int x = 0; x < smoothNoise.Width; x++)
        {
            int sampleX0 = (x / samplePeriod) * samplePeriod;
            int sampleX1 = (sampleX0 + samplePeriod) % smoothNoise.Width;

            float horizontalBlend = (x - sampleX0) * sampleFrequency;

            for (int y = 0; y < smoothNoise.Height; y++)
            {
                int sampleY0 = (y / samplePeriod) * samplePeriod;
                int sampleY1 = (sampleY0 + samplePeriod) % smoothNoise.Height;

                float verticalBlend = (y - sampleY0) * sampleFrequency;

                float top = Interpolation(
                    whiteNoise.Field[sampleX0, sampleY0],
                    whiteNoise.Field[sampleX1, sampleY0],
                    horizontalBlend
                );

                float bottom = Interpolation(
                    whiteNoise.Field[sampleX0, sampleY1],
                    whiteNoise.Field[sampleX1, sampleY1],
                    horizontalBlend
                );

                smoothNoise.Field[x, y] = Interpolation(top, bottom, verticalBlend);
            }
        }

        return smoothNoise;
    }

    public NoiseField<float> PerlinNoiseField(NoiseField<float> baseNoise)
    {
        NoiseField<float>[] smoothNoise = new NoiseField<float>[Octaves];

        for (int i = 0; i < Octaves; i++)
        {
            smoothNoise[i] = SmoothNoiseField(baseNoise, i);
        }

        NoiseField<float> perlinNoise = new NoiseField<float>(baseNoise.Width, baseNoise.Height);
        float amplitude = 1.0f;
        float totalAmplitude = 0.0f;

        for (int octave = Octaves - 1; octave >= 0; octave--)
        {
            amplitude *= Persistance;
            totalAmplitude += amplitude;

            for (int x = 0; x < baseNoise.Width; x++)
            {
                for (int y = 0; y < baseNoise.Height; y++)
                {
                    perlinNoise.Field[x, y] += smoothNoise[octave].Field[x, y] * amplitude;
                }
            }
        }

        // Normalize the fields
        for (int x = 0; x < baseNoise.Width; x++)
        {
            for (int y = 0; y < baseNoise.Height; y++)
            {
                perlinNoise.Field[x, y] /= totalAmplitude;
            }
        }

        return perlinNoise;
    }
}
