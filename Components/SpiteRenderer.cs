using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;

namespace RTS_Engine;

public class SpiteRenderer : Component
{
    public Texture2D Sprite;
    public Color Color = Color.White;
    public bool useLocalPosition = true;

    private Point currentSize;
    
    public SpiteRenderer(GameObject parentObject, Texture2D sprite)
    {
        ParentObject = parentObject;
        Sprite = sprite;
    }
    
    public SpiteRenderer()
    {
        
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

    public override void Update()
    {
        if(currentSize.X != Globals.GraphicsDeviceManager.PreferredBackBufferWidth || currentSize.Y != Globals.GraphicsDeviceManager.PreferredBackBufferHeight) Resize();
    }

    public void Draw()
    {
        if (!Active || !ParentObject.Active) return;
        if(useLocalPosition)
        {
            Globals.SpriteBatch?.Draw(Sprite,
            new Rectangle(
                (int)ParentObject.Transform.Pos.X,
                (int)ParentObject.Transform.Pos.Y,
                (int)(Sprite.Width * ParentObject.Transform.Scl.X),
                (int)(Sprite.Height * ParentObject.Transform.Scl.Y)),
                null,
                Color,
                MathHelper.ToRadians(ParentObject.Transform.Rot.Z),
                Vector2.Zero,
                SpriteEffects.None,
                ParentObject.Transform.Pos.Z);
        } else
        {
            ParentObject.Transform.ModelMatrix.Decompose(out Vector3 scale, out Quaternion k, out Vector3 v);
            Globals.SpriteBatch?.Draw(Sprite,
           new Rectangle(
               (int)v.X,
               (int)v.Y,
               (int)(Sprite.Width * scale.X),
               (int)(Sprite.Height * scale.Y)),
               null,
               Color,
               MathHelper.ToRadians(ParentObject.Transform.Rot.Z),
               Vector2.Zero,
               SpriteEffects.None,
               ParentObject.Transform.Pos.Z);
        }
        
    }

    public override void Initialize()
    {
        Globals.Renderer.Sprites.Add(this);
        Sprite = AssetManager.DefaultSprite;
        currentSize = new Point(Globals.GraphicsDeviceManager.PreferredBackBufferWidth,
            Globals.GraphicsDeviceManager.PreferredBackBufferHeight);
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>SpiteRenderer</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<useLocal>" + useLocalPosition +"</useLocal>");
        
        builder.Append("<sprite>" + Sprite.Name + "</sprite>");
        
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
        LoadSprite(element.Element("sprite").Value);
        XElement color = element.Element("color");
        Color = new Color(int.Parse(color.Element("r").Value),int.Parse(color.Element("g").Value),int.Parse(color.Element("b").Value),int.Parse(color.Element("a").Value));

        currentSize =
            int.TryParse(element.Element("screenSizeX")?.Value, out int x) &&
            int.TryParse(element.Element("screenSizeY")?.Value, out int y)
                ? new Point(x, y)
                : new Point(1440, 900);
    }

    public override void RemoveComponent()
    {
        //Also remove associated button if it's exists anyway
        Button button = ParentObject.GetComponent<Button>();
        if (button != null && button.ButtonVisual == this)
        {
            ParentObject.RemoveComponent(button);
        }
        
        Globals.Renderer.Sprites.Remove(this);
        ParentObject.RemoveComponent(this);
        AssetManager.FreeSprite(Sprite);
    }

    public void LoadSprite(string name)
    {
        Sprite = AssetManager.GetSprite(name);
    }

#if DEBUG
    private bool _switchingSprites = false;
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Sprite Renderer"))
        {
            ImGui.Checkbox("Sprite active", ref Active);
            ImGui.Text(Sprite.Name);
            ImGui.Checkbox("Use local postion", ref useLocalPosition);
            System.Numerics.Vector4 temp = Color.ToVector4().ToNumerics();
            if (ImGui.ColorEdit4("Sprite Color", ref temp))
            {
                Color = new Color(temp);
            }

            if (ImGui.Button("Switch sprite"))
            {
                _switchingSprites = true;
            }
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }

            if (_switchingSprites)
            {
                ImGui.Begin("Switching sprites");
                foreach (string n in AssetManager.SpriteNames)
                {
                    if (ImGui.Button(n))
                    {
                        AssetManager.FreeSprite(Sprite);
                        Sprite = AssetManager.GetSprite(n);
                        _switchingSprites = false;
                    }
                }
                if (ImGui.Button("Cancel selection"))
                {
                    _switchingSprites = false;
                }
                ImGui.End();
            }
        }   
    }
#endif
}