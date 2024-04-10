using System;
using System.Collections.Generic;

namespace RTS_Engine;

public class NoiseField<T>
{
    public T[,] Field { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public NoiseField(int width, int height)
    {
        Width = width;
        Height = height;
        Field = new T[width, height];
    }

    public T[] Flatten()
    {
        T[] arr = new T[Width * Height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                arr[x + (y * Height)] = Field[x, y];
            }
        }

        return arr;
    }
}
