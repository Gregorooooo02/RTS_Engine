using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using RTS_Engine.Components.AI;

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
    
    #region Pathfinding parameters

    public MapNode[,] MapNodes;
    public int NodeFrequency = 1;
    private float maxAngle = 30.0f;
    
    #endregion
    
    
    public float[,] HeightData;
    private int _terrainWidth;
    private int _terrainHeight;
    
    private Texture2D _sandTexture;
    private Texture2D _grassTexture;
    private Texture2D _rockTexture;
    private Texture2D _snowTexture;
    
    private readonly List<Chunk> _chunks = new List<Chunk>();
    private readonly int _chunkSize = 128;
    
    private WaterBody _waterBody;
    private List<WaterBody> _waterBodies = new List<WaterBody>();
    
    // Voronoi stuff
    private Dictionary<Vector2, List<Vector2>> _voronoiRegions;
    public List<GameObject> Features;

    public WorldRenderer(GameObject parentObject)
    {
        ParentObject = parentObject;
        Initialize();
    }

    public WorldRenderer(){}

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
                vertices[x + y * chunkWidth].Position = new Vector3(x + chunkX, heightValue, (y + chunkY));
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
        _terrainWidth = heightmap.Width;
        _terrainHeight = heightmap.Height;
        
        Color[] heightMapColors = new Color[_terrainWidth * _terrainHeight];
        heightmap.GetData(heightMapColors);
        
        HeightData = new float[_terrainWidth, _terrainHeight];
        
        float globalMinHeight = float.MaxValue;
        float globalMaxHeight = float.MinValue;
        
        for (int x = 0; x < _terrainWidth; x++)
        {
            for (int y = 0; y < _terrainHeight; y++)
            {
                HeightData[x, y] = heightMapColors[x + y * _terrainWidth].R / 5.0f;
                
                if (HeightData[x, y] < globalMinHeight)
                {
                    globalMinHeight = HeightData[x, y];
                }
                if (HeightData[x, y] > globalMaxHeight)
                {
                    globalMaxHeight = HeightData[x, y];
                }
            }
        }
        
        for (int x = 0; x < _terrainWidth - 1; x += _chunkSize - 1)
        {
            for (int y = 0; y < _terrainHeight - 1; y += _chunkSize - 1)
            {
                int chunkWidth = Math.Min(_chunkSize, _terrainWidth - x);
                int chunkHeight = Math.Min(_chunkSize, _terrainHeight - y);
                
                CreateChunk(
                    x,
                    y,
                    chunkWidth,
                    chunkHeight,
                    HeightData,
                    heightMapColors,
                    globalMinHeight,
                    globalMaxHeight
                );
                
                _waterBody = new WaterBody(x, y, _chunkSize, 5);
                _waterBodies.Add(_waterBody);
            }
        }
        MapNodes = new MapNode[_terrainWidth / NodeFrequency, _terrainHeight / NodeFrequency];

        //Setup map nodes
        for (int i = 0; i < MapNodes.GetLength(0); i++)
        {
            for (int j = 0; j < MapNodes.GetLength(1); j++)
            {
                float nodeHeight = 0;
                for (int k = 0; k < NodeFrequency; k++)
                {
                    for (int l = 0; l < NodeFrequency; l++)
                    {
                        try
                        {
                            nodeHeight += HeightData[i * NodeFrequency + k, j * NodeFrequency + l];
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
                MapNodes[i,j] = new MapNode(new Point(i, j),nodeHeight / (NodeFrequency * NodeFrequency));
            }
        }
        //Create connection between the nodes
        for (int i = 0; i < MapNodes.GetLength(0); i++)
        {
            for (int j = 0; j < MapNodes.GetLength(1); j++)
            {
                byte neighborMask = 0;
                //Iterate through node's neighbours and check connections
                for (int k = -1; k < 2; k++)
                {
                    for (int l = -1; l < 2; l++)
                    {
                        if(k == 0 && l == 0) continue;
                        try
                        {
                            //Calculate height difference between two points
                            var heightDifference = MathF.Abs(MapNodes[i + k, j + l].Height - MapNodes[i, j].Height);
                            //Calculate offset vector between the point in XZ space
                            Point temp = MapNodes[i + k, j + l].Location - MapNodes[i, j].Location;
                            //Calculate tangent of tilt angle between two points
                            float tanA = heightDifference / MathF.Sqrt((temp.X * temp.X) + (temp.Y * temp.Y));
                            //Calculate the angle itself
                            float angle = MathF.Atan(tanA) * 180.0f / MathF.PI;
                            //If the calculated angle is smaller than the maximum allowed then there is a connection between points
                            if (angle <= maxAngle) neighborMask += 1;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        if(k != 1 || l != 1)neighborMask <<= 1;
                    }
                }
                MapNodes[i, j].Connections = neighborMask;
            }
        }

        /*
        for (int i = 0; i < MapNodes.GetLength(0); i++)
        {
            for (int j = 0; j < MapNodes.GetLength(1); j++)
            {
                Console.Write(Convert.ToString(MapNodes[i,j].Connections,2).ToCharArray().Count(c => c == '1') + " ");
            }
            Console.WriteLine("");
        }
        */
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
        DrawFeatures();
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
        
        Features = new List<GameObject>();

        GenerateVoronoiFeatures();
        Console.WriteLine($"Generated {_voronoiRegions.Count} Voronoi regions.");
        
        Globals.Renderer.WorldRenderer = this;
    }
    
    // Voronoi methods
    private void GenerateVoronoiFeatures()
    {
        var points = GenerateRandomPoints(200, _terrainWidth, _terrainHeight);
        _voronoiRegions = ComputeVoronoiDiagram(points, _terrainWidth, _terrainHeight);
        ClipVoronoiCells(_voronoiRegions, _terrainWidth, _terrainHeight);
        PlaceFeatures(_voronoiRegions);
    }
    
    private List<Vector2> GenerateRandomPoints(int numPoints, int width, int height)
    {
        Random random = new Random();
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < numPoints; i++)
        {
            float x = (float)(random.NextDouble() * width);
            float y = (float)(random.NextDouble() * height);
            points.Add(new Vector2(x, y));
        }
        
        return points;
    }

    private Dictionary<Vector2, List<Vector2>> ComputeVoronoiDiagram(List<Vector2> sites, int width, int height)
    {
        var voronoiRegions = new Dictionary<Vector2, List<Vector2>>();

        foreach (var site in sites)
        {
            voronoiRegions[site] = new List<Vector2>();
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 closestSite = Vector2.Zero;
                float closestDistance = float.MaxValue;

                foreach (var site in sites)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), site);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestSite = site;
                    }
                }
                
                voronoiRegions[closestSite].Add(new Vector2(x, y));
            }
        }
        
        return voronoiRegions;
    }
    
    public void ClipVoronoiCells(Dictionary<Vector2, List<Vector2>> voronoiRegions, int width, int height)
    {
        foreach (var site in voronoiRegions.Keys.ToList())
        {
            var region = voronoiRegions[site];
            var clippedRegion = region.Where(p => p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height).ToList();
            voronoiRegions[site] = clippedRegion;
        }
    }
    
    private void PlaceFeatures(Dictionary<Vector2, List<Vector2>> voronoiRegions)
    {
        Random random = new Random();
        
        foreach (var kvp in voronoiRegions)
        {
            var site = kvp.Key;
            var region = kvp.Value;
            
            // Place a feature in the center of the region
            Vector3 position = CalculateCentroid(region);
            if (random.NextDouble() > 0.5)
            {
                if (HeightData[(int)position.X, (int)position.Z] > 6.0f
                    && HeightData[(int)position.X, (int)position.Z] < 25.0f)
                {
                    PlaceTree(position);    
                }
            }
            else
            {
                // Place a rock
            }
        }
    }
    
    private Vector3 CalculateCentroid(List<Vector2> region)
    {
        float x = 0;
        float y = 0;
        foreach (var point in region)
        {
            x += point.X;
            y += point.Y;
        }
        
        x /= region.Count;
        y /= region.Count;
        
        // Return the position in world space and make that the y coordinate is equal to the height of the terrain
        return new Vector3(x, HeightData[(int)x, (int)y] + 8, y);
    }
    
    private void PlaceTree(Vector3 position)
    {
        GameObject tree = new GameObject();
        tree.Name = "Tree";
        tree.Transform.SetLocalPosition(position);
        tree.AddComponent<MeshRenderer>();
        tree.GetComponent<MeshRenderer>().LoadModel("Env/Trees/drzewoiglaste");
        
        Features.Add(tree);
    }
    
    private void DrawFeatures()
    {
        foreach (var feature in Features)
        {
            feature.GetComponent<MeshRenderer>()._model.Draw(
                ParentObject.Transform.ModelMatrix * 
                Matrix.CreateTranslation(feature.Transform._pos)
            );
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
