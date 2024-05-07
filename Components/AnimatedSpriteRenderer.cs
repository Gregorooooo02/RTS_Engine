using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class AnimatedSpriteRenderer : Component
{
    public Texture2D SpriteSheet { get; private set;} = AssetManager.DefaultAnimatedSprite;
    public Color Color = Color.White;
    private List<Rectangle> _sourceRectangles = new();
    private int _frames;
    private float _frameTime;
    private int _frame;
    private float _frameTimeLeft;
    private bool _isAnimationActive = true;

    public int Frames { get; private set; } = 6;
    public float FrameTime { get; private set;} = 0.1f;

    public AnimatedSpriteRenderer(GameObject parentObject, Texture2D spriteSheet) 
    {
        ParentObject = parentObject;
        this.SpriteSheet = spriteSheet;
    }

    public AnimatedSpriteRenderer() {}

    public override void Update()
    {
        if (!_isAnimationActive) 
        {
            return;
        }

        _frameTimeLeft -= Globals.DeltaTime;

        if (_frameTimeLeft <= 0) 
        {
            _frameTimeLeft += FrameTime;
            _frame = (_frame + 1) % Frames;
        }
    }

    public void Draw()
    {
        Globals.SpriteBatch?.Draw(SpriteSheet,
            new Rectangle(
                (int)ParentObject.Transform._pos.X,
                (int)ParentObject.Transform._pos.Y,
                (int)(SpriteSheet.Width * ParentObject.Transform._scl.X),
                (int)(SpriteSheet.Height * ParentObject.Transform._scl.Y)),
                _sourceRectangles[_frame],
                Color,
                MathHelper.ToRadians(ParentObject.Transform._rot.Z),
                Vector2.One,
                SpriteEffects.None,
                1);
    }

    public override void Initialize()
    {
        _frames = Frames;
        _frameTime = FrameTime;

        var frameWdith = SpriteSheet.Width / Frames;
        var frameHeight = SpriteSheet.Height;

        for (int i = 0; i < Frames; i++) 
        {
            _sourceRectangles.Add(new(i * frameWdith, 0, frameWdith, frameHeight));
        }
        Globals.Renderer.AnimatedSprites.Add(this);
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>AnimatedSpriteRenderer</type>");
        
        builder.Append("<active>" + Active +"</active>");

        builder.Append("<spriteSheet>" + SpriteSheet.Name + "</spriteSheet>");
        
        //TODO: Append relevant data here
        builder.Append("<frames>" + Frames + "</frames>");

        builder.Append("<frameTime>" + FrameTime + "</frameTime>");

        builder.Append("<color>");
        builder.Append("<r>" + Color.R + "</r>");
        builder.Append("<g>" + Color.G + "</g>");
        builder.Append("<b>" + Color.B + "</b>");
        builder.Append("<a>" + Color.A + "</a>");
        builder.Append("</color>");

        builder.Append("<isAnimationActive>" + _isAnimationActive + "</isAnimationActive>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        
        //TODO: Deserialize relevant data here
        LoadSpriteSheet(element.Element("spriteSheet").Value);
        SetFrames(int.Parse(element.Element("frames").Value));
        SetFrameTime(float.Parse(element.Element("frameTime").Value));

        XElement color = element.Element("color");
        Color = new Color(int.Parse(color.Element("r").Value),int.Parse(color.Element("g").Value),int.Parse(color.Element("b").Value),int.Parse(color.Element("a").Value));
        
        _isAnimationActive = element.Element("isAnimationActive")?.Value == "True";
        Globals.Renderer.AnimatedSprites.Add(this);
    }

    public override void RemoveComponent()
    {
        Globals.Renderer.AnimatedSprites.Remove(this);
        ParentObject.RemoveComponent(this);
        AssetManager.FreeSprite(SpriteSheet);
    }

    public void StartAnimation() 
    {
        _isAnimationActive = true;
    }

    public void StopAnimation() 
    {
        _isAnimationActive = false;
    }

    public void ResetAnimation() 
    {
        _frame = 0;
        _frameTimeLeft = FrameTime;
    }

    public void SetSpriteSheet(Texture2D spriteSheet) 
    {
        SpriteSheet = spriteSheet;
        _sourceRectangles.Clear();
        Initialize();
    }

    public void SetFrameTime(float frameTime) 
    {
        FrameTime = frameTime;
        _frameTime = frameTime;
        _frameTimeLeft = frameTime;
    }

    public void SetFrames(int frames) 
    {
        Frames = frames;
        _sourceRectangles.Clear();
        Initialize();
    }

    public void LoadSpriteSheet(string name) 
    {
        SpriteSheet = AssetManager.GetSprite(name);
        _sourceRectangles.Clear();
        Initialize();
    }

#if DEBUG
    public override void Inspect()
    {
        if (ImGui.CollapsingHeader("Animated Sprite Renderer"))
        {   
            System.Numerics.Vector4 temp = Color.ToVector4().ToNumerics();
            ImGui.Checkbox("Sprite active", ref Active);
            ImGui.SameLine();
            ImGui.Checkbox("Animation active", ref _isAnimationActive);
            if (ImGui.DragInt("Frames", ref _frames, 1, 2, 64))
            {
                SetFrames(_frames);
            }
            if (ImGui.DragFloat("Frame time", ref _frameTime, 0.1f, 0.1f, 10.0f))
            {
                SetFrameTime(_frameTime);
            }
            if (ImGui.ColorEdit4("Sprite Color", ref temp))
            {
                Color = new Color(temp);
            }
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }
    }
#endif
}
