using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class ModelData
{
    private readonly string[] _modelMaps = {"albedo","normal","roughness","metalness","ao" };
    
    public Model Model;
    public string ModelPath;
    public BoundingSphere BoundingSphere;
    
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
    public List<Texture2D> Textures = new List<Texture2D>();
    
    
    
    public ModelData(ContentManager manager, string modelPath)
    {
        LoadModel(manager,modelPath);
        BoundingSphere = CalculateBoundingSphere();
    }

    private BoundingSphere CalculateBoundingSphere()
    {
        List<Vector3> modelVertices = new List<Vector3>();
        foreach (var mesh in Model.Meshes)
        {
            foreach (ModelMeshPart meshPart in mesh.MeshParts)
            {
                var indices = new short[meshPart.IndexBuffer.IndexCount];
                meshPart.IndexBuffer.GetData<short>(indices);

                var vertices = new float[meshPart.VertexBuffer.VertexCount
                    * meshPart.VertexBuffer.VertexDeclaration.VertexStride / 4];
                meshPart.VertexBuffer.GetData<float>(vertices);


                for (int i = meshPart.StartIndex; i < meshPart.StartIndex + meshPart.PrimitiveCount * 3; i++)
                {
                    int index = (meshPart.VertexOffset + indices[i]) *
                        meshPart.VertexBuffer.VertexDeclaration.VertexStride / 4;

                    modelVertices.Add(new Vector3(vertices[index], vertices[index + 1], vertices[index + 2]));
                }
            }
        }
        return BoundingSphere.CreateFromPoints(modelVertices);
    }

    private void LoadModel(ContentManager manager, string modelPath)
    {
        //Loading model's textures. Textures must be in the same catalog as model and has to follow naming convention.
        for (int i = 0;i < _modelMaps.Length;i++)
        {
            Texture2D temp;
            try
            {
                temp = manager.Load<Texture2D>(modelPath + "/" + _modelMaps[i]);
            }
            catch (ContentLoadException)
            {
                Textures.Add(AssetManager.DefaultTextureMaps[i]);
                continue;
            }
            Textures.Add(temp);
        }

        //Loading model itself, saving model name
        string modelName = modelPath.Substring(modelPath.LastIndexOf('/') + 1);
        ModelPath = modelPath;
        Model = manager.Load<Model>(modelPath + "/" + modelName);
    }

    public ModelData(ContentManager manager, string modelPath, Vector3 boundingPosition, float boundingRadius)
    {
        LoadModel(manager,modelPath);
        BoundingSphere.Center = boundingPosition;
        BoundingSphere.Radius = boundingRadius;
    }

}