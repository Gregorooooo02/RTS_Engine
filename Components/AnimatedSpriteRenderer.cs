using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class AnimatedSpriteRenderer : Component
{
    public Texture2D spriteSheet;
    public Color Color = Color.White;
    private List<Rectangle> _sourceRectangles = new();
    private int _frames;
    private int _frame;
    private float _frameTime;
    private float _frameTimeLeft;
    private bool _isAnimationActive = true;

    public AnimatedSpriteRenderer(GameObject parentObject, Texture2D spriteSheet) 
    {
        ParentObject = parentObject;
        this.spriteSheet = spriteSheet;
    }

    public AnimatedSpriteRenderer() {}

    public override void Update()
    {
        if (!_isAnimationActive) 
        {
            return;
        }

        _frameTimeLeft -= Globals.TotalSeconds;

        if (_frameTimeLeft <= 0) 
        {
            _frameTimeLeft += _frameTime;
            _frame = (_frame + 1) % _frames;
        }
    }

    public override void Draw(Matrix _view, Matrix _projection)
    {
        Globals.Instance.SpriteBatch?.Draw(spriteSheet,
            new Rectangle(
                (int)ParentObject.Transform._pos.X,
                (int)ParentObject.Transform._pos.Y,
                (int)(spriteSheet.Width * ParentObject.Transform._scl.X),
                (int)(spriteSheet.Height * ParentObject.Transform._scl.Y)),
                _sourceRectangles[_frame],
                Color,
                MathHelper.ToRadians(ParentObject.Transform._rot.Z),
                Vector2.One,
                SpriteEffects.None,
                1);
    }

    public override void Initialize()
    {
        spriteSheet = AssetManager.DefaultAnimatedSprite;
        _frameTime = 0.1f;
        _frameTimeLeft = _frameTime;
        _frames = 6;

        var frameWdith = spriteSheet.Width / _frames;
        var frameHeight = spriteSheet.Height;

        for (int i = 0; i < _frames; i++) 
        {
            _sourceRectangles.Add(new(i * frameWdith, 0, frameWdith, frameHeight));
        }
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
        _frameTimeLeft = _frameTime;
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
            if (ImGui.DragFloat("Frame time", ref _frameTime, 0.1f, 0.1f, 10.0f))
            {
                _frameTimeLeft = _frameTime;
            }
            if (ImGui.ColorEdit4("Sprite Color", ref temp))
            {
                Color = new Color(temp);
            }
            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
                AssetManager.FreeSprite(spriteSheet);
            }
        }
    }
#endif
}
