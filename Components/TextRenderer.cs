using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ImGuiNET;
using System.Text;

namespace RTS_Engine;

public class TextRenderer : Component
{
    public string Content = "Sample text";
    public string NewContent;
    public SpriteFont Font;
    private string name;
    public Color Color = Color.White;
    
    public override void Update() 
    {
        if (NewContent != null) 
        {
            Content = NewContent;
            NewContent = null;
        }
    }

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

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>TextRenderer</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<contents>" + Content +"</contents>");

        builder.Append("<font>" + name + "</font>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

#if DEBUG
    private bool _switchingFont = false;
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
            ImGui.InputText("Contents", ref Content, 200);
            if (ImGui.Button("Switch font"))
            {
                _switchingFont = true;
            }
            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
                AssetManager.FreeFont(Font);
            }

            if (_switchingFont)
            {
                ImGui.Begin("Switching fonts");
                foreach (string n in AssetManager.FontNames)
                {
                    if (ImGui.Button(n))
                    {
                        AssetManager.FreeFont(Font);
                        Font = AssetManager.GetFont(n);
                        _switchingFont = false;
                    }
                }
                if (ImGui.Button("Cancel selection"))
                {
                    _switchingFont = false;
                }
                ImGui.End();
            }
        }   
    }
#endif
}