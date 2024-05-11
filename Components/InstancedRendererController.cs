using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class InstancedRendererController : Component
{
    public ModelData ModelData;
    public List<InstanceData> WorldMatrices = new();

    public struct InstanceData
    {
        public Matrix World;
        private Matrix _normal;

        public InstanceData(Matrix world)
        {
            this.World = world;
            _normal = Matrix.Transpose(Matrix.Invert(World));
        }

        public void SetWorld(Matrix world)
        {
            this.World = world;
            _normal = Matrix.Transpose(Matrix.Invert(World));
        }
    }
    
    public void Draw()
    {
        if (Active && WorldMatrices.Count > 0)
        {
            var instanceVertexBuffer = new DynamicVertexBuffer(Globals.GraphicsDevice,
                Globals.InstanceVertexDeclaration, WorldMatrices.Count, BufferUsage.WriteOnly);
            
            instanceVertexBuffer.SetData(WorldMatrices.ToArray(),0,WorldMatrices.Count);
            
            ModelData.ApplyLod();
            
            if (!ModelData.IsMultiMesh)
            {
                ModelData.PassTextures(0);
            }
            
            for (int i = 0; i < ModelData.Models[ModelData.CurrentModelIndex].Meshes.Count; i++)
            {
                if (ModelData.IsMultiMesh)ModelData.PassTextures(i);
                foreach (ModelMeshPart part in ModelData.Models[ModelData.CurrentModelIndex].Meshes[i].MeshParts)
                {
                    Globals.GraphicsDevice.SetVertexBuffers(
                        new VertexBufferBinding(part.VertexBuffer, part.VertexOffset,0),
                        new VertexBufferBinding(instanceVertexBuffer,0,1)
                        );
                    Globals.GraphicsDevice.Indices = part.IndexBuffer;
                    for (int j = 0; j < Globals.MainEffect.CurrentTechnique.Passes.Count; j++)
                    {
                        Globals.MainEffect.CurrentTechnique.Passes[j].Apply();
                        Globals.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, part.StartIndex, part.PrimitiveCount, WorldMatrices.Count);
                    }
                }
            }
        }
    }

    public InstancedRendererController(GameObject parentObject)
    {
        ParentObject = parentObject;
        Initialize();
    }
    
    public InstancedRendererController(){}
    
    
    public override void Update()
    {
        WorldMatrices.Clear();
    }

    public override void Initialize()
    {
        Globals.Renderer.InstancedRendererControllers.Add(this);
        ModelData = AssetManager.DefaultModel;
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>InstancedRendererController</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<model>" + ModelData.Serialize() +"</model>");
        
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
            LoadModel(model?.Element("path")?.Value);
        }
    }

    public void LoadModel(string modelPath)
    {
        ModelData = AssetManager.GetModel(modelPath);
    }
    
    public override void RemoveComponent()
    {
        //If controller is removed, remove all units connected to it
        foreach (GameObject childObject in ParentObject.Children)
        {
            var unit = childObject.GetComponent<InstancedRendererUnit>();
            if (unit != null && unit.Controller == this)
            {
                childObject.RemoveComponent(unit);
            }
        }

        Globals.Renderer.InstancedRendererControllers.Remove(this);
        
        
        AssetManager.FreeModel(ModelData);
        ParentObject.RemoveComponent(this);
    }

    private void AddToAllChildren()
    {
        foreach (GameObject gameObject in ParentObject.Children)
        {
            if(gameObject.GetComponent<InstancedRendererUnit>() == null) gameObject.AddComponent<InstancedRendererUnit>();
        }
    }
    
#if DEBUG
    
    
    private bool _switchingModel = false;
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Instanced Renderer Controller"))
        {
            ImGui.Checkbox("Controller active", ref Active);
            ImGui.Text("Current mesh: " + ModelData.ModelPath);
            ImGui.Text("Current technique: " + ModelData.ShaderTechniqueName);
            ImGui.InputInt("Current model", ref ModelData.CurrentModelIndex);
            if (ImGui.Button("Add Unit to all children"))
            {
                AddToAllChildren();
            }
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
                        AssetManager.FreeModel(ModelData);
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