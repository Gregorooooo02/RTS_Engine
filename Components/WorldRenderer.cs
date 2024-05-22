using System;
using System.Collections.Generic;
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
        public Vector4 TextureCoordinate;
        public Vector4 TexWeights;
        
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 10, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1)
        );
    }
    
    private struct Chunk
    {
        public VertexMultitextured[] Vertices;
        public short[] Indices;
        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;
        public int TerrainWidth;
        public int TerrainHeight;
        public Vector3 Position;
    }
    
    private Texture2D _sandTexture;
    private Texture2D _grassTexture;
    private Texture2D _rockTexture;
    private Texture2D _snowTexture;
    
    private readonly List<Chunk> _chunks = new List<Chunk>();
    private readonly int _chunkSize = 128;
    
    private WaterBody _waterBody;
    private List<WaterBody> _waterBodies = new List<WaterBody>();

    public WorldRenderer(GameObject parentObject)
    {
        ParentObject = parentObject;
        Initialize();
    }

    public WorldRenderer() 
    {
        Initialize();
    }

    private void LoadTextures()
    {
        _sandTexture = AssetManager.DefaultTerrainTextrues[1];
        _grassTexture = AssetManager.DefaultTerrainTextrues[2];
        _rockTexture = AssetManager.DefaultTerrainTextrues[3];
        _snowTexture = AssetManager.DefaultTerrainTextrues[4];
    }
    
    private void CreateChunk(int chunkX, int chunkY, int chunkWidth, int chunkHeight, float[,] heightData, Color[] heightMapColors, float globalMin, float globalMax)
    {
        Vector3 chunkPosition = new Vector3(chunkX, 0, -chunkY);
        
        VertexMultitextured[] vertices = new VertexMultitextured[chunkWidth * chunkHeight];
        short[] indices = new short[(chunkWidth - 1) * (chunkHeight - 1) * 6];
        
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                float heightValue = heightMapColors[(x + chunkX) + (y + chunkY) * heightData.GetLength(0)].R / 5.0f;
                vertices[x + y * chunkWidth].Position = new Vector3(x + chunkX, heightValue, -(y + chunkY));
            }
        }
        
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                float heightValue = heightMapColors[(x + chunkX) + (y + chunkY) * heightData.GetLength(0)].R / 5.0f;

                // Adjust the height thresholds for better texture distribution
                float weight0 = MathHelper.Clamp(1.0f - Math.Abs(heightValue - (globalMin + (globalMax - globalMin) * 0.05f)) / ((globalMax - globalMin) * 0.1f), 0, 1); // Sand
                float weight1 = MathHelper.Clamp(1.0f - Math.Abs(heightValue - (globalMin + (globalMax - globalMin) * 0.3f)) / ((globalMax - globalMin) * 0.2f), 0, 1); // Grass
                float weight2 = MathHelper.Clamp(1.0f - Math.Abs(heightValue - (globalMin + (globalMax - globalMin) * 0.6f)) / ((globalMax - globalMin) * 0.3f), 0, 1); // Rock
                float weight3 = MathHelper.Clamp(1.0f - Math.Abs(heightValue - globalMax) / ((globalMax - globalMin) * 0.3f), 0, 1); // Snow
                
                Vector4 texWeights = new Vector4(weight0, weight1, weight2, weight3);
                texWeights.Normalize();
                
                vertices[x + y * chunkWidth].TexWeights = texWeights;
            }
        }
        
        int counter = 0;
        for (int y = 0; y < chunkHeight - 1; y++)
        {
            for (int x = 0; x < chunkWidth - 1; x++)
            {
                short lowerLeft = (short)(x + y * chunkWidth);
                short lowerRight = (short)((x + 1) + y * chunkWidth);
                short topLeft = (short)(x + (y + 1) * chunkWidth);
                short topRight = (short)((x + 1) + (y + 1) * chunkWidth);
                
                indices[counter++] = topLeft;
                indices[counter++] = lowerRight;
                indices[counter++] = lowerLeft;
                
                indices[counter++] = topLeft;
                indices[counter++] = topRight;
                indices[counter++] = lowerRight;
            }
        }
        
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].Normal = Vector3.Zero;
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
        
        VertexBuffer vertexBuffer = new VertexBuffer(Globals.GraphicsDevice, VertexMultitextured.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
        vertexBuffer.SetData(vertices);
        
        IndexBuffer indexBuffer = new IndexBuffer(Globals.GraphicsDevice, typeof(short), indices.Length, BufferUsage.WriteOnly);
        indexBuffer.SetData(indices);
        
        _chunks.Add(new Chunk
        {
            Vertices = vertices,
            Indices = indices,
            VertexBuffer = vertexBuffer,
            IndexBuffer = indexBuffer,
            TerrainWidth = chunkWidth,
            TerrainHeight = chunkHeight,
            Position = chunkPosition
        });
    }
    
    public void LoadHeightData(Texture2D heightmap)
    {
        int terrainWidth = heightmap.Width;
        int terrainHeight = heightmap.Height;
        
        Color[] heightMapColors = new Color[terrainWidth * terrainHeight];
        heightmap.GetData(heightMapColors);
        
        float[,] heightData = new float[terrainWidth, terrainHeight];
        
        float globalMinHeight = float.MaxValue;
        float globalMaxHeight = float.MinValue;
        
        for (int x = 0; x < terrainWidth; x++)
        {
            for (int y = 0; y < terrainHeight; y++)
            {
                heightData[x, y] = heightMapColors[x + y * terrainWidth].R / 5.0f;
                
                if (heightData[x, y] < globalMinHeight)
                {
                    globalMinHeight = heightData[x, y];
                }
                if (heightData[x, y] > globalMaxHeight)
                {
                    globalMaxHeight = heightData[x, y];
                }
            }
        }
        
        for (int x = 0; x < terrainWidth - 1; x += _chunkSize - 1)
        {
            for (int y = 0; y < terrainHeight - 1; y += _chunkSize - 1)
            {
                int chunkWidth = Math.Min(_chunkSize, terrainWidth - x);
                int chunkHeight = Math.Min(_chunkSize, terrainHeight - y);
                
                CreateChunk(
                    x,
                    y,
                    chunkWidth,
                    chunkHeight,
                    heightData,
                    heightMapColors,
                    globalMinHeight,
                    globalMaxHeight
                );
                
                _waterBody = new WaterBody(x, y, _chunkSize, 5);
                _waterBodies.Add(_waterBody);
            }
        }
    }

    public override void Update()
    {
        foreach (WaterBody waterBody in _waterBodies)
        {
            waterBody.Update();
        }
    }

    public void Draw()
    {
        if (!Active) return;
        DrawChunk();
    }

    public void DrawChunk()
    {
        RasterizerState rs = new RasterizerState();
        rs.CullMode = CullMode.None;
        rs.FillMode = FillMode.Solid;
        Globals.GraphicsDevice.RasterizerState = rs;
        
        foreach (Chunk chunk in _chunks)
        {
            Globals.TerrainEffect.CurrentTechnique = Globals.TerrainEffect.Techniques["Multitextured"];
            Globals.TerrainEffect.Parameters["xView"].SetValue(Globals.View);
            Globals.TerrainEffect.Parameters["xProjection"].SetValue(Globals.Projection);
            Globals.TerrainEffect.Parameters["xWorld"].SetValue(ParentObject.Transform.ModelMatrix);
            
            Globals.TerrainEffect.Parameters["xTexture0"].SetValue(_sandTexture);
            Globals.TerrainEffect.Parameters["xTexture1"].SetValue(_grassTexture);
            Globals.TerrainEffect.Parameters["xTexture2"].SetValue(_rockTexture);
            Globals.TerrainEffect.Parameters["xTexture3"].SetValue(_snowTexture);
            
            Vector3 lightDir = new Vector3(1.0f, -1.0f, -1.0f);
            lightDir.Normalize();
            Globals.TerrainEffect.Parameters["xLightDirection"].SetValue(lightDir);
            Globals.TerrainEffect.Parameters["xAmbient"].SetValue(0.1f);
            Globals.TerrainEffect.Parameters["xEnableLighting"].SetValue(true);
            
            Globals.GraphicsDevice.SetVertexBuffer(chunk.VertexBuffer);
            Globals.GraphicsDevice.Indices = chunk.IndexBuffer;
            
            foreach (EffectPass pass in Globals.TerrainEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Globals.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, chunk.Vertices.Length, 0, chunk.Indices.Length / 3);
            }
        }

        foreach (WaterBody waterBody in _waterBodies)
        {
            waterBody.Draw(ParentObject.Transform.ModelMatrix);
        }
    }
    
    public override void Initialize()
    {
        LoadTextures();
        LoadHeightData(GenerateMap.noiseTexture);
        
        Globals.Renderer.WorldRenderers.Add(this);
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
            // ImGui.Text("Width: " + _terrainWidth);
            // ImGui.Text("Height: " + _terrainHeight);

            ImGui.Separator();

            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
            }
        }
    }
#endif
}
