using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Animation;
using Animation.Controllers;

namespace RTS_Engine;

public class AnimatedMeshRenderer : Component
{
    public AnimatedModelData _skinnedModel {get; private set;}
    public bool IsVisible { get; private set; } = true;

    public bool AdditionalVisibility = true;
    
    //TODO: Add serialization, deserialization and Inspect checkbox for boolean below
    public bool ApplyFog = true;
    
    public AnimatedMeshRenderer(GameObject parentObject)
    {
        ParentObject = parentObject;
        Initialize();
    }
    
    public AnimatedMeshRenderer() {}

    public override void Update()
    {
        if (Active)
        {
            if (_skinnedModel.IsInView(ParentObject.Transform.ModelMatrix) && AdditionalVisibility)
            {
                IsVisible = true;
                Globals.Renderer.AnimatedMeshes.Add(this);
            }
            else
            {
                IsVisible = false;
            }
            if (Globals.IsPaused) return;
            _skinnedModel.UpdateClip();
            _skinnedModel.AnimationController.Update(Globals.ElapsedGameTime, ParentObject.Transform.ModelMatrix);
            _skinnedModel.AnimationController.SkinnedBoneTransforms.CopyTo(_skinnedModel.BoneTransforms, 0);
        }
    }

    public void Draw(Matrix world)
    {
        if (!Active) return;
    }

    public override void Initialize()
    {
        _skinnedModel = AssetManager.DefaultAnimatedModel;
        // Copy the bone transforms from the animation controller to the skinned model
        _skinnedModel.BoneTransforms = new Matrix[_skinnedModel.AnimationController.SkinnedBoneTransforms.Length];
        _skinnedModel.AnimationController.SkinnedBoneTransforms.CopyTo(_skinnedModel.BoneTransforms, 0);
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append("<component>");
        
        builder.Append("<type>AnimatedMeshRenderer</type>");
        
        builder.Append("<active>" + Active + "</active>");
        
        builder.Append("<applyFog>" + ApplyFog +"</applyFog>");

        builder.Append("<animatedModel>" + _skinnedModel.Serialize() + "</animatedModel>");
        
        builder.Append("</component>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        ApplyFog = !bool.TryParse(element.Element("applyFog")?.Value, out bool fog) || fog;
        XElement model = element.Element("animatedModel");
        if (model?.Element("path") == null)
        {
            LoadModel(model?.Value);
        }
        else
        {
            LoadModel(model?.Element("path")?.Value, model?.Element("technique")?.Value);
        }
        
        _skinnedModel.Deserialize(element.Element("animatedModel"));
    }

    public override void RemoveComponent()
    {
        Pickable pickable = ParentObject.GetComponent<Pickable>();
        if (pickable != null)
        {
            ParentObject.RemoveComponent(pickable);
        }
        
        AssetManager.FreeAnimatedModel(_skinnedModel);
        ParentObject.RemoveComponent(this);
    }

    public void LoadModel(string modelPath, string technique = "PBR_Skinned")
    {
        _skinnedModel = new AnimatedModelData(AssetManager.GetAnimatedModel(modelPath));
        // Copy the bone transforms from the animation controller to the skinned model
        _skinnedModel.BoneTransforms = new Matrix[_skinnedModel.AnimationController.SkinnedBoneTransforms.Length];
        _skinnedModel.AnimationController.SkinnedBoneTransforms.CopyTo(_skinnedModel.BoneTransforms, 0);
        
        _skinnedModel.ShaderTechniqueName = technique;
    }

    public SkinnedModel GetModel()
    {
        return _skinnedModel.SkinnedModels[_skinnedModel.CurrentModelIndex];
    }

#if DEBUG
    private bool _switchingModel = false;
    public override void Inspect()
    {
        if (ImGui.CollapsingHeader("Animated Mesh Renderer"))
        {
            ImGui.Checkbox("Animated Mesh Active", ref Active);
            ImGui.Checkbox("Apply fog", ref ApplyFog);
            ImGui.Text("Additional visibility: " + AdditionalVisibility);
            ImGui.Text("Current animated mesh: " + _skinnedModel.ModelPath);

            var animationControllerSpeed = _skinnedModel.AnimationController.Speed;
            ImGui.DragFloat("Animation speed", ref animationControllerSpeed, 0.01f, 0.01f, 10f);
            _skinnedModel.AnimationController.Speed = animationControllerSpeed;

            var activeAnimationClip = _skinnedModel.ActiveAnimationClip;
            if (ImGui.SliderInt("Active animation clip", ref activeAnimationClip, 0,
                    _skinnedModel.SkinnedModels[_skinnedModel.CurrentModelIndex].AnimationClips.Count - 1))
                _skinnedModel.ChangedClip = true;
            
            _skinnedModel.ActiveAnimationClip = activeAnimationClip;

            if (ImGui.CollapsingHeader("Translation interpolation: " +
                                       _skinnedModel.AnimationController.TranslationInterpolation))
            {
                if (ImGui.Button("None"))
                {
                    _skinnedModel.AnimationController.TranslationInterpolation = InterpolationMode.None;
                }
                ImGui.SameLine();
                if (ImGui.Button("Linear"))
                {
                    _skinnedModel.AnimationController.TranslationInterpolation = InterpolationMode.Linear;
                }
            }

            if (ImGui.CollapsingHeader("Orientation interpolation: " + 
                                       _skinnedModel.AnimationController.OrientationInterpolation))
            {
                if (ImGui.Button("None"))
                {
                    _skinnedModel.AnimationController.OrientationInterpolation = InterpolationMode.None;
                }
                ImGui.SameLine();
                if (ImGui.Button("Linear"))
                {
                    _skinnedModel.AnimationController.OrientationInterpolation = InterpolationMode.Linear;
                }
            }

            if (ImGui.CollapsingHeader("Scale interpolation: " + 
                                       _skinnedModel.AnimationController.ScaleInterpolation))
            {
                if (ImGui.Button("None"))
                {
                    _skinnedModel.AnimationController.ScaleInterpolation = InterpolationMode.None;
                }
                ImGui.SameLine();
                if (ImGui.Button("Linear"))
                {
                    _skinnedModel.AnimationController.ScaleInterpolation = InterpolationMode.Linear;
                }
            }

            var animationControllerLoopEnabled = _skinnedModel.AnimationController.LoopEnabled;
            ImGui.Checkbox("Loop enabled", ref animationControllerLoopEnabled);
            _skinnedModel.AnimationController.LoopEnabled = animationControllerLoopEnabled;
            
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
                        AssetManager.FreeAnimatedModel(_skinnedModel);
                        LoadModel(n);
                        _switchingModel = false;
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