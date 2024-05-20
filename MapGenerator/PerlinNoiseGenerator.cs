using System;
using System.Numerics;
using System.Reflection;

namespace RTS_Engine;

public delegate float InterpolationAlgorithm(float a, float b, float t);

public class PerlinNoiseGenerator
{
        public int OctaveCount { get; set; }
        public float Persistence { get; set; }
        public Random Random { get; set; }
        
        public InterpolationAlgorithm Interpolation { get; set; }

        public PerlinNoiseGenerator() {
            OctaveCount = 4;
            Persistence = 0.5f;
            Interpolation = Helpers.LinearInterpolation;
            Random = new Random();
        }
        
        public NoiseField<float> GenerateWhiteNoise(int width, int height) {
            return GenerateWhiteNoise(width, height, Random);
        }
        
        public NoiseField<float> GenerateWhiteNoise(int width, int height, Random random) {
            NoiseField<float> field = new NoiseField<float>(width, height);

            for(int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    field.Field[x, y] = (float)random.NextDouble() % 1;
                }
            }

            return field;
        }
        
        public NoiseField<float> SmoothNoiseField(NoiseField<float> whiteNoise, int octave) {
            NoiseField<float> smooth = new NoiseField<float>(whiteNoise.Width, whiteNoise.Height);

            int samplePeriod = 1 << octave;
            float sampleFrequency = 1.0f / samplePeriod;

            for(int x = 0; x < smooth.Width; x++) {
                int sampleX1 = (x / samplePeriod) * samplePeriod;
                int sampleX2 = (sampleX1 + samplePeriod) % smooth.Width;

                float horizontalBlend = (x - sampleX1) * sampleFrequency;

                for(int y = 0; y < smooth.Height; y++) {
                    int sampleY1 = (y / samplePeriod) * samplePeriod;
                    int sampleY2 = (sampleY1 + samplePeriod) % smooth.Height;

                    float verticalBlend = (y - sampleY1) * sampleFrequency;

                    float top = Interpolation(whiteNoise.Field[sampleX1, sampleY1], whiteNoise.Field[sampleX2, sampleY1], horizontalBlend);
                    float bottom = Interpolation(whiteNoise.Field[sampleX1, sampleY2], whiteNoise.Field[sampleX2, sampleY2], horizontalBlend);

                    smooth.Field[x, y] = Interpolation(top, bottom, verticalBlend);
                }
            }

            return smooth;
        }

        public NoiseField<float> PerlinNoiseField(NoiseField<float> baseNoise)
        {
            NoiseField<float>[] smoothNoise = new NoiseField<float>[OctaveCount];

            for (int i = 0; i < OctaveCount; i++)
            {
                smoothNoise[i] = SmoothNoiseField(baseNoise, i);
            }

            NoiseField<float> perlinNoise = new NoiseField<float>(baseNoise.Width, baseNoise.Height);
            float amplitude = 1.0f;
            float totalAmplitude = 0.0f;

            for (int octave = OctaveCount - 1; octave >= 0; octave--)
            {
                amplitude *= Persistence;
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
        
        public float CalculateFalloff(int x, int y, int width, int height)
        {
            float nx = (2.0f * x) / width - 1f;
            float ny = (2.0f * y) / height - 1f;
            
            float distance = MathF.Sqrt(nx * nx + ny * ny);
            
            return Helpers.Clamp01(1.25f - distance);
        }

        public NoiseField<float> GeneratePerlinNoise(int width, int height) {
            NoiseField<float> whiteNoise = GenerateWhiteNoise(width, height);
            NoiseField<float> perlinNoise = PerlinNoiseField(whiteNoise);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float falloff = CalculateFalloff(x, y, width, height);
                    perlinNoise.Field[x, y] *= falloff;
                }
            }
            
            return perlinNoise;
        }
}
