using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using Num = System.Numerics;

namespace RTS_Engine;

public class WorldRenderer : Component
{
    private Texture2D _heightMap;
    private Effect _effect;

    // Mesh parameters for the terrain
    private VertexPositionColor[] _vertices;
    private short[] _indices;
    private int _width = 4;
    private int _height = 4;
    private float[,] _heightData;

    private Num.Vector3[] _colors = new Num.Vector3[]
    {
        new Num.Vector3(0, 0, 0.8f), // Blue
        new Num.Vector3(1.0f, 1.0f, 0.7f), // Yellow
        new Num.Vector3(0.25f, 0.75f, 0.25f), // Green
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

        _effect.CurrentTechnique = _effect.Techniques["ColoredNoShading"];
        _effect.Parameters["xView"].SetValue(Globals.View);
        _effect.Parameters["xProjection"].SetValue(Globals.Projection);
        _effect.Parameters["xWorld"].SetValue(ParentObject.Transform.ModelMatrix);

        foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
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
#if _WINDOWS
        _effect = this._content.Load<Effect>("effect");
#else
        byte[] bytecode = File.ReadAllBytes("Content/effects");
        _effect = new Effect(Globals.GraphicsDevice, bytecode);
#endif
        //_heightMap = AssetManager.HeightMap;
        _heightMap = GenerateMap.noiseTexture;

        LoadHeightData(_heightMap);
        SetUpVertices();
        SetUpIndices();
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

        _vertices = new VertexPositionColor[_width * _height];

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
