using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class ModelData
{
    private readonly string[] _modelMaps = {"albedo","normal","roughness","metalness","ao" };
    
    public Model Model;
    public string ModelPath;
    private BoundingSphere _boundingSphere;
    public string ShaderTechniqueName;
    
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
    
    public void Draw(Matrix world)
    {
        if(Globals.BoundingFrustum.Contains(_boundingSphere.Transform(world)) == ContainmentType.Disjoint)return;
        if (Globals.MainEffect.CurrentTechnique.Name != ShaderTechniqueName)
            Globals.MainEffect.CurrentTechnique = Globals.MainEffect.Techniques[ShaderTechniqueName];
        foreach (ModelMesh mesh in Model.Meshes)
        {
            foreach (ModelMeshPart part in mesh.MeshParts)
            {
                if (part.PrimitiveCount > 0)
                {
                    Globals.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                    Globals.GraphicsDevice.Indices = part.IndexBuffer;
                    
                    //Pass textures to the shader
                    Globals.MainEffect.Parameters["albedo"]?.SetValue(Textures[0]);
                    Globals.MainEffect.Parameters["normal"]?.SetValue(Textures[1]);
                    Globals.MainEffect.Parameters["roughness"]?.SetValue(Textures[2]);
                    Globals.MainEffect.Parameters["metalness"]?.SetValue(Textures[3]);
                    Globals.MainEffect.Parameters["ao"]?.SetValue(Textures[4]);
                    
                    //Pass world and normal matrices to the shader
                    Globals.MainEffect.Parameters["World"]?.SetValue(world);
                    //ModelEffect.Parameters["View"].SetValue(Globals.View);
                    //ModelEffect.Parameters["Projection"].SetValue(Globals.Projection);
                    Matrix temp = Matrix.Transpose(Matrix.Invert(world));
                    temp.M41 = 0;
                    temp.M42 = 0;
                    temp.M43 = 0;
                    temp.M44 = 1;
                    temp.M14 = 0;
                    temp.M24 = 0;
                    temp.M34 = 0;
                    Globals.MainEffect.Parameters["normalMatrix"]?.SetValue(temp);
                    
                    for (int i = 0; i < Globals.MainEffect.CurrentTechnique.Passes.Count; i++)
                    {
                        Globals.MainEffect.CurrentTechnique.Passes[i].Apply();
                        Globals.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }
        } 
    }
    
    public ModelData(ContentManager manager, string modelPath)
    {
        LoadModel(manager,modelPath);
        _boundingSphere = CalculateBoundingSphere();
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
        ShaderTechniqueName = "PBR";
        Model = manager.Load<Model>(modelPath + "/" + modelName);
    }

    public ModelData(ContentManager manager, string modelPath, Vector3 boundingPosition, float boundingRadius)
    {
        LoadModel(manager,modelPath);
        _boundingSphere.Center = boundingPosition;
        _boundingSphere.Radius = boundingRadius;
    }

    public string Serialize()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<path>"+ ModelPath +"</path>");
        builder.Append("<technique>"+ ShaderTechniqueName +"</technique>");
        
        return builder.ToString();
    }

}