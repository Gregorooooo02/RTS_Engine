﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;

namespace RTS_Engine;

public class SpiteRenderer : Component
{
    public Texture2D Sprite;
    public Color Color = Color.White;

    
    public SpiteRenderer(GameObject parentObject, Texture2D sprite)
    {
        ParentObject = parentObject;
        Sprite = sprite;
    }
    
    public SpiteRenderer()
    {
        
    }
    
    public override void Update(){}

    public override void Draw(Matrix _view, Matrix _projection)
    {
        Globals.Instance.SpriteBatch?.Draw(Sprite,
            new Rectangle(
                (int)ParentObject.Transform._pos.X,
                (int)ParentObject.Transform._pos.Y,
                (int)(Sprite.Width * ParentObject.Transform._scl.X),
                (int)(Sprite.Height * ParentObject.Transform._scl.Y)),
                null,
                Color,
                MathHelper.ToRadians(ParentObject.Transform._rot.Z),
                Vector2.Zero,
                SpriteEffects.None,
                1);
    }

    public override void Initialize()
    {
        Sprite = AssetManager.DefaultSprite;
    }

#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Sprite Renderer"))
        {
            ImGui.Checkbox("Sprite active", ref Active);
            System.Numerics.Vector4 temp = Color.ToVector4().ToNumerics();
            if (ImGui.ColorEdit4("Sprite Color", ref temp))
            {
                Color = new Color(temp);
            }
            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
                AssetManager.FreeSprite(Sprite);
            }
        }   
    }
#endif
}