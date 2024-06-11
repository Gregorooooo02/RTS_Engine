using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Animation;
using Animation.Controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class AnimatedModelData
{
    private readonly string[] _modelMaps = { "albedo", "normal", "roughness", "metalness", "ao" };

    public int CurrentModelIndex;
    public readonly List<SkinnedModel> SkinnedModels = new();
    public AnimationController AnimationController;
    public int ActiveAnimationClip;
    
    public string ModelPath;
    public BoundingSphere BoundingSphere;
    public string ShaderTechniqueName;
    public List<float> _lodThresholds = new();

    public bool ChangedClip;
    public bool IsMultiMesh;
    private bool _lodUsed;
    
    public readonly List<List<List<Texture2D>>> Textures = new();

    public bool IsInView(Matrix world)
    {
        BoundingSphere temp = BoundingSphere.Transform(world);
        temp.Radius *= 1.5f;
        
        return Globals.BoundingFrustum.Contains(temp) != ContainmentType.Disjoint;
    }
    
    public void ApplyLod()
    {
        if (_lodUsed)
        {
            int levels = Textures.Count;
            //Check first threshold
            if (Globals.ZoomDegrees < _lodThresholds[0])
            {
                CurrentModelIndex = 0;
                return;
            }

            //Check thresholds 2nd to (levels - 1)th
            for (int i = 0; i < _lodThresholds.Count - 1; i++)
            {
                if (Globals.ZoomDegrees >= _lodThresholds[i] && Globals.ZoomDegrees < _lodThresholds[i + 1])
                {
                    CurrentModelIndex = i + 1;
                    return;
                }
            }
            //Check last threshold
            if (Globals.ZoomDegrees >= _lodThresholds[levels - 2])
            {
                CurrentModelIndex = levels - 1;
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
        Globals.MainEffect.Parameters["normalMatrix"]?.SetValue(temp);
        Globals.MainEffect.Parameters["BoneTransforms"]?.SetValue(AnimationController.SkinnedBoneTransforms);
        
        for (int i = 0; i < SkinnedModels[CurrentModelIndex].Model.Meshes.Count; i++)
        {
            if (IsMultiMesh) PassTextures(i);
            
            for (int j = 0; j < SkinnedModels[CurrentModelIndex].Model.Meshes[i].MeshParts.Count; j++)
            {
                var part = SkinnedModels[CurrentModelIndex].Model.Meshes[i].MeshParts[j];
                if (part.PrimitiveCount > 0)
                {
                    Globals.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                    Globals.GraphicsDevice.Indices = part.IndexBuffer;
                    
                    for (int k = 0; k < Globals.MainEffect.CurrentTechnique.Passes.Count; k++)
                    {
                        Globals.MainEffect.CurrentTechnique.Passes[k].Apply();
                        Globals.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }
        }
    }

    public void UpdateClip()
    {
        if (ChangedClip)
        {
            AnimationController.CrossFade(SkinnedModels[CurrentModelIndex].AnimationClips.Values[ActiveAnimationClip], TimeSpan.FromSeconds(0.5f));
            ChangedClip = false;
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

    public AnimatedModelData(ContentManager manager, string modelPath)
    {
        LoadModel(manager, modelPath);
        CalculateBoundingSphere();
        
        AnimationController = new AnimationController(SkinnedModels[CurrentModelIndex].SkeletonBones);
        AnimationController.Speed = 0.5f;
        
        AnimationController.TranslationInterpolation = InterpolationMode.Linear;
        AnimationController.OrientationInterpolation = InterpolationMode.Linear;
        AnimationController.ScaleInterpolation = InterpolationMode.Linear;
        
        ActiveAnimationClip = 1;
        AnimationController.StartClip(SkinnedModels[CurrentModelIndex].AnimationClips.Values[ActiveAnimationClip]);
    }

    private void CalculateBoundingSphere()
    {
        List<Vector3> modelVertices = new List<Vector3>();
        foreach (var mesh in SkinnedModels[0].Model.Meshes)
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
                    int index = (meshPart.VertexOffset + indices[i]) *
                        meshPart.VertexBuffer.VertexDeclaration.VertexStride / 4;
                    
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
                SkinnedModels.Add(manager.Load<SkinnedModel>(modelPath + "/" + modelName));
                ModelPath = modelPath;
                ShaderTechniqueName = "PBR_Skinned";

            } catch (Exception)
            {
                SkinnedModels.Add(AssetManager.DefaultAnimatedModel.SkinnedModels[0]);
            }
            return;
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
                for (int i = 0; i < lod; i++)
                {
                    XElement element = modelConfig?.Element("lod")?.Element("thresholds")?.Element("t"+i.ToString());
                    if (element != null)
                    {
                        float threshold = float.Parse(element.Value);
                        _lodThresholds.Add(threshold);
                    }
                }
                ApplyLod();
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
                            SkinnedModels.Add(manager.Load<SkinnedModel>(modelPath + "/" + (i + 1) + "/" + modelName));
                        }
                        catch (Exception)
                        {
                            SkinnedModels.Add(AssetManager.DefaultAnimatedModel.SkinnedModels[0]);
                            modelPath = AssetManager.DefaultAnimatedModel.ModelPath;
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
                            SkinnedModels.Add(manager.Load<SkinnedModel>(modelPath + "/" + (i + 1) + "/" + modelName));
                        }
                        catch (Exception)
                        {
                            SkinnedModels.Add(AssetManager.DefaultAnimatedModel.SkinnedModels[0]);
                            modelPath = AssetManager.DefaultAnimatedModel.ModelPath;
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
                        SkinnedModels.Add(manager.Load<SkinnedModel>(modelPath + "/" + modelName));
                    }
                    catch (Exception)
                    {
                        SkinnedModels.Add(AssetManager.DefaultAnimatedModel.SkinnedModels[0]);
                        modelPath = AssetManager.DefaultAnimatedModel.ModelPath;
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
                    SkinnedModels.Add(manager.Load<SkinnedModel>(modelPath + "/" + modelName));
                }
                catch (Exception)
                {
                    SkinnedModels.Add(AssetManager.DefaultAnimatedModel.SkinnedModels[0]);
                    modelPath = AssetManager.DefaultAnimatedModel.ModelPath;
                }
                
            }
        }
        
        ModelPath = modelPath;
        ShaderTechniqueName = "PBR_Skinned";
    }

    public string Serialize()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<path>" + ModelPath + "</path>");
        builder.Append("<technique>" + ShaderTechniqueName + "</technique>");
        
        return builder.ToString();
    }
}