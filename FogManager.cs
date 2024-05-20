using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace RTS_Engine;

public class FogManager
{
    public readonly int FogResolution = 2;
    public readonly int TextureSize = 2048;
    
    public bool FogActive = true;

    private Texture2D _permanentMask;
    private Texture2D _visibilityMask;
    
    private readonly Texture2D _blank = Globals.Content.Load<Texture2D>("blank");

    public readonly List<FogReveler> Revelers = new();

    public bool Changed = false;

    private bool _isRunning = false;

    private Microsoft.Xna.Framework.Color[] _visibleColor = new []{new Color(240,0,0,255)};
    private Microsoft.Xna.Framework.Color[] _exploredColor = new []{new Color(15,0,0,255)};
    
    public FogManager()
    {
        _permanentMask = new(Globals.GraphicsDevice, TextureSize, TextureSize);
        _visibilityMask = new(Globals.GraphicsDevice, TextureSize, TextureSize);
    }

    public void UpdateFog()
    {
        if (Changed && FogActive)
        {
            if(_isRunning) return;
            _isRunning = true;
            Changed = false;
            foreach (FogReveler reveler in Revelers)
            {
                if (reveler.Changed)
                {
                    MakeHole(_permanentMask,reveler.CurrentPosition,reveler.RevealRadius,_exploredColor);
                    reveler.Changed = false;
                    reveler.PreviousPosition = reveler.CurrentPosition;
                }
                _visibilityMask = new(Globals.GraphicsDevice, TextureSize, TextureSize);
                MakeHole(_visibilityMask,reveler.CurrentPosition,reveler.RevealRadius,_visibleColor);
            }
            Globals.MainEffect.Parameters["discovery"]?.SetValue(GetPermanent());
            Globals.MainEffect.Parameters["visibility"]?.SetValue(GetVisibility());
            _isRunning = false;
        }
    }
    public async Task UpdateFogAsync()
    {
        await Task.Run(UpdateFog);
    }

    public Texture2D GetPermanent()
    {
        return FogActive ? _permanentMask : _blank;
    }

    public Texture2D GetVisibility()
    {
        return FogActive ? _visibilityMask : _blank;
    }
    
    private void MakeHole(Texture2D texture, Point position, float radiusF, Color[] color)
    {
        position.X /= FogResolution;
        position.Y /= FogResolution;
        int radius = (int)MathF.Round(radiusF,0) / FogResolution;
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
    public void ResetFog()
    {
        _permanentMask = new(Globals.GraphicsDevice, TextureSize, TextureSize);
        _visibilityMask = new(Globals.GraphicsDevice, TextureSize, TextureSize);
    }
}