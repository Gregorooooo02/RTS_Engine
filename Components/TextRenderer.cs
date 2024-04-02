﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ImGuiNET;

namespace RTS_Engine;

public class TextRenderer : Component
{
    public string Content = "Sample text";
    public SpriteFont Font;
    public Color Color = Color.White;
    
    public override void Update(){}

    public TextRenderer()
    {
        Initialize();
    }

    public override void Draw()
    {
        Globals.Instance.SpriteBatch.DrawString(
            Font,
            Content,
            new Vector2(ParentObject.Transform._pos.X,ParentObject.Transform._pos.Y),
            Color,
            ParentObject.Transform._rot.Z,
            new Vector2(0,0),
            new Vector2(ParentObject.Transform._scl.X,ParentObject.Transform._scl.Y),
            SpriteEffects.None,
            0);
        
    }

    public override void Initialize()
    {
        Font = Globals.Instance.DefaultFont;
    }

#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Text Renderer"))
        {
            ImGui.Checkbox("Text active", ref Active);
            System.Numerics.Vector4 temp = Color.ToVector4().ToNumerics();
            if (ImGui.ColorEdit4("Text Color", ref temp))
            {
                Color = new Color(temp);
            }
            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
            }
        }   
    }
#endif
}