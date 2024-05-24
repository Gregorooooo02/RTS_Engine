using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;

namespace RTS_Engine;

public class MeshRenderer : Component
{
    public ModelData _model {get; private set;}
    public bool IsVisible { get; private set; } = false;

    public MeshRenderer(GameObject parentObject)
    {
        ParentObject = parentObject;
        Initialize();
    }
    
    public MeshRenderer()
    {
        
    }

    public override void Update()
    {
        //Check if MeshRenderer is active
        if (Active)
        {
            //Check if model is in view, if yes add to render list, if not skip
            if (_model.IsInView(ParentObject.Transform.ModelMatrix))
            {
                IsVisible = true;
                Globals.Renderer.Meshes.Add(this);
            }
            else
            {
                IsVisible = false;
            }
        }
    }

    public override void Initialize()
    {
        _model = AssetManager.DefaultModel;
    }
    
    public void Draw()
    {
        if(!Active) return;
        //_model.Draw(ParentObject.Transform.ModelMatrix);
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>MeshRenderer</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<model>" + _model.Serialize() +"</model>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        XElement model = element.Element("model");
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
        //Remove linked components that can't/shouldn't exist without this one
        Pickable pickable = ParentObject.GetComponent<Pickable>();
        if (pickable != null && pickable.Renderer == this)
        {
            ParentObject.RemoveComponent(pickable);
        }
        
        AssetManager.FreeModel(_model);
        ParentObject.RemoveComponent(this);
    }


    public void LoadModel(string modelPath, string technique = "PBR")
    {
        _model = AssetManager.GetModel(modelPath);
        _model.ShaderTechniqueName = technique;
        //foreach (VertexElement element in _model.Meshes[0].MeshParts[0].VertexBuffer.VertexDeclaration.GetVertexElements())
        //{
        //    Console.WriteLine(element.VertexElementUsage);
        //}
    }

    public Model GetModel()
    {
        return _model.Models[_model.CurrentModelIndex];
    }
    
#if DEBUG
    private bool _switchingModel = false;
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Mesh Renderer"))
        {
            ImGui.Checkbox("Mesh active", ref Active);
            ImGui.Text("Current mesh: " + _model.ModelPath);
            ImGui.Text("Current technique: " + _model.ShaderTechniqueName);
            ImGui.InputInt("Current model", ref _model.CurrentModelIndex);
            if (ImGui.Button("Switch mesh"))
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
                        AssetManager.FreeModel(_model);
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