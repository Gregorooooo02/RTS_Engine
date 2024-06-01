using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ImGuiNET;
using System.Text;
using System.Xml.Linq;

namespace RTS_Engine;

public class TextRenderer : Component
{
    public string Content = "Sample text";
    public string NewContent;
    public SpriteFont Font;
    private string _name;
    public Color Color = Color.White;
    public bool useLocalPosition = true;

    private Point currentSize;
    
    public override void Update() 
    {
        if (NewContent != null) 
        {
            Content = NewContent;
            NewContent = null;
        }
        if(currentSize.X != Globals.GraphicsDeviceManager.PreferredBackBufferWidth || currentSize.Y != Globals.GraphicsDeviceManager.PreferredBackBufferHeight) Resize();
    }

    private void Resize()
    {
        if(!useLocalPosition) return;
        float ratio = Globals.GraphicsDeviceManager.PreferredBackBufferWidth / (float)currentSize.X;
        ParentObject.Transform.SetLocalPosition(ParentObject.Transform.Pos * ratio);
        ParentObject.Transform.SetLocalScale(ParentObject.Transform.Scl * ratio);
        currentSize = new Point(Globals.GraphicsDeviceManager.PreferredBackBufferWidth,
            Globals.GraphicsDeviceManager.PreferredBackBufferHeight);
    }
    
    public TextRenderer()
    {
        
    }

    public void Draw()
    {
        if(!Active || !ParentObject.Active) return;
        if(useLocalPosition)
        {
            Globals.SpriteBatch.DrawString(
            Font,
            Content,
            new Vector2(ParentObject.Transform.Pos.X, ParentObject.Transform.Pos.Y),
            Color,
            MathHelper.ToRadians(ParentObject.Transform.Rot.Z),
            new Vector2(0, 0),
            new Vector2(ParentObject.Transform.Scl.X, ParentObject.Transform.Scl.Y),
            SpriteEffects.None,
            ParentObject.Transform.Pos.Z);
        } else
        {
            ParentObject.Transform.ModelMatrix.Decompose(out Vector3 scale, out Quaternion k, out Vector3 v);
            Globals.SpriteBatch.DrawString(
            Font,
            Content,
            new Vector2(v.X, v.Y),
            Color,
            MathHelper.ToRadians(ParentObject.Transform.Rot.Z),
            new Vector2(0, 0),
            new Vector2(scale.X, scale.Y),
            SpriteEffects.None,
            ParentObject.Transform.Pos.Z);
        }
        
    }

    public override void Initialize()
    {
        Font = AssetManager.DefaultFont;
        _name = "defaultFont";
        Globals.Renderer.Texts.Add(this);
        currentSize = new Point(Globals.GraphicsDeviceManager.PreferredBackBufferWidth,
            Globals.GraphicsDeviceManager.PreferredBackBufferHeight);
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>TextRenderer</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<useLocal>" + useLocalPosition +"</useLocal>");
        
        builder.Append("<contents>" + Content +"</contents>");

        builder.Append("<font>" + _name + "</font>");
        
        builder.Append("<color>");
        builder.Append("<r>" + Color.R + "</r>");
        builder.Append("<g>" + Color.G + "</g>");
        builder.Append("<b>" + Color.B + "</b>");
        builder.Append("<a>" + Color.A + "</a>");
        builder.Append("</color>");
        
        builder.Append("<screenSizeX>" + currentSize.X + "</screenSizeX>");
        builder.Append("<screenSizeY>" + currentSize.Y + "</screenSizeY>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        useLocalPosition = element.Element("useLocal")?.Value == "True";
        Content = element.Element("contents").Value;
        LoadFont(element.Element("font").Value);
        
        XElement color = element.Element("color");
        if (color == null) Color = new Color(255, 255, 255);
        else Color = new Color(int.Parse(color.Element("r")?.Value),int.Parse(color.Element("g").Value),int.Parse(color.Element("b").Value),int.Parse(color.Element("a").Value));

        currentSize =
            int.TryParse(element.Element("screenSizeX")?.Value, out int x) &&
            int.TryParse(element.Element("screenSizeY")?.Value, out int y)
                ? new Point(x, y)
                : new Point(1440, 900);
    }

    public override void RemoveComponent()
    {
        Globals.Renderer.Texts.Remove(this);
        ParentObject.RemoveComponent(this);
        AssetManager.FreeFont(Font);
    }

    public void LoadFont(string name)
    {
        Font = AssetManager.GetFont(name);
        _name = name;
    }

#if DEBUG
    private bool _switchingFont = false;
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Text Renderer"))
        {
            ImGui.Checkbox("Text active", ref Active);
            ImGui.Checkbox("Use local postion", ref useLocalPosition);
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
                RemoveComponent();
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