using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public sealed class WaterBody
{
    // Let's just create a basic plane for the water and modify the normal map to make it look like water
    private VertexPositionTexture[] _vertices;
    private short[] _indices;
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    
    private int _waterSize = 128;
    private float _waterHeight;

    private Vector2 _waveVelocity;
    private Vector2 _waveNormalOffset;
    private float _waveNormalScale;

    private float _waveTime;

    private Random random = new Random(1990);
    
    public int WaterSize
    {
        get => _waterSize;
        set => _waterSize = value;
    }
    
    public float WaterHeight
    {
        get => _waterHeight;
        set => _waterHeight = value;
    }
    
    private Texture2D _waveMap;
    private Vector4 _waterColor;

    // Constructor
    public WaterBody(int x, int y, int width, float height)
    {
        _waterSize = width;
        _waterHeight = height;
            
        _waveNormalScale = 1.0f;
        _waveVelocity = new Vector2(0.01f, 0.03f);

        _waterColor = new Vector4(0.5f, 0.5f, 0.75f, 1.0f);
        _waveMap = AssetManager.DefaultWaveNormalMap;
        
        SetUpVertices(x, y);
        SetUpIndices();
        
        // Create the vertex buffer
        _vertexBuffer = new VertexBuffer(Globals.GraphicsDevice, typeof(VertexPositionTexture), _vertices.Length, BufferUsage.WriteOnly);
        _vertexBuffer.SetData(_vertices);
        
        // Create the index buffer
        _indexBuffer = new IndexBuffer(Globals.GraphicsDevice, typeof(short), _indices.Length, BufferUsage.WriteOnly);
        _indexBuffer.SetData(_indices);
    }

    public void Update()
    {
        _waveTime += Globals.DeltaTime; 
        _waveNormalOffset += _waveVelocity * Globals.DeltaTime;
        _waterHeight += (float)Math.Sin(_waveTime) * 0.005f;
        
        // TODO: Change this to modify the world matrix and not all the vertices
        
        //for (int i = 0 ; i < _vertices.Length; i++)
        //{
        //    _vertices[i].Position.Y = _waterHeight;
        //}
        
        
        
        //_vertexBuffer.SetData(_vertices);
    }
    
    public void Draw(Matrix world)
    {
        
        Globals.TerrainEffect.CurrentTechnique = Globals.TerrainEffect.Techniques["WaterWaves"];
        world.M42 = _waterHeight;
        Globals.TerrainEffect.Parameters["xWorld"].SetValue(world);
        
        
        
        Globals.TerrainEffect.Parameters["xWaveMapScale"].SetValue(_waveNormalScale);
        Globals.TerrainEffect.Parameters["xWaveMapOffset"].SetValue(_waveNormalOffset);
        Globals.TerrainEffect.Parameters["xWaterColor"]?.SetValue(_waterColor);
        Globals.TerrainEffect.Parameters["xWaveNormalMap"]?.SetValue(_waveMap);
        
        Globals.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
        Globals.GraphicsDevice.Indices = _indexBuffer;
        
        foreach (EffectPass pass in Globals.TerrainEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            Globals.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertices, 0, _vertices.Length, _indices, 0, 2);
        }
        
        Globals.GraphicsDevice.SetVertexBuffer(null);
    }

    private void SetUpVertices(int x, int y)
    {
        _vertices = new VertexPositionTexture[4];
        
        _vertices[0].Position = new Vector3(x, _waterHeight, y);
        _vertices[0].TextureCoordinate = new Vector2(0, 0);
        
        _vertices[1].Position = new Vector3(x, _waterHeight, (_waterSize + y));
        _vertices[1].TextureCoordinate = new Vector2(0, 1);
        
        _vertices[2].Position = new Vector3(_waterSize + x, _waterHeight, y);
        _vertices[2].TextureCoordinate = new Vector2(1, 0);
        
        _vertices[3].Position = new Vector3(_waterSize + x, _waterHeight, (_waterSize + y));
        _vertices[3].TextureCoordinate = new Vector2(1, 1);
    }
    
    private void SetUpIndices()
    {
        _indices = new short[6];
        
        _indices[0] = 0;
        _indices[1] = 1;
        _indices[2] = 2;
        
        _indices[3] = 2;
        _indices[4] = 1;
        _indices[5] = 3;
    }
}