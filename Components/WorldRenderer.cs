using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;

namespace RTS_Engine;

public class WorldRenderer : Component
{
    public struct VertexMultitextured
    {
        public Vector3 Position;
        public Vector3 Normal;

        // Texture variables
        public Vector4 TextureCoordinate;
        public Vector4 TextureWeights;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 10, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1)
        );
    };

    private int _width;
    private int _height;
    private float[,] _heightData;

    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    
    // Textures
    private Texture2D _sandTexture;
    private Texture2D _grassTexture;
    private Texture2D _rockTexture;
    private Texture2D _snowTexture;

    // Water parameters
    private const float waterHeight = 0.5f;

    private VertexBuffer _waterVertexBuffer;
    private IndexBuffer _waterIndexBuffer;
    private Texture2D _waterTexture;

    public WorldRenderer(GameObject parentObject)
    {
        ParentObject = parentObject;
        Initialize();
    }

    public WorldRenderer() 
    {
        Initialize();
    }

    public override void Update() {}

    public void Draw()
    {
        if (!Active) return;
        DrawWater();
        DrawTerrain();
    }

    public void DrawTerrain()
    {
        RasterizerState rs = new RasterizerState();
        rs.CullMode = CullMode.None;
        rs.FillMode = FillMode.Solid;
        Globals.GraphicsDevice.RasterizerState = rs;

        Globals.TerrainEffect.CurrentTechnique = Globals.TerrainEffect.Techniques["MultiTextured"];

        Globals.TerrainEffect.Parameters["xView"]?.SetValue(Globals.View);
        Globals.TerrainEffect.Parameters["xProjection"]?.SetValue(Globals.Projection);
        Globals.TerrainEffect.Parameters["xWorld"]?.SetValue(ParentObject.Transform.ModelMatrix);

        // For debug purpose
        Globals.TerrainEffect.Parameters["xTexture"]?.SetValue(_grassTexture);

        Globals.TerrainEffect.Parameters["xTexture0"]?.SetValue(_sandTexture);
        Globals.TerrainEffect.Parameters["xTexture1"]?.SetValue(_grassTexture);
        Globals.TerrainEffect.Parameters["xTexture2"]?.SetValue(_rockTexture);
        Globals.TerrainEffect.Parameters["xTexture3"]?.SetValue(_snowTexture);

        Vector3 lightDirection = new Vector3(0.0f, -1.0f, -1.0f);
        lightDirection.Normalize();
        Globals.TerrainEffect.Parameters["xLightDirection"]?.SetValue(lightDirection);
        Globals.TerrainEffect.Parameters["xAmbient"]?.SetValue(0.2f);
        Globals.TerrainEffect.Parameters["xEnableLighting"]?.SetValue(true);

        foreach (EffectPass pass in Globals.TerrainEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            Globals.GraphicsDevice.Indices = _indexBuffer;
            Globals.GraphicsDevice.SetVertexBuffer(_vertexBuffer);

            Globals.GraphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0,
                0,
                _indexBuffer.IndexCount / 3
            );
        }
    }

    public void DrawWater()
    {
        RasterizerState rs = new RasterizerState();
        rs.CullMode = CullMode.None;
        rs.FillMode = FillMode.Solid;
        Globals.GraphicsDevice.RasterizerState = rs;

        Globals.TerrainEffect.CurrentTechnique = Globals.TerrainEffect.Techniques["Textured"];

        Globals.TerrainEffect.Parameters["xView"]?.SetValue(Globals.View);
        Globals.TerrainEffect.Parameters["xProjection"]?.SetValue(Globals.Projection);
        Globals.TerrainEffect.Parameters["xWorld"]?.SetValue(ParentObject.Transform.ModelMatrix);
        
        Globals.TerrainEffect.Parameters["xTexture"]?.SetValue(_waterTexture);

        foreach (EffectPass pass in Globals.TerrainEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            Globals.GraphicsDevice.Indices = _waterIndexBuffer;
            Globals.GraphicsDevice.SetVertexBuffer(_waterVertexBuffer);

            Globals.GraphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0,
                0,
                _waterIndexBuffer.IndexCount / 3
            );
        }
    }

    public override void Initialize()
    {
        // _heightMap = AssetManager.DefaultHeightMap;
        Globals.Renderer.WorldRenderer = this;

        LoadVertices();
        LoadPlaneVertices();
        LoadTextures();
    }

    private void LoadTextures()
    {
        _sandTexture = AssetManager.DefaultTerrainTextrues[1];
        _grassTexture = AssetManager.DefaultTerrainTextrues[2];
        _rockTexture = AssetManager.DefaultTerrainTextrues[3];
        _snowTexture = AssetManager.DefaultTerrainTextrues[4];

        _waterTexture = AssetManager.DefaultTerrainTextrues[0];
    }

    private void LoadVertices()
    {
        Texture2D heightMap = GenerateMap.noiseTexture;
        LoadHeightData(heightMap);

        VertexMultitextured[] terrainVertices = SetUpVertices();
        short[] terrainIndices = SetUpIndices();

        terrainVertices = CalculateNormals(terrainVertices, terrainIndices);
        CopyToBuffers(terrainVertices, terrainIndices);
    }

    private void LoadPlaneVertices()
    {
        VertexMultitextured[] planeVertices = SetUpPlaneVertices();
        short[] planeIndices = SetUpIndices();

        planeVertices = CalculateNormals(planeVertices, planeIndices);
        CopyToPlaneBuffers(planeVertices, planeIndices);
    }

    private VertexMultitextured[] SetUpVertices()
    {
        VertexMultitextured[] terrainVertices = new VertexMultitextured[_width * _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                terrainVertices[x + y * _width].Position = new Vector3(x, _heightData[x, y] * 0.75f, -y);
                terrainVertices[x + y * _width].TextureCoordinate.X = (float)x / 30.0f;
                terrainVertices[x + y * _width].TextureCoordinate.Y = (float)y / 30.0f;
                
                terrainVertices[x + y * _width].TextureWeights.X = MathHelper.Clamp(1.0f - Math.Abs(_heightData[x, y] - 0) / 8.0f, 0, 10);
                terrainVertices[x + y * _width].TextureWeights.Y = MathHelper.Clamp(1.0f - Math.Abs(_heightData[x, y] - 8) / 4.0f, 0, 10);
                terrainVertices[x + y * _width].TextureWeights.Z = MathHelper.Clamp(1.0f - Math.Abs(_heightData[x, y] - 14) / 4.0f, 0, 10);
                terrainVertices[x + y * _width].TextureWeights.W = MathHelper.Clamp(1.0f - Math.Abs(_heightData[x, y] - 20) / 4.0f, 0, 10);

                float total = terrainVertices[x + y * _width].TextureWeights.X;
                total += terrainVertices[x + y * _width].TextureWeights.Y;
                total += terrainVertices[x + y * _width].TextureWeights.Z;
                total += terrainVertices[x + y * _width].TextureWeights.W;

                terrainVertices[x + y * _width].TextureWeights.X /= total;
                terrainVertices[x + y * _width].TextureWeights.Y /= total;
                terrainVertices[x + y * _width].TextureWeights.Z /= total;
                terrainVertices[x + y * _width].TextureWeights.W /= total;
            }
        }

        return terrainVertices;
    }

    private VertexMultitextured[] SetUpPlaneVertices()
    {
        VertexMultitextured[] planeVertices = new VertexMultitextured[_width * _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                planeVertices[x + y * _width].Position = new Vector3(x, waterHeight, -y);
                planeVertices[x + y * _width].TextureCoordinate.X = (float)x / 30.0f;
                planeVertices[x + y * _width].TextureCoordinate.Y = (float)y / 30.0f;
            }
        }

        return planeVertices;
    }

    private short[] SetUpIndices()
    {
        short[] indices = new short[(_width - 1) * (_height - 1) * 6];
        int counter = 0;

        for (int y = 0; y < _height - 1; y++)
        {
            for (int x = 0; x < _width - 1; x++)
            {
                short lowerLeft = (short)(x + y * _width);
                short lowerRight = (short)((x + 1) + y * _width);
                short topLeft = (short)(x + (y + 1) * _width);
                short topRight = (short)((x + 1) + (y + 1) * _width);

                indices[counter++] = topLeft;
                indices[counter++] = lowerRight;
                indices[counter++] = lowerLeft;

                indices[counter++] = topLeft;
                indices[counter++] = topRight;
                indices[counter++] = lowerRight;
            }
        }

        return indices;
    }

    private VertexMultitextured[] CalculateNormals(VertexMultitextured[] vertices, short[] indices)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].Normal = new Vector3(0, 0, 0);
        }

        for (int i = 0; i < indices.Length / 3; i++)
        {
            int index1 = indices[i * 3];
            int index2 = indices[i * 3 + 1];
            int index3 = indices[i * 3 + 2];

            Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
            Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
            Vector3 normal = Vector3.Cross(side1, side2);

            vertices[index1].Normal += normal;
            vertices[index2].Normal += normal;
            vertices[index3].Normal += normal;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].Normal.Normalize();
        }

        return vertices;
    }

    private void CopyToBuffers(VertexMultitextured[] vertices, short[] indices)
    {
        _vertexBuffer = new VertexBuffer(Globals.GraphicsDevice, VertexMultitextured.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
        _vertexBuffer.SetData(vertices);

        _indexBuffer = new IndexBuffer(Globals.GraphicsDevice, typeof(short), indices.Length, BufferUsage.WriteOnly);
        _indexBuffer.SetData(indices);
    }

    private void CopyToPlaneBuffers(VertexMultitextured[] vertices, short[] indices)
    {
        _waterVertexBuffer = new VertexBuffer(Globals.GraphicsDevice, VertexMultitextured.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
        _waterVertexBuffer.SetData(vertices);

        _waterIndexBuffer = new IndexBuffer(Globals.GraphicsDevice, typeof(short), indices.Length, BufferUsage.WriteOnly);
        _waterIndexBuffer.SetData(indices);
    }

    private void LoadHeightData(Texture2D heightMap)
    {
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        _width = heightMap.Width;
        _height = heightMap.Height;

        Color[] heightMapColors = new Color[_width * _height];
        heightMap.GetData(heightMapColors);

        _heightData = new float[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _heightData[x, y] = heightMapColors[x + y * _width].R / 5.0f;

                if (_heightData[x, y] < minHeight)
                {
                    minHeight = _heightData[x, y];
                }

                if (_heightData[x, y] > maxHeight)
                {
                    maxHeight = _heightData[x, y];
                }
            }
        }

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _heightData[x, y] = (_heightData[x, y] - minHeight) / (maxHeight - minHeight) * 20.0f;
            }
        }
    }

    public override string ComponentToXmlString()
    {
        throw new NotImplementedException();
    }

    public override void Deserialize(XElement element)
    {
        throw new NotImplementedException();
    }

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }

#if DEBUG
    public override void Inspect()
    {
        if (ImGui.CollapsingHeader("World Renderer"))
        {   
            ImGui.Checkbox("World active", ref Active);
            ImGui.Text("Width: " + _width);
            ImGui.Text("Height: " + _height);

            ImGui.Separator();

            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
            }
        }
    }
#endif
}
