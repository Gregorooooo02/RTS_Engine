using System.Text;
using Microsoft.Xna.Framework;
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

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>SpriteRenderer</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<sprite>" + Sprite.Name + "</sprite>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

#if DEBUG
    private bool _switchingSprites = false;
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Sprite Renderer"))
        {
            ImGui.Checkbox("Sprite active", ref Active);
            ImGui.Text(Sprite.Name);
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
                ParentObject.RemoveComponent(this);
                AssetManager.FreeSprite(Sprite);
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