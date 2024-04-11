using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class Texture2DTransformer : INoiseTransformer<float, Texture2D>, INoiseTransformer<Color, Texture2D>
{
    public GraphicsDevice Graphics { get; set; }

    public Texture2DTransformer(GraphicsDevice graphics)
    {
        Graphics = graphics;
    }

    public Texture2D Transform(NoiseField<Color> field)
    {
        Color[] data = field.Flatten();
        Texture2D texture = new Texture2D(Graphics, field.Width, field.Height);
        texture.SetData<Color>(data);

        return texture;
    }

    public Texture2D Transform(NoiseField<float> field)
    {
        float[] data = field.Flatten();
        Color[] colors = new Color[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            colors[i] = new Color(data[i], data[i], data[i]);
        }

        Texture2D texture = new Texture2D(Graphics, field.Width, field.Height);
        texture.SetData<Color>(colors);

        return texture;
    }
}
