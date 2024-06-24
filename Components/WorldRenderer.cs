using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static readonly VertexDeclaration ShadowDeclaration = new VertexDeclaration(
            new VertexElement(0,VertexElementFormat.Vector3, VertexElementUsage.Position,0));
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

        public VertexBuffer ShadowBuffer;
    }
    
    #region Pathfinding parameters

    public MapNode[,] MapNodes;
    public int NodeFrequency = 1;
    private float maxAngle = 25.0f;
    
    #endregion

    public float[,] FinalHeightData;
    
    public float[,] HeightData;
    public readonly float HeightScale = 1.5f;
    private int _terrainWidth;
    private int _terrainHeight;
    
    private Texture2D _sandTexture;
    private Texture2D _grassTexture;
    private Texture2D _rockTexture;
    private Texture2D _snowTexture;

    public float MaxWaterLevel = 5.2f;
    
    private readonly List<Chunk> _chunks = new List<Chunk>();
    private readonly int _chunkSize = 128;
    
    private WaterBody _waterBody;
    private List<WaterBody> _waterBodies = new List<WaterBody>();

    private Color[] _scannedHeightData;

    private float[] rockOffsets = { 1.0f, 1.0f, 1.0f };
    
    private float globalMinHeight = float.MaxValue;
    private float globalMaxHeight = float.MinValue;
    
    // Voronoi stuff
    private Dictionary<Vector2, List<Vector2>> _voronoiRegions;

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
        Vector3 chunkPosition = new Vector3(chunkX, 0, chunkY);
        
        VertexMultitextured[] vertices = new VertexMultitextured[chunkWidth * chunkHeight];
        short[] indices = new short[(chunkWidth - 1) * (chunkHeight - 1) * 6];
        Vector3[] positions = new Vector3[chunkWidth * chunkHeight];
        
        // Smooth the height data
        float[,] smoothedHeightData = SmoothHeightData(heightData, chunkX, chunkY, chunkWidth, chunkHeight);
        
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                float heightValue = smoothedHeightData[x, y] * HeightScale;
                
                // Making the ground taller, to the sand is lower than the grass
                if (heightValue < 6.0f)
                {
                    heightValue /= 1.2f + MathF.Log(6.0f / heightValue);
                }

                if (heightValue > 30.0f)
                {
                    // heightValue *= heightValue / 30.0f;
                    heightValue *= 1.0f + MathF.Log(heightValue / 30.0f);
                }
                
                vertices[x + y * chunkWidth].Position = new Vector3(x + chunkX, heightValue, (y + chunkY));
                FinalHeightData[x + chunkX, y + chunkY] = vertices[x + y * chunkWidth].Position.Y;
                positions[x + y * chunkWidth] = vertices[x + y * chunkWidth].Position;
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

        VertexBuffer shadowData = new VertexBuffer(Globals.GraphicsDevice, VertexMultitextured.ShadowDeclaration,positions.Length, BufferUsage.WriteOnly);
        shadowData.SetData(positions);
        
        _chunks.Add(new Chunk
        {
            Vertices = vertices,
            Indices = indices,
            VertexBuffer = vertexBuffer,
            IndexBuffer = indexBuffer,
            TerrainWidth = chunkWidth,
            TerrainHeight = chunkHeight,
            Position = chunkPosition,
            ShadowBuffer = shadowData
        });
    }
    
    public void LoadHeightData(Texture2D heightmap)
    {
        HeightData = new float[_terrainWidth, _terrainHeight];
        FinalHeightData = new float[_terrainWidth, _terrainHeight];
        
        //float globalMinHeight = float.MaxValue;
        //float globalMaxHeight = float.MinValue;
        
        for (int x = 0; x < _terrainWidth; x++)
        {
            for (int y = 0; y < _terrainHeight; y++)
            {
                HeightData[x, y] = _scannedHeightData[x + y * _terrainWidth].R / 5.0f;
                
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
                    _scannedHeightData,
                    globalMinHeight,
                    globalMaxHeight
                );
            }
        }

        MaxWaterLevel = (globalMaxHeight - globalMinHeight) * 0.115f;
        
        _waterBody = new WaterBody(0, 0, _terrainWidth, MaxWaterLevel - 1.5f);
        _waterBodies.Add(_waterBody);
        
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
                            nodeHeight += FinalHeightData[i * NodeFrequency + k, j * NodeFrequency + l];
                        }
                        catch (Exception)
                        {
                            //ignored
                        }
                    }
                }
                MapNodes[i,j] = new MapNode(new Point(i, j),nodeHeight / (NodeFrequency * NodeFrequency));
                if (MapNodes[i, j].Height <= MaxWaterLevel) MapNodes[i, j].Available = false;
            }
        }
    }

    public void CalculatePathfindingGridConnections()
    {
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
                        if((k == 0 && l == 0)) continue;
                        if ((i + k >= 0 && i + k < MapNodes.GetLength(0) && j + l >= 0 &&
                             j + l < MapNodes.GetLength(1)))
                        {
                            if(!MapNodes[i + k, j + l].Available) continue;
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
                        if(k != 1 || l != 1)neighborMask <<= 1;
                    }
                }
                MapNodes[i, j].Connections = neighborMask;
            }
        }
    }

    private float[,] SmoothHeightData(float[,] heightData, int chunkX, int chunkY, int chunkWidth, int chunkHeight)
    {
        float[,] smoothedData = new float[chunkWidth, chunkHeight];

        for (int x = 0; x < chunkWidth; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                float total = 0;
                int count = 0;
                
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    for (int offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        int newX = x + chunkX + offsetX;
                        int newY = y + chunkY + offsetY;
                        
                        if (newX >= 0 && newX < heightData.GetLength(0) && newY >= 0 && newY < heightData.GetLength(1))
                        {
                            total += heightData[newX, newY];
                            count++;
                        }
                    }
                }
                
                smoothedData[x, y] = total / count;
            }
        }
        
        return smoothedData;
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
        DrawChunks();
    }

    public void DrawShadows(Effect shadowMapGenerator)
    {
        RasterizerState rs = new RasterizerState();
        rs.CullMode = CullMode.None;
        rs.FillMode = FillMode.Solid;
        Globals.GraphicsDevice.RasterizerState = rs;
        shadowMapGenerator.CurrentTechnique = shadowMapGenerator.Techniques["TerrainShadow"];
        for (int i = 0; i < _chunks.Count; i++)
        {
            shadowMapGenerator.Parameters["World"].SetValue(ParentObject.Transform.ModelMatrix);
            Globals.GraphicsDevice.SetVertexBuffer(_chunks[i].ShadowBuffer);
            Globals.GraphicsDevice.Indices = _chunks[i].IndexBuffer;
            foreach (EffectPass pass in shadowMapGenerator.CurrentTechnique.Passes)
            {
                pass.Apply();
                Globals.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,0,0, _chunks[i].Indices.Length / 3);
            }
        }
    }
    
    public void DrawChunks()
    {
        RasterizerState rs = new RasterizerState();
        rs.CullMode = CullMode.None;
        rs.FillMode = FillMode.Solid;
        Globals.GraphicsDevice.RasterizerState = rs;
        
        Globals.TerrainEffect.CurrentTechnique = Globals.TerrainEffect.Techniques["ShadowFog"];
        
        Globals.TerrainEffect.Parameters["xView"].SetValue(Globals.View);
        Globals.TerrainEffect.Parameters["xProjection"].SetValue(Globals.Projection);
        
        Globals.TerrainEffect.Parameters["xTexture0"].SetValue(_sandTexture);
        Globals.TerrainEffect.Parameters["xTexture1"].SetValue(_grassTexture);
        Globals.TerrainEffect.Parameters["xTexture2"].SetValue(_rockTexture);
        Globals.TerrainEffect.Parameters["xTexture3"].SetValue(_snowTexture);
        
        foreach (Chunk chunk in _chunks)
        {
            Globals.TerrainEffect.Parameters["xWorld"].SetValue(ParentObject.Transform.ModelMatrix);
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
         ScanHeightDataFromTexture(GenerateMap.noiseTexture);
    }

    public void GenerateWorld()
    {
        LoadTextures();
        LoadHeightData(GenerateMap.noiseTexture);
        GenerateVoronoiFeatures();
        
        //TODO: Move the invocation below, so it's executed after all changes to MapNodes array has been made. Mainly it should be executed after static terrain features are placed in mission.
        CalculatePathfindingGridConnections();
        // Console.WriteLine($"Generated {_voronoiRegions.Count} Voronoi regions.");
    }
    
    private void ScanHeightDataFromTexture(Texture2D heightmap)
    {
        _terrainWidth = heightmap.Width;
        _terrainHeight = heightmap.Height;
        
        _scannedHeightData = new Color[_terrainWidth * _terrainHeight];
        heightmap.GetData(_scannedHeightData);
    }
    
    // Voronoi methods
    private void GenerateVoronoiFeatures()
    {
        var points = GenerateRandomPoints(10, _terrainWidth, _terrainHeight);
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
        float bottomGrass = (globalMaxHeight - globalMinHeight) * 0.16f;
        float upperGrass = (globalMaxHeight - globalMinHeight) * 0.4f;
        
        Random random = new Random();
        
        GameObject trees = new GameObject();
        trees.Name = "Trees";
        
        GameObject rocks1 = new GameObject();
        GameObject rocks2 = new GameObject();
        GameObject rocks3 = new GameObject();

        GameObject villages = new GameObject();
        
        ParentObject.AddChildObject(trees);
        trees.AddComponent<InstancedRendererController>();
        trees.GetComponent<InstancedRendererController>().LoadModel("Env/Trees/drzewoiglaste");
        
        //TODO: Load models for rocks
        ParentObject.AddChildObject(rocks1);
        rocks1.AddComponent<InstancedRendererController>();
        
        ParentObject.AddChildObject(rocks2);
        rocks2.AddComponent<InstancedRendererController>();
        
        ParentObject.AddChildObject(rocks3);
        rocks3.AddComponent<InstancedRendererController>();

        ParentObject.AddChildObject(villages);
        villages.Name = "Villages";

        float minDistance = 5.0f;
        int maxAttempts = 10;
        List<Vector3> placedProps = new();
        List<Vector2> villagePositions = new();

        float minVillageDistance = 60.0f;

        int villageLimit = random.Next(1, 4);
        int currentVillageCount = 0;

        //How many village prefabs there are
        int villageVariants = 1;
        //List of already used variants
        List<int> availableVariants = new();

        for (int i = 0; i < villageVariants; i++)
        {
            availableVariants.Add(i + 1);
        }

        float minHeight = bottomGrass * 1.8f;
        float maxHeight = upperGrass;

        //TODO: Do some additional correction for spawning villages
        //Place villages
        foreach (var kvp in voronoiRegions)
        {
            if(currentVillageCount >= villageLimit || availableVariants.Count == 0) break;
            var site = kvp.Key;
            var region = kvp.Value;
            bool skip = false;
            
            Vector3 centroid = CalculateCentroid(region);
            Vector2 location = new Vector2(centroid.X, centroid.Z);
            foreach (Vector2 position in villagePositions)
            {
                if (Vector2.Distance(position, location) < minVillageDistance)
                {
                    skip = true;
                    break;
                }
            }
            if(skip) continue;
            float height = PickingManager.InterpolateWorldHeight(location, this);
            if (height > minHeight && height < maxHeight)
            {
                int variant = random.Next(0, availableVariants.Count);
#if _WINDOWS
                villages.LoadPrefab(Globals.MainPath + "/Prefabs/Village" + availableVariants[variant] +".xml");
#else
                villages.LoadPrefab("Prefabs/Village" + variant + ".xml");
#endif
                availableVariants.RemoveAt(variant);
                PlaceVillage(location,villages.Children.Last());
                currentVillageCount++;
                voronoiRegions.Remove(site);
            }
        }


        //Place player units
        //TODO: Implement placing player units in the world
        bool placedUnits = false;
        do
        {
            foreach (var kvp in voronoiRegions)
            {
                var site = kvp.Key;
                var region = kvp.Value;
                bool skip = false;

                Vector3 centroid = CalculateCentroid(region);
                Vector2 location = new Vector2(centroid.X, centroid.Z);
                foreach (Vector2 position in villagePositions)
                {
                    if (Vector2.Distance(position, location) < minVillageDistance)
                    {
                        skip = true;
                        break;
                    }
                }

                if (skip) continue;
                float height = PickingManager.InterpolateWorldHeight(location, this);
                if (height > minHeight && height < maxHeight)
                {
                    placedUnits = true;
                    byte unitsMask = GameManager.UnitsSelectedForMission;
                    string name = null;
                    for (int i = 0; i < 3; i++)
                    {
                        if ((unitsMask & 1) > 0)
                        {
                            switch (i)
                            {
                                case 0:
                                    name = "Chair.xml";
                                    break;
                                case 1:
                                    name = "Chair.xml";
                                    break;
                                case 2:
                                    name = "Minion.xml";
                                    break;
                            }

                            if (name == null) continue;
#if _WINDOWS
                            ParentObject.LoadPrefab(Globals.MainPath + "/Prefabs/" + name);
#else
                        ParentObject.LoadPrefab("Prefabs/" + name);
#endif
                            CorrectObjectPosition(ParentObject.Children.Last(),
                                location + (Vector2.One * 5.0f * (i - 1)));
                        }

                        unitsMask >>= 1;
                    }

                    voronoiRegions.Remove(site);
                    break;
                }
            }
            Console.WriteLine("Failed to spawn player units! Attempting new max height. (Old height: )");
            maxHeight *= 1.05f;
        } while (!placedUnits);

        
        
        //Place terrain features
        //TODO: Rework placing features. Right now in one region there will be tress OR boulders places instead of trees AND boulders. Additionally reduce the RNG while selecting the chunk
        foreach (var kvp in voronoiRegions)
        {
            var site = kvp.Key;
            var region = kvp.Value;
            
            // Try to place multiple trees in the region
            int propCount = random.Next(5, 30);
            
            
            if (random.NextDouble() < 0.9f)
            {
                // Place props
                for (int i = 0; i < propCount; i++)
                {
                    bool propPlaced = false;
                    for (int attempt = 0; attempt < maxAttempts; attempt++)
                    {
                        Vector2 randomPoint = region[random.Next(region.Count)];
                        if (random.NextDouble() < 0.7f)
                        {
                            //Place tree
                            Vector3 position = new Vector3(randomPoint.X,
                                FinalHeightData[(int)randomPoint.X, (int)randomPoint.Y] + 8, randomPoint.Y);
                            if (IsPositionValid(placedProps, position, minDistance))
                            {
                                if (HeightData[(int)position.X, (int)position.Z] > bottomGrass
                                    && HeightData[(int)position.X, (int)position.Z] < upperGrass)
                                {
                                    PlaceTree(trees, position);
                                    ObstructTerrain(new Vector2(position.X, position.Z), 3.0f);
                                    placedProps.Add(position);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            //Place rock
                            Vector3 position = new Vector3(randomPoint.X,
                                FinalHeightData[(int)randomPoint.X, (int)randomPoint.Y] + 8, randomPoint.Y);
                            if (IsPositionValid(placedProps, position, minDistance))
                            {
                                if (HeightData[(int)position.X, (int)position.Z] < upperGrass * 1.2f)
                                {
                                    int rockVariant = random.Next(3);
                                    switch (rockVariant)
                                    {
                                        case 0:
                                            PlaceRock(rocks1,position,0);
                                            break;
                                        case 1:
                                            PlaceRock(rocks2,position,1);
                                            break;
                                        case 2:
                                            PlaceRock(rocks3,position,2);
                                            break;
                                    }
                                    ObstructTerrain(new Vector2(position.X, position.Z), 1.0f);
                                    placedProps.Add(position);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void CorrectObjectPosition(GameObject gameObject, Vector2 offset)
    {
        Vector3 newPos = gameObject.Transform.Pos + new Vector3(offset.X,0,offset.Y);
        newPos.Y += PickingManager.InterpolateWorldHeight(new Vector2(newPos.X, newPos.Z), this);
        gameObject.Transform.SetLocalPosition(newPos);
    }
    
    private void PlaceVillage(Vector2 location, GameObject villageRoot)
    {
        //villageRoot.Children[0] buildings
        //villageRoot.Children[1] props
        //villageRoot.Children[2] soldiers
        //villageRoot.Children[3] civilians
        Vector3 offset = new Vector3(location.X, 0, location.Y);
        Vector3 newPos;
        foreach (GameObject building in villageRoot.Children[0].Children)
        {
            newPos = building.Transform.Pos + offset;
            newPos.Y += PickingManager.InterpolateWorldHeight(new Vector2(newPos.X, newPos.Z), this);
            building.Transform.SetLocalPosition(newPos);
        }

        foreach (GameObject prop in villageRoot.Children[1].Children)
        {
            newPos = prop.Transform.Pos + offset;
            newPos.Y += PickingManager.InterpolateWorldHeight(new Vector2(newPos.X, newPos.Z), this);
            prop.Transform.SetLocalPosition(newPos);
        }

        foreach (GameObject soldier in villageRoot.Children[2].Children)
        {
            newPos = soldier.Children[0].Transform.Pos + offset;
            newPos.Y += PickingManager.InterpolateWorldHeight(new Vector2(newPos.X, newPos.Z), this);
            soldier.Children[0].Transform.SetLocalPosition(newPos); 
            
            soldier.Children[1].Transform.SetLocalPosition(offset);
        }

        foreach (GameObject civilian in villageRoot.Children[3].Children)
        {
            newPos = civilian.Transform.Pos + offset;
            newPos.Y += PickingManager.InterpolateWorldHeight(new Vector2(newPos.X, newPos.Z), this);
            civilian.Transform.SetLocalPosition(newPos); 
        }
    }

    private void ObstructTerrain(Vector2 location, float radius)
    {
        //Remove pathfinding nodes in square around location, radius is half the length of the side of said square
        int leftX = (int)MathF.Ceiling(location.X - radius);
        int rightX = (int)(location.X + radius);
        
        int topY = (int)MathF.Ceiling(location.Y - radius);
        int bottomY = (int)(location.Y + radius);
        
        for (int i = leftX; i <= rightX; i++)
        {
            for (int j = topY; j <= bottomY; j++)
            {
                if(i < 0 || j < 0 || i > MapNodes.GetLength(0) - 2 || j > MapNodes.GetLength(1) - 2)continue;
                MapNodes[i, j].Available = false;
            }
        }
    }
    
    private bool IsPositionValid(List<Vector3> placedTrees, Vector3 newPosition, float minDistance)
    {
        foreach (var tree in placedTrees)
        {
            if (Vector3.Distance(tree, newPosition) < minDistance)
            {
                return false;
            }
        }
        
        return true;
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
    
    private void PlaceTree(GameObject root, Vector3 position)
    {
        Random random = new Random();
        
        GameObject tree = new GameObject();
        tree.Name = "Tree";
        root.AddChildObject(tree);
        tree.AddComponent<InstancedRendererUnit>();
        tree.Transform.SetLocalPosition(position);
        
        float randomRotation = (float)(random.NextDouble() * 360.0f);
        tree.Transform.SetLocalRotationY(randomRotation);
        
        // Make a random scale for the tree between 0.9 and 1.25
        float randomScaleY = (float)(random.NextDouble() * 0.35f + 0.9f);
        tree.Transform.SetLocalScale(new Vector3(1, randomScaleY, 1));
    }

    private void PlaceRock(GameObject root, Vector3 position, int rockId)
    {
        Random random = new Random();
        
        GameObject rock = new()
        {
            Name = "Rock" + rockId
        };
        root.AddChildObject(rock);
        rock.AddComponent<InstancedRendererUnit>();

        position.Y = PickingManager.InterpolateWorldHeight(new Vector2(position.X, position.Z), this) + rockOffsets[rockId];
        
        rock.Transform.SetLocalPosition(position);
        
        float randomRotation = (float)(random.NextDouble() * 360.0f);
        rock.Transform.SetLocalRotationY(randomRotation);
        
        // Make a random scale for the rocks between 0.7 and 1.4 for all axi
        float randomScaleX = (float)(random.NextDouble() * 0.7f + 0.7f);
        float randomScaleY = (float)(random.NextDouble() * 0.7f + 0.7f);
        float randomScaleZ = (float)(random.NextDouble() * 0.7f + 0.7f);
        rock.Transform.SetLocalScale(new Vector3(randomScaleX, randomScaleY, randomScaleZ));
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>WorldRenderer</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("</component>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        Initialize();
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
