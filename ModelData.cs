using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class ModelData
{
    private readonly string[] _modelMaps = {"albedo","normal","roughness","metalness","ao" };

    public int CurrentModelIndex;
    public readonly List<Model> Models = new();
    public string ModelPath;
    public BoundingSphere BoundingSphere;
    public string ShaderTechniqueName;
    
    public bool IsMultiMesh;
    private bool _lodUsed;
    /*
     *--------------------------------------------------------------------------------------------------------------------------
     *Textures as stored like:
     * [0] - albedo
     * [1] - normal
     * [2] - roughness
     * [3] - metalness
     * [4] - ambient occlusion
     *--------------------------------------------------------------------------------------------------------------------------
     */

    public readonly List<List<List<Texture2D>>> Textures = new();

    public bool IsInView(Matrix world)
    {
        return Globals.BoundingFrustum.Contains(BoundingSphere.Transform(world)) != ContainmentType.Disjoint;
    }

    public void ApplyLod()
    {
        if (_lodUsed)
        {
            int levels = Textures.Count;
            float step = 60.0f / levels;
            for (int i = 0; i < levels; i++)
            {
                if (Globals.ZoomDegrees > 25.0f + (i * step) && Globals.ZoomDegrees < 25.0f + ((i + 1) * step))
                {
                    CurrentModelIndex = levels - i - 1;
                }
            }
        }
    }
    
    public void Draw(Matrix world)
    {
        if (Globals.MainEffect.CurrentTechnique.Name != ShaderTechniqueName)
            Globals.MainEffect.CurrentTechnique = Globals.MainEffect.Techniques[ShaderTechniqueName];
        
        ApplyLod();
        
        if (!IsMultiMesh) PassTextures(0);
        
        Globals.MainEffect.Parameters["World"]?.SetValue(world);
        Matrix temp = Matrix.Transpose(Matrix.Invert(world));
        temp.M41 = 0;
        temp.M42 = 0;
        temp.M43 = 0;
        temp.M44 = 1;
        temp.M14 = 0;
        temp.M24 = 0;
        temp.M34 = 0;
        Globals.MainEffect.Parameters["normalMatrix"]?.SetValue(temp);
        for (int j = 0; j < Models[CurrentModelIndex].Meshes.Count;j++)
        {
            if (IsMultiMesh)PassTextures(j);
            foreach (ModelMeshPart part in Models[CurrentModelIndex].Meshes[j].MeshParts)
            {
                if (part.PrimitiveCount > 0)
                {
                    Globals.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                    Globals.GraphicsDevice.Indices = part.IndexBuffer;
                    for (int i = 0; i < Globals.MainEffect.CurrentTechnique.Passes.Count; i++)
                    {
                        Globals.MainEffect.CurrentTechnique.Passes[i].Apply();
                        Globals.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }
        } 
    }

    public void PassTextures(int mesh)
    {
        Globals.MainEffect.Parameters["albedo"]?.SetValue(Textures[CurrentModelIndex][mesh][0]);
        Globals.MainEffect.Parameters["normal"]?.SetValue(Textures[CurrentModelIndex][mesh][1]);
        Globals.MainEffect.Parameters["roughness"]?.SetValue(Textures[CurrentModelIndex][mesh][2]);
        Globals.MainEffect.Parameters["metalness"]?.SetValue(Textures[CurrentModelIndex][mesh][3]);
        Globals.MainEffect.Parameters["ao"]?.SetValue(Textures[CurrentModelIndex][mesh][4]);
    }
    
    public ModelData(ContentManager manager, string modelPath)
    {
        LoadModel(manager,modelPath);
        CalculateBoundingSphere();
    }

    private void CalculateBoundingSphere()
    {
        List<Vector3> modelVertices = new List<Vector3>();
        foreach (var mesh in Models[0].Meshes)
        {
            foreach (ModelMeshPart meshPart in mesh.MeshParts)
            {
                var indices = new short[meshPart.IndexBuffer.IndexCount];
                    meshPart.IndexBuffer.GetData(indices);

                var vertices = new float[meshPart.VertexBuffer.VertexCount
                        * meshPart.VertexBuffer.VertexDeclaration.VertexStride / 4];
                meshPart.VertexBuffer.GetData(vertices);


                for (int i = meshPart.StartIndex; i < meshPart.StartIndex + meshPart.PrimitiveCount * 3; i++)
                { 
                    int index = (meshPart.VertexOffset + indices[i]) * meshPart.VertexBuffer.VertexDeclaration.VertexStride / 4;
                    modelVertices.Add(new Vector3(vertices[index], vertices[index + 1], vertices[index + 2]));
                }
            }
        }
        BoundingSphere = BoundingSphere.CreateFromPoints(modelVertices);
    }

    private void LoadModel(ContentManager manager, string modelPath)
    {
        //Loading model's textures. Textures must be in the same catalog as model and has to follow naming convention.
        XElement modelConfig = null;
        try
        {
            modelConfig = XDocument.Load(Globals.MainPath + "Content/" + modelPath + "/config.xml").Element("config");
        }
        catch (Exception)
        {
            //Console.WriteLine(e);
            //throw;
            Textures.Add(new List<List<Texture2D>>());
            Textures[0].Add(new List<Texture2D>());
            for (int i = 0;i < _modelMaps.Length;i++)
            {
                Texture2D temp;
                try
                {
                    temp = manager.Load<Texture2D>(modelPath + "/" + _modelMaps[i]);
                }
                catch (ContentLoadException)
                {
                    Textures[0][0].Add(AssetManager.DefaultTextureMaps[i]);
                    continue;
                }
                Textures[0][0].Add(temp);
            }
        
            //Loading model itself, saving model name
            
            try
            {
                string modelName = modelPath.Substring(modelPath.LastIndexOf('/') + 1);
                Models.Add(manager.Load<Model>(modelPath + "/" + modelName));

            } catch (Exception)
            {
                Models.Add(AssetManager.DefaultModel.Models[0]);
            }
            
        }

        IsMultiMesh = modelConfig?.Element("multimesh")?.Value == "True";
        _lodUsed = modelConfig?.Element("lod")?.Element("used")?.Value == "True";
        
        //if LOD is used for this model
        if (_lodUsed)
        {
            var value = modelConfig?.Element("lod")?.Element("levels")?.Value;
            if (value != null)
            {
                int lod = int.Parse(value);
                {
                    float step = 60.0f / lod;
                    for (int i = 0; i < lod; i++)
                    {
                        if (Globals.ZoomDegrees >= 25.0f + (i * step) && Globals.ZoomDegrees < 25.0f + ((i + 1) * step))
                        {
                            CurrentModelIndex = lod - i - 1;
                        }
                    }
                    string modelName = modelPath.Substring(modelPath.LastIndexOf('/') + 1);
                    //if model has multiple meshes with separate texture maps
                    if (IsMultiMesh)
                    {
                        int meshCount = int.Parse(modelConfig?.Element("meshes")?.Value);
                        for (int i = 0; i < lod; i++)
                        {
                            Textures.Add(new List<List<Texture2D>>());
                            for (int j = 0; j < meshCount; j++)
                            {
                                Textures[i].Add(new List<Texture2D>());
                                for (int k = 0; k < _modelMaps.Length; k++)
                                {
                                    Texture2D temp;
                                    try
                                    {
                                        temp = manager.Load<Texture2D>(modelPath + "/" + (i + 1) + "/" + (j + 1) + "/" +_modelMaps[k]);
                                    }
                                    catch (ContentLoadException)
                                    {
                                        Textures[i][j].Add(AssetManager.DefaultTextureMaps[k]);
                                        continue;
                                    }
                                    Textures[i][j].Add(temp);
                                }
                            }
                            try
                            {
                                Models.Add(manager.Load<Model>(modelPath + "/" + (i + 1) + "/" + modelName));
                            }
                            catch (Exception)
                            {
                                Models.Add(AssetManager.DefaultModel.Models[0]);
                                modelPath = AssetManager.DefaultModel.ModelPath;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < lod; i++)
                        {
                            Textures.Add(new List<List<Texture2D>>());
                            Textures[i].Add(new List<Texture2D>());
                            for (int j = 0;j < _modelMaps.Length;j++)
                            {
                                Texture2D temp;
                                try
                                {
                                    temp = manager.Load<Texture2D>(modelPath + "/"+ (i + 1) + "/" + _modelMaps[j]);
                                }
                                catch (ContentLoadException)
                                {
                                    Textures[i][0].Add(AssetManager.DefaultTextureMaps[j]);
                                    continue;
                                }
                                Textures[i][0].Add(temp);
                            }
                            try
                            {
                                Models.Add(manager.Load<Model>(modelPath + "/" + (i + 1) + "/" + modelName));
                            }
                            catch (Exception)
                            {
                                Models.Add(AssetManager.DefaultModel.Models[0]);
                                modelPath = AssetManager.DefaultModel.ModelPath;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            //if model has multiple meshes with separate texture maps
            if (IsMultiMesh)
            {
                
                var value = modelConfig?.Element("meshes")?.Value;
                if (value != null)
                {
                    int meshCount = int.Parse(value);
                    Textures.Add(new List<List<Texture2D>>());
                    for (int i = 0; i < meshCount; i++)
                    {
                        Textures[0].Add(new List<Texture2D>());
                        for (int j = 0; j < _modelMaps.Length; j++)
                        {
                            Texture2D temp;
                            try
                            {
                                temp = manager.Load<Texture2D>(modelPath + "/" + (i + 1) + "/" + _modelMaps[j]);
                            }
                            catch (ContentLoadException)
                            {
                                Textures[0][i].Add(AssetManager.DefaultTextureMaps[j]);
                                continue;
                            }
                            Textures[0][i].Add(temp);
                        }
                    }
                    //Loading model itself, saving model name
                    string modelName = modelPath.Substring(modelPath.LastIndexOf('/') + 1);
                    try
                    {
                        Models.Add(manager.Load<Model>(modelPath + "/" + modelName));
                    }
                    catch (Exception)
                    {
                        Models.Add(AssetManager.DefaultModel.Models[0]);
                        modelPath = AssetManager.DefaultModel.ModelPath;
                    }
                    
                }
                
            }
            else
            {
                Textures.Add(new List<List<Texture2D>>());
                Textures[0].Add(new List<Texture2D>());
                for (int i = 0;i < _modelMaps.Length;i++)
                {
                    Texture2D temp;
                    try
                    {
                        temp = manager.Load<Texture2D>(modelPath + "/" + _modelMaps[i]);
                    }
                    catch (ContentLoadException)
                    {
                        Textures[0][0].Add(AssetManager.DefaultTextureMaps[i]);
                        continue;
                    }
                    Textures[0][0].Add(temp);
                }
        
                //Loading model itself, saving model name
                string modelName = modelPath.Substring(modelPath.LastIndexOf('/') + 1);
                try
                {
                    Models.Add(manager.Load<Model>(modelPath + "/" + modelName));
                }
                catch (Exception)
                {
                    Models.Add(AssetManager.DefaultModel.Models[0]);
                    modelPath = AssetManager.DefaultModel.ModelPath;
                }
                
            }
        }
        
        ModelPath = modelPath;
        ShaderTechniqueName = "PBR";
    }
    
    public string Serialize()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<path>"+ ModelPath +"</path>");
        builder.Append("<technique>"+ ShaderTechniqueName +"</technique>");
        
        return builder.ToString();
    }

}