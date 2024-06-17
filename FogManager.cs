using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = System.Drawing.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace RTS_Engine;

public class FogManager
{
    public bool FogActive = false;

    #region MasksParameters

    private const int FogResolution = 2;
    private const int TextureSize = 2048;

    #endregion
    
    #region Textures

    private readonly Texture2D _blank = Globals.Content.Load<Texture2D>("blank");
    private readonly Dictionary<int, WeakReference<Texture2D>> _tracks = new();

    #endregion

    #region RenderTargets

    public readonly RenderTarget2D PermanentMaskTarget = new(Globals.GraphicsDevice, TextureSize, TextureSize, false,
        SurfaceFormat.Single, DepthFormat.Depth16,0,RenderTargetUsage.PreserveContents);
    public readonly RenderTarget2D VisibilityMaskTarget = new(Globals.GraphicsDevice, TextureSize, TextureSize, false,
        SurfaceFormat.Single, DepthFormat.Depth16,0,RenderTargetUsage.DiscardContents);

    #endregion
    
    #region Colors

    private readonly Color _visibleColor = new(239,0,0,255);
    private readonly Color _exploredColor = new(15,0,0,255);
    
    private readonly Color[] _color = {new(255,0,0,255)};

    #endregion
    
    public readonly List<FogReveler> Revelers = new();
    public bool Changed;

    public Texture2D GetCircleTexture(int radius)
    {
        if (_tracks.TryGetValue(radius,out WeakReference<Texture2D> texture2D))
        {
            if (texture2D.TryGetTarget(out Texture2D texture))
            {
                return texture;
            }
            _tracks.Remove(radius);
        }
        //If above failed, texture with this radius is not available. Create texture with this radius.
        Texture2D output = CreateTexture(radius);
        if (_tracks.TryAdd(radius, new WeakReference<Texture2D>(output)))
        {
            return output;
        }
        throw new DataException("Something went wrong!");
    }
    public void UpdateFog()
    {
        if (Changed && FogActive)
        {
            Changed = true;
            Globals.GraphicsDevice.SetRenderTarget(PermanentMaskTarget);
            Globals.SpriteBatch.Begin();
            foreach (FogReveler reveler in Revelers)
            {
                if (reveler.Changed)
                {
                    Globals.SpriteBatch.Draw(reveler.Track,new Vector2(reveler.CurrentPosition.X/ (float)FogResolution,reveler.CurrentPosition.Y/ (float)FogResolution),null,_exploredColor,0,
                        new Vector2(reveler.RevealRadius),new Vector2(1.0f / FogResolution),SpriteEffects.None,0);
                    reveler.Changed = false;
                    reveler.PreviousPosition = reveler.CurrentPosition;
                }
            }
            Globals.SpriteBatch.End();
            Globals.GraphicsDevice.SetRenderTarget(VisibilityMaskTarget);
            Globals.SpriteBatch.Begin();
            foreach (FogReveler reveler in Revelers)
            {
                Globals.SpriteBatch.Draw(reveler.Track,new Vector2(reveler.CurrentPosition.X / (float)FogResolution,reveler.CurrentPosition.Y / (float)FogResolution),null,_visibleColor,0,
                    new Vector2(reveler.RevealRadius),new Vector2(1.0f / FogResolution),SpriteEffects.None,0);
            }
            Globals.SpriteBatch.End();
            Globals.GraphicsDevice.SetRenderTarget(null);
            Globals.MainEffect.Parameters["discovery"]?.SetValue(PermanentMaskTarget);
            Globals.MainEffect.Parameters["visibility"]?.SetValue(VisibilityMaskTarget);
            
            Globals.TerrainEffect.Parameters["discovery"]?.SetValue(PermanentMaskTarget);
            Globals.TerrainEffect.Parameters["visibility"]?.SetValue(VisibilityMaskTarget);
        }
        else if(!FogActive)
        {
            Globals.MainEffect.Parameters["discovery"]?.SetValue(_blank);
            Globals.MainEffect.Parameters["visibility"]?.SetValue(_blank);
            
            Globals.TerrainEffect.Parameters["discovery"]?.SetValue(_blank);
            Globals.TerrainEffect.Parameters["visibility"]?.SetValue(_blank);
        }
    }
    
    public bool IsVisible(Vector3 position)
    {
        if (!FogActive) return true;
        foreach (FogReveler reveler in Revelers)
        {
            float distance = MathF.Sqrt(MathF.Pow(reveler.CurrentPosition.X - position.X, 2) +
                                        MathF.Pow(reveler.CurrentPosition.Y - position.Z, 2));
            if (distance <= reveler.RevealRadius)
            {
                return true;
            }
        }
        return false;
    }


    private Texture2D CreateTexture(int radius)
    {
        Texture2D texture = new Texture2D(Globals.GraphicsDevice, 1 + radius * 2, 1 + radius * 2);
        MakeHole(texture,new Point(radius,radius),radius,_color);
        return texture;
    }
    
    private void MakeHole(Texture2D texture, Point position, int radius, Color[] color)
    {
        int px, nx, py, ny, distance;
        for (int i = 0; i < radius; i++)
        {
            distance = (int)MathF.Round(MathF.Sqrt((float)radius * radius - i * i));
            for (int j = 0; j < distance; j++)
            {
                px = Math.Clamp(position.X + i, 0, TextureSize);
                nx = Math.Clamp(position.X - i, 0, TextureSize);
                py = Math.Clamp(position.Y + j, 0, TextureSize);
                ny = Math.Clamp(position.Y - j, 0, TextureSize);
                texture.SetData(0,new Rectangle(px,py,1,1),color,0,1);
                texture.SetData(0,new Rectangle(nx,py,1,1),color,0,1);
                texture.SetData(0,new Rectangle(px,ny,1,1),color,0,1);
                texture.SetData(0,new Rectangle(nx,ny,1,1),color,0,1);
            }
        }
    }
}