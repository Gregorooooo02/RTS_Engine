using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ImGuiNET;
using System;

namespace RTS_Engine;

public class TextRenderer : Component
{
    public string Content = "Sample text";
    public SpriteFont Font;
    public Color Color = Color.White;
    
    public override void Update(){}

    public TextRenderer()
    {
        
    }

    public override void Draw(Matrix _view, Matrix _projection)
    {
        if(!Active) return;
        Globals.Instance.SpriteBatch.DrawString(
            Font,
            Content,
            new Vector2(ParentObject.Transform._pos.X,ParentObject.Transform._pos.Y),
            Color,
            MathHelper.ToRadians(ParentObject.Transform._rot.Z),
            new Vector2(0,0),
            new Vector2(ParentObject.Transform._scl.X,ParentObject.Transform._scl.Y),
            SpriteEffects.None,
            0);
        
    }

    public override void Initialize()
    {
        Font = AssetManager.DefaultFont;
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
                AssetManager.FreeFont(Font);
            }
        }   
    }
#endif
}