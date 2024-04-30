using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;

namespace RTS_Engine;

public class WorldRenderer : Component
{
    private Texture2D _heightMap;

    // Mesh parameters for the terrain
    private VertexPositionColor[] _vertices;
    private short[] _indices;
    private int _width = 4;
    private int _height = 4;
    private float[,] _heightData;

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

    public override void Draw(Matrix _view, Matrix _projection)
    {
        if (!Active) return;

        RasterizerState rs = new RasterizerState();
        rs.CullMode = CullMode.None;
        rs.FillMode = FillMode.WireFrame;
        Globals.GraphicsDevice.RasterizerState = rs;

        Globals.BasicEffect.EnableDefaultLighting();
        Globals.BasicEffect.View = _view;
        Globals.BasicEffect.Projection = _projection;
        Globals.BasicEffect.World = ParentObject.Transform.ModelMatrix;

        foreach (EffectPass pass in Globals.BasicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            Globals.GraphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                _vertices,
                0,
                _vertices.Length,
                _indices,
                0,
                _indices.Length / 3,
                VertexPositionColor.VertexDeclaration
            );
        }
    }

    public override void Initialize()
    {
        //_heightMap = AssetManager.HeightMap;
        _heightMap = GenerateMap.noiseTexture;

        LoadHeightData(_heightMap);
        SetUpVertices();
        SetUpIndices();
    }

    private void SetUpVertices()
    {
        _vertices = new VertexPositionColor[_width * _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _vertices[x + y * _width].Position = new Vector3(x, _heightData[x, y], -y);
                _vertices[x + y * _width].Color = Color.White;
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

#if DEBUG
    public override void Inspect()
    {
        if (ImGui.CollapsingHeader("World Renderer"))
        {   
            ImGui.Checkbox("World active", ref Active);
            ImGui.Text("Height map loaded: " + (_heightMap != null ? "Yes" : "No"));
            ImGui.Text("Width: " + _width);
            ImGui.Text("Height: " + _height);
            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
            }
        }
    }
#endif
}
