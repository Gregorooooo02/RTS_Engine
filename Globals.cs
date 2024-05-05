using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine
{
    internal class Globals
    {
        
        public static void Initialize() 
        {
            ComponentsTypes = GetAllComponents();
        }
        
        public static float TotalSeconds { get; set; }
        public static GraphicsDevice GraphicsDevice;
        public static SpriteBatch SpriteBatch;
        public static Effect MainEffect;
        public static Effect TerrainEffect;
        public static Matrix View;
        public static Matrix Projection;
        public static Matrix World;
        public static Vector3 ViewPos;

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
            TotalSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        
        private static List<Type> GetAllComponents()
        {
            Type baseType = typeof(Component);
            Type transform = typeof(Transform);
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetTypes().Where(x => baseType.IsAssignableFrom(x) && x != baseType && x != transform).ToList();
        }

        public static List<Type> ComponentsTypes;
#if DEBUG
        public static GameObject CurrentlySelectedObject;
        public static List<string> AvailableScenes = new List<string>();

        public static void UpdateScenesList()
        {
#if _WINDOWS
            AvailableScenes = Directory.GetFiles("../../../Scenes").ToList();
#else
            AvailableScenes = Directory.GetFiles("Scenes").ToList();
#endif
        }
        
        public static int ShadowMapResolutionMultiplier = 3;
        
        //Switches for debug windows UWU
        public static bool InspectorVisible = true;
        public static bool HierarchyVisible = true;
        public static bool SceneSelectionVisible = true;
        public static bool ShowShadowMap = false;
        public static bool DrawShadows = true;
        public static bool DrawMeshes = true;
#endif
    }
}
