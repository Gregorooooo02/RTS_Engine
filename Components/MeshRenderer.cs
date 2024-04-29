using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ImGuiNET;

namespace RTS_Engine;

public class MeshRenderer : Component
{
    private ModelData _model;
    
    public MeshRenderer(GameObject parentObject)
    {
        ParentObject = parentObject;
        Initialize();
    }
    
    public MeshRenderer()
    {
        
    }
    
    public override void Update(){}

    public override void Initialize()
    {
        _model = AssetManager.DefaultModel;
    }
    
    public override void Draw()
    {
        if(!Active) return;
        _model.Draw(ParentObject.Transform.ModelMatrix);
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
        return _model.Model;
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
            if (ImGui.Button("Switch mesh"))
            {
                _switchingModel = true;
            }
            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
                AssetManager.FreeModel(_model);
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