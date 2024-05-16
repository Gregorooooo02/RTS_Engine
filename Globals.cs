using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTS_Engine.Pathfinding;
using Point = System.Drawing.Point;

namespace RTS_Engine
{
    internal class Globals
    {
        
        public static void Initialize() 
        {
            ComponentsTypes = GetAllComponents();
            // CreateNodes();
        }
        
        public static float DeltaTime { get; set; }
        public static GraphicsDevice GraphicsDevice;
        public static SpriteBatch SpriteBatch;
        public static Effect MainEffect;
        public static Effect TerrainEffect;
        public static Matrix View = Matrix.Identity;
        public static Matrix Projection = Matrix.Identity;
        public static Vector3 ViewPos;
        public static float ZoomDegrees = 45.0f;
        // public static Node[,] Nodes;

        public static Renderer Renderer;
        public static PickingManager PickingManager;
        
        public static BoundingFrustum BoundingFrustum;

        public static float Gamma = 2.2f;
        public static float LightIntensity = 10;


        public enum LayerType 
        {
            DEFAULT,
            PLAYER,
            ENEMY,
            UI,
            PROP,
            BUILDING
        }

        public static void Update(GameTime gameTime)
        {
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        
        private static List<Type> GetAllComponents()
        {
            Type baseType = typeof(Component);
            Type transform = typeof(Transform);
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetTypes().Where(x => baseType.IsAssignableFrom(x) && x != baseType && x != transform).ToList();
        }

        public static List<Type> ComponentsTypes;

        //for pathfinding
        // public static void CreateNodes()
        // {
        //     for (int i = 0; i < 128; i++)
        //     {
        //         for (int j = 0; j < 128; j++)
        //         {
        //             Nodes[i, j] = new Node(new Point(i, j), true);
        //         }
        //     }
        // }
        
#if DEBUG
        public static GameObject CurrentlySelectedObject;
        public static List<string> AvailableScenes = new List<string>();

        #if _WINDOWS
        public static readonly string MainPath = "../../../";
        #else
        public static string MainPath = "";
        #endif

        public static void UpdateScenesList()
        {
            AvailableScenes = Directory.GetFiles(MainPath + "Scenes").ToList();
        }
        
        public static int ShadowMapResolutionMultiplier = 3;
        
        //Switches for debug windows UWU
        public static bool InspectorVisible = true;
        public static bool HierarchyVisible = true;
        public static bool SceneSelectionVisible = true;
        public static bool ShowShadowMap = false;
        public static bool DrawShadows = true;
        public static bool DrawMeshes = true;
        public static bool DebugCamera = true;
#endif
    }
}
