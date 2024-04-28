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

    //TODO: This method is just copy-pasted from somewhere else. May require some tweaking.
    private void DrawModel(Model model, Matrix wrld, Matrix vw, Matrix proj)
    {
        foreach (ModelMesh mesh in _model.Model.Meshes)
        {
                 foreach (ModelMeshPart part in mesh.MeshParts)
                 {
                     if (part.PrimitiveCount > 0)
                     {
                         Globals.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                         Globals.GraphicsDevice.Indices = part.IndexBuffer;
                         
                         //Here pass parameters that are used in all techniques
                         Globals.TestEffect.Parameters["World"].SetValue(wrld);
                         Matrix temp = Matrix.Transpose(Matrix.Invert(wrld));
                         temp.M41 = 0;
                         temp.M42 = 0;
                         temp.M43 = 0;
                         temp.M44 = 1;
                         temp.M14 = 0;
                         temp.M24 = 0;
                         temp.M34 = 0;
                         Globals.TestEffect.Parameters["normalMatrix"].SetValue(temp);
                         Globals.TestEffect.Parameters["albedo"].SetValue(_model.Textures[0]);
                         Globals.TestEffect.Parameters["normal"]?.SetValue(_model.Textures[1]);
                         Globals.TestEffect.Parameters["roughness"]?.SetValue(_model.Textures[2]);
                         Globals.TestEffect.Parameters["metalness"]?.SetValue(_model.Textures[3]);
                         Globals.TestEffect.Parameters["ao"].SetValue(_model.Textures[4]);
                         
                         for (int i = 0; i < Globals.TestEffect.CurrentTechnique.Passes.Count; i++)
                         {
                             Globals.TestEffect.CurrentTechnique.Passes[i].Apply();
                             Globals.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                         }
                     }
                 }
        } 
    }

    public override void Draw(Matrix _view, Matrix _projection)
    {
        if(!Active) return;
        //TODO: Implement globally accessible View and Projection matrices. Then use them here
        DrawModel(_model.Model, ParentObject.Transform.ModelMatrix, _view, _projection);
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>MeshRenderer</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<model>" + _model.ModelPath +"</model>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        LoadModel(element.Element("model").Value);
    }


    public void LoadModel(string modelPath)
    {
        _model = AssetManager.GetModel(modelPath);
        
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
            ImGui.Text(_model.ModelPath);
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