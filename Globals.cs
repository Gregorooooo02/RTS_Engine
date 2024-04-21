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
        public static BasicEffect BasicEffect;
        public static Effect TestEffect;

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

        //Switches for debug windows UWU
        public static bool InspectorVisible = true;
        public static bool HierarchyVisible = true;
        public static bool SceneSelectionVisible = true;
        public static bool MapModifyVisible = true;
#endif
    }
}
