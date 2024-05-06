using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using Num = System.Numerics;
using System.Security.Cryptography;

namespace RTS_Engine;

public class WorldRenderer : Component
{
    public struct VertexPositionColorNormal
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );
    };

    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;

    private Texture2D _heightMap;

    // Mesh parameters for the terrain
    private VertexPositionColorNormal[] _vertices;
    private short[] _indices;
    private int _width = 8;
    private int _height = 8;
    private float[,] _heightData;

    private Num.Vector3[] _colors = new Num.Vector3[]
    {
        new Num.Vector3(0.25f, 0.75f, 0.25f), // Blue (Green for now)
        new Num.Vector3(0.25f, 0.75f, 0.25f), // Yellow (Green for now)
        new Num.Vector3(0.5f, 0.5f, 0.5f), // Green (Gray for now)
        new Num.Vector3(0.5f, 0.5f, 0.5f), // Grey
        new Num.Vector3(1, 1, 1) // White
    };

    public WorldRenderer(GameObject parentObject)
    {
        ParentObject = parentObject;
        Initialize();
    }

    public WorldRenderer(GameObject parentObject, Texture2D heightMap)
    {
        ParentObject = parentObject;
        _heightMap = heightMap;
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

        RasterizerState rs = new RasterizerState();
        rs.CullMode = CullMode.None;
        rs.FillMode = FillMode.Solid;
        Globals.GraphicsDevice.RasterizerState = rs;

        Matrix worldMatrix = Matrix.CreateTranslation(-_width / 2, -40, -100);
        Globals.TerrainEffect.CurrentTechnique = Globals.TerrainEffect.Techniques["Colored"];
        Globals.TerrainEffect.Parameters["xView"]?.SetValue(Globals.View);
        Globals.TerrainEffect.Parameters["xProjection"]?.SetValue(Globals.Projection);
        Globals.TerrainEffect.Parameters["xWorld"]?.SetValue(worldMatrix);

        Vector3 lightDirection = new Vector3(1.0f, -1.0f, -1.0f);
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
                _indices.Length / 3);
        }
    }

    public override void Initialize()
    {
        // _heightMap = AssetManager.DefaultHeightMap;
        _heightMap = GenerateMap.noiseTexture;

        LoadHeightData(_heightMap);
        SetUpVertices();
        SetUpIndices();
        CalculateNormals();
        CopyToBuffers();
    }

    private void SetUpVertices()
    {
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
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

        _vertices = new VertexPositionColorNormal[_width * _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _vertices[x + y * _width].Position = new Vector3(x, _heightData[x, y], -y);
                
                if (_heightData[x, y] < minHeight + (maxHeight - minHeight) * 0.3f)
                {
                    _vertices[x + y * _width].Color = new Color(_colors[0]);
                }
                else if (_heightData[x, y] < minHeight + (maxHeight - minHeight) * 0.35f)
                {
                    _vertices[x + y * _width].Color = new Color(_colors[1]);
                }
                else if (_heightData[x, y] < minHeight + (maxHeight - minHeight) * 0.6f)
                {
                    _vertices[x + y * _width].Color = new Color(_colors[2]);
                }
                else if (_heightData[x, y] < minHeight + (maxHeight - minHeight) * 0.7f)
                {
                    _vertices[x + y * _width].Color = new Color(_colors[3]);
                }
                else
                {
                    _vertices[x + y * _width].Color = new Color(_colors[4]);
                }
            }
        }
    }

    private void SetUpIndices()
    {
        _indices = new short[(_width - 1) * (_height - 1) * 6];
        int counter = 0;

        for (int y = 0; y < _height - 1; y++)
        {
            for (int x = 0; x < _width - 1; x++)
            {
                short lowerLeft = (short)(x + y * _width);
                short lowerRight = (short)((x + 1) + y * _width);
                short topLeft = (short)(x + (y + 1) * _width);
                short topRight = (short)((x + 1) + (y + 1) * _width);

                _indices[counter++] = topLeft;
                _indices[counter++] = lowerRight;
                _indices[counter++] = lowerLeft;

                _indices[counter++] = topLeft;
                _indices[counter++] = topRight;
                _indices[counter++] = lowerRight;
            }
        }
    }

    private void CalculateNormals()
    {
        for (int i = 0; i < _vertices.Length; i++)
        {
            _vertices[i].Normal = new Vector3(0, 0, 0);
        }

        for (int i = 0; i < _indices.Length / 3; i++)
        {
            int index1 = _indices[i * 3];
            int index2 = _indices[i * 3 + 1];
            int index3 = _indices[i * 3 + 2];

            Vector3 side1 = _vertices[index1].Position - _vertices[index3].Position;
            Vector3 side2 = _vertices[index1].Position - _vertices[index2].Position;
            Vector3 normal = Vector3.Cross(side1, side2);

            _vertices[index1].Normal += normal;
            _vertices[index2].Normal += normal;
            _vertices[index3].Normal += normal;
        }

        for (int i = 0; i < _vertices.Length; i++)
        {
            _vertices[i].Normal.Normalize();
        }
    }

    private void CopyToBuffers()
    {
        _vertexBuffer = new VertexBuffer(Globals.GraphicsDevice, VertexPositionColorNormal.VertexDeclaration, _vertices.Length, BufferUsage.WriteOnly);
        _vertexBuffer.SetData(_vertices);

        _indexBuffer = new IndexBuffer(Globals.GraphicsDevice, typeof(short), _indices.Length, BufferUsage.WriteOnly);
        _indexBuffer.SetData(_indices);
    }

    private void LoadHeightData(Texture2D heightMap)
    {
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
            ImGui.Text("Height map loaded: " + (_heightMap != null ? "Yes" : "No"));
            ImGui.Text("Width: " + _width);
            ImGui.Text("Height: " + _height);

            ImGui.Separator();

            ImGui.Text("Color settings:");
            if (ImGui.ColorEdit3("First color", ref _colors[0]))
            {
                SetUpVertices();
            }
            if (ImGui.ColorEdit3("Second color", ref _colors[1]))
            {
                SetUpVertices();
            }
            if (ImGui.ColorEdit3("Third color", ref _colors[2]))
            {
                SetUpVertices();
            }
            if (ImGui.ColorEdit3("Fourth color", ref _colors[3]))
            {
                SetUpVertices();
            }
            if (ImGui.ColorEdit3("Fifth color", ref _colors[4]))
            {
                SetUpVertices();
            }
            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
            }
        }
    }
#endif
}
