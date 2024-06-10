using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Animation;
using Animation.Controllers;
using Assimp;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace RTS_Engine;

public class AnimatedMeshRenderer : Component
{
    public SkinnedModel _skinnedModel {get; private set;}
    private AnimationController _animationController {get; set;}
    public int _activeAnimationClip {get; private set;}
    public bool IsVisible { get; private set; } = true;
    
    public AnimatedMeshRenderer(GameObject parentObject)
    {
        ParentObject = parentObject;
        Initialize();
    }

    public AnimatedMeshRenderer() {}

    public override void Update()
    {
        Globals.Renderer.AnimatedMeshes.Add(this);
        _animationController.Update(Globals.ElapsedGameTime, ParentObject.Transform.ModelMatrix);
    }

    public void Draw(Matrix world)
    {
        if (!Active) return;

        foreach (ModelMesh mesh in _skinnedModel.Model.Meshes)
        {
            foreach (SkinnedEffect effect in mesh.Effects)
            {
                effect.SetBoneTransforms(_animationController.SkinnedBoneTransforms);
                effect.World = world;
                effect.View = Globals.View;
                effect.Projection = Globals.Projection;
            }
            mesh.Draw();
        }
    }

    public override void Initialize()
    {
        _skinnedModel = AssetManager.DefaultAnimatedModel;
        
        foreach (ModelMesh mesh in _skinnedModel.Model.Meshes)
        {
            foreach (SkinnedEffect effect in mesh.Effects)
            {
                effect.EnableDefaultLighting();
                effect.PreferPerPixelLighting = true;
                effect.SpecularColor = new Vector3(0.25f);
                effect.SpecularPower = 16;
            }
        }
        
        _animationController = new AnimationController(_skinnedModel.SkeletonBones);
        _animationController.Speed = 0.5f;
        
        _animationController.TranslationInterpolation = InterpolationMode.Linear;
        _animationController.OrientationInterpolation = InterpolationMode.Linear;
        _animationController.ScaleInterpolation = InterpolationMode.Linear;
        
        _activeAnimationClip = 0;
        _animationController.StartClip(_skinnedModel.AnimationClips.Values[_activeAnimationClip]);
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append("<component>");
        
        builder.Append("<type>AnimatedMeshRenderer</type>");
        
        builder.Append("<active>" + Active + "</active>");
        
        builder.Append("</component>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        
    }

    public override void RemoveComponent()
    {
        
    }

#if DEBUG
    private bool _switchingModel = false;
    public override void Inspect()
    {
        if (ImGui.CollapsingHeader("Animated Mesh Renderer"))
        {
            ImGui.Checkbox("Animated Mesh Active", ref Active);
            ImGui.Text("Current animated mesh: " + _skinnedModel);

            var animationControllerSpeed = _animationController.Speed;
            ImGui.DragFloat("Animation speed", ref animationControllerSpeed, 0.01f, 0.01f, 10f);
            _animationController.Speed = animationControllerSpeed;

            var activeAnimationClip = _activeAnimationClip;
            ImGui.SliderInt("Active animation clip", ref activeAnimationClip, 0, _skinnedModel.AnimationClips.Count - 1);
            _activeAnimationClip = activeAnimationClip;
            
            ImGui.Text("Translation interpolation: " + _animationController.TranslationInterpolation);
            if (ImGui.Button("None"))
            {
                _animationController.TranslationInterpolation = InterpolationMode.None;
            }
            ImGui.SameLine();
            if (ImGui.Button("Linear"))
            {
                _animationController.TranslationInterpolation = InterpolationMode.Linear;
            }
            ImGui.SameLine();
            if (ImGui.Button("Cubic"))
            {
                _animationController.TranslationInterpolation = InterpolationMode.Cubic;
            }
            
            ImGui.Text("Orientation interpolation: " + _animationController.OrientationInterpolation);
            if (ImGui.Button("None"))
            {
                _animationController.OrientationInterpolation = InterpolationMode.None;
            }
            ImGui.SameLine();
            if (ImGui.Button("Linear"))
            {
                _animationController.OrientationInterpolation = InterpolationMode.Linear;
            }
            ImGui.SameLine();
            if (ImGui.Button("Cubic"))
            {
                _animationController.OrientationInterpolation = InterpolationMode.Cubic;
            }
            
            ImGui.Text("Scale interpolation: " + _animationController.ScaleInterpolation);
            if (ImGui.Button("None"))
            {
                _animationController.ScaleInterpolation = InterpolationMode.None;
            }
            ImGui.SameLine();
            if (ImGui.Button("Linear"))
            {
                _animationController.ScaleInterpolation = InterpolationMode.Linear;
            }
            ImGui.SameLine();
            if (ImGui.Button("Cubic"))
            {
                _animationController.ScaleInterpolation = InterpolationMode.Cubic;
            }

            var animationControllerLoopEnabled = _animationController.LoopEnabled;
            ImGui.Checkbox("Loop enabled", ref animationControllerLoopEnabled);
            _animationController.LoopEnabled = animationControllerLoopEnabled;
            
            var animationControllerCrossfade = _animationController.CrossFading;
            ImGui.Checkbox("Crossfade", ref animationControllerCrossfade);
            _animationController.CrossFading = animationControllerCrossfade;

            if (animationControllerCrossfade)
            {
                
            }
            
            if (ImGui.Button("Switch animated mesh"))
            {
                _switchingModel = true;
            }

            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }

            if (_switchingModel)
            {
                ImGui.Begin("Switching animated models");
                foreach (string n in AssetManager.ModelPaths)
                {
                    if (ImGui.Button(n))
                    {
                        
                    }
                }

                if (ImGui.Button("Cancel selection"))
                {
                    _switchingModel = false;
                }

                ImGui.End();
            }
        }
    }
#endif
}