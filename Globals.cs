using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine
{
    internal class Globals
    {
        public static Globals Instance;
        public static void Initialize(ContentManager asset) 
        {
            Instance = new Globals();
            Instance.DefaultModel = asset.Load<Model>("defaultCube");
            Instance.DefaultSprite = asset.Load<Texture2D>("smile");
            Instance.DefaultFont = asset.Load<SpriteFont>("defaultFont");
        }

        public Model DefaultModel;
        public Texture2D DefaultSprite;
        public SpriteFont DefaultFont;
        
        public SpriteBatch SpriteBatch;

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
#endif
    }
}
