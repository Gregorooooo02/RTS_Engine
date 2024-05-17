using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTS.Animation;
using SharpFont;

namespace RTS_Engine;

public class AnimatedMeshRenderer : Component
{
    public Model _model { get; private set; }
    public Animations _animations { get; private set; }
    public bool IsVisible { get; private set; } = false;
    
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
            IsVisible = true;
            Globals.Renderer.AnimatedMeshes.Add(this);
        }
        else
        {
            IsVisible = false;
        }
        _animations.Update(Globals.ElapsedGameTime, true, ParentObject.Transform.ModelMatrix);
    }

    public override void Initialize()
    {
        _model = AssetManager.DefaultAnimatedModel;
        _animations = _model.GetAnimations();
        var clip = _animations.Clips["Take 001"];
        _animations.SetClip(clip);
    }
    
    public void Draw(Matrix world)
    {
        if(!Active) return;
        Matrix[] transforms = new Matrix[_model.Bones.Count];
        _model.CopyAbsoluteBoneTransformsTo(transforms);
        
        foreach (ModelMesh mesh in _model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                ((BasicEffect)part.Effect).SpecularColor = Vector3.Zero;
                ConfigureEffectMatrices((IEffectMatrices)part.Effect, world, Globals.View, Globals.Projection);
                ConfigureEffectLighting((IEffectLights)part.Effect);
                part.UpdateVertices(_animations.AnimationTransforms);
            }
            mesh.Draw();
        }
    }
    
    private void ConfigureEffectMatrices(IEffectMatrices effect, Matrix world, Matrix view, Matrix projection)
    {
        effect.World = world;
        effect.View = view;
        effect.Projection = projection;
    }

    private void ConfigureEffectLighting(IEffectLights effect)
    {
        effect.EnableDefaultLighting();
        effect.DirectionalLight0.Direction = Vector3.Backward;
        effect.DirectionalLight0.Enabled = true;
        effect.DirectionalLight1.Enabled = false;
        effect.DirectionalLight2.Enabled = false;
    }
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>AnimatedMeshRenderer</type>");
        
        builder.Append("<active>" + Active + "</active>");

        builder.Append("<model>" + _model.ToString() + "</model>");
        
        builder.Append("</component>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        XElement model = element.Element("model");
        string path;

        if (model?.Element("path") == null)
        {
            LoadModel(model?.Value);
        }
        else
        {
            LoadModel(model?.Element("path")?.Value, model?.Element("technique")?.Value);
        }
    }

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }

    public void LoadModel(string modelPath, string technique = "PBR")
    {
        _model = Globals.content.Load<Model>("snowman_animated");
        // _model.ShaderTechniqueName = technique;
    }

    public Model GetModel()
    {
        // return _model.Model;
        return _model;
    }
    
#if DEBUG
    private bool _switchingModel = false;
    public override void Inspect()
    {
        if (ImGui.CollapsingHeader("Animated Mesh Renderer"))
        {
            ImGui.Checkbox("Animated Mesh Active", ref Active);
            // ImGui.Text("Current anim. mesh: " + _model.ModelPath);
            // ImGui.Text("Current technique: " + _model.ShaderTechniqueName);
            if (ImGui.Button("Switch anim. mesh"))
            {
                _switchingModel = true;
            }

            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }

            if (_switchingModel)
            {
                ImGui.Begin("Switching models");
                foreach (string n in AssetManager.ModelPaths)
                {
                    if (ImGui.Button(n))
                    {
                        // AssetManager.FreeModel(_model);
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