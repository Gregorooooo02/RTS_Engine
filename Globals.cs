using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine
{
    internal class Globals
    {
        public static Globals Instance;
        public static void Initialize() 
        {
            Instance = new Globals();
        }
        
        public static float TotalSeconds { get; set; }
        public GraphicsDevice GraphicsDevice;
        public SpriteBatch SpriteBatch;

        public static void Update(GameTime gameTime)
        {
            TotalSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        private Globals()
        {
#if DEBUG
            ComponentsTypes = GetAllComponents();
#endif
        }

#if DEBUG
        private List<Type> GetAllComponents()
        {
            Type baseType = typeof(Component);
            Type transform = typeof(Transform);
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetTypes().Where(x => baseType.IsAssignableFrom(x) && x != baseType && x != transform).ToList();
        }

        public List<Type> ComponentsTypes;
        public GameObject CurrentlySelectedObject;

        //Switches for debug windows UWU
        public bool InspectorVisible = true;
        public bool HierarchyVisible = true;
        public bool SceneSelectionVisible = true;
        public bool MapModifyVisible = true;
#endif
    }
}
