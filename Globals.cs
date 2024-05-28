﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RTS_Engine.Pathfinding;
using Point = System.Drawing.Point;

namespace RTS_Engine
{
    public enum LayerType 
    {
        DEFAULT,
        PLAYER,
        ENEMY,
        UI,
        PROP,
        BUILDING
    }

    public enum ScreenSize
    {
        NOTSET,
        WINDOWED,
        WINDOWED_FULLSCREEN,
        FULLSCREEN
    }
    
    internal class Globals
    {
        
        public static void Initialize() 
        {
            ComponentsTypes = GetAllComponents();
            BoundingFrustum = new BoundingFrustum(Matrix.Identity);

            ShadowInstanceDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
                new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
                new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
                new VertexElement(sizeof(float) * 12, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
            );
            
            InstanceVertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
                new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
                new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
                new VertexElement(sizeof(float) * 12, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3),
                new VertexElement(sizeof(float) * 16, VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 0),
                new VertexElement(sizeof(float) * 20, VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 1),
                new VertexElement(sizeof(float) * 24, VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 2),
                new VertexElement(sizeof(float) * 28, VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 3)
            );

            Solid = new RasterizerState() { FillMode = FillMode.Solid };
            WireFrame = new RasterizerState() { FillMode = FillMode.WireFrame };
        }
        
        #region Shaders

        public static Effect MainEffect;
        public static Effect TerrainEffect;

        #region ShaderParameters

        public static float Gamma = 2.2f;
        public static float LightIntensity = 10;

        #endregion

        #endregion
        
        #region CameraParameters

        public static Matrix View = Matrix.Identity;
        public static Matrix Projection = Matrix.Identity;
        public static Vector3 ViewPos;
        public static float ZoomDegrees = 45.0f;
        
        public static BoundingFrustum BoundingFrustum = new BoundingFrustum(Matrix.Identity);
        #endregion

        #region Managers

        public static FogManager FogManager;
        public static PickingManager PickingManager;
        public static ContentManager Content;

        #endregion

        #region Time
        
        public static float DeltaTime { get; set; }
        public static TimeSpan ElapsedGameTime { get; set; }
        public static bool IsPaused = false;

        #endregion
        
        #region Rendering
        public static GraphicsDevice GraphicsDevice;
        public static GraphicsDeviceManager GraphicsDeviceManager;
        public static SpriteBatch SpriteBatch;
        
        public static Renderer Renderer;
        public static VertexDeclaration InstanceVertexDeclaration;
        public static VertexDeclaration ShadowInstanceDeclaration;
        #endregion
        
        public static bool HitUI = false;
        
        public static void Update(GameTime gameTime)
        {
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            ElapsedGameTime = gameTime.ElapsedGameTime;

            if(InputManager.Instance.GetAction(GameAction.PAUSE)?.state == ActionState.RELEASED) IsPaused = !IsPaused;
            
            if(InputManager.Instance.GetAction(GameAction.RESIZE_FULLSCREEN)?.state == ActionState.RELEASED) ChangeScreenSize(ScreenSize.FULLSCREEN);
            if(InputManager.Instance.GetAction(GameAction.RESIZE_WINDOWED)?.state == ActionState.RELEASED) ChangeScreenSize(ScreenSize.WINDOWED);
            
            HitUI = false;
        }
        
        private static List<Type> GetAllComponents()
        {
            Type baseType = typeof(Component);
            Type transform = typeof(Transform);
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetTypes().Where(x => baseType.IsAssignableFrom(x) && x != baseType && x != transform).ToList();
        }

        private static ScreenSize currentSize;
        public static void ChangeScreenSize(ScreenSize newSize)
        {
            if (currentSize != newSize)
            {
                currentSize = newSize;
                switch (currentSize)
                {
                    case ScreenSize.WINDOWED:
                    {
                        GraphicsDeviceManager.PreferredBackBufferWidth = 1440;
                        GraphicsDeviceManager.PreferredBackBufferHeight = 900;
                        GraphicsDeviceManager.IsFullScreen = false;
                        break;
                    }
                    case ScreenSize.WINDOWED_FULLSCREEN:
                    {
                        GraphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                        GraphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                        GraphicsDeviceManager.IsFullScreen = false;
                        break; 
                    }
                    
                    case ScreenSize.FULLSCREEN:
                    {
                        GraphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                        GraphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                        GraphicsDeviceManager.IsFullScreen = true;
                        break;
                    }
                }
                GraphicsDeviceManager.ApplyChanges();
            }
        }

#if _WINDOWS
        public static readonly string MainPath = "../../../";
#else
        public static string MainPath = "";
#endif

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
        public static RasterizerState Solid;
        public static RasterizerState WireFrame;
        
        public static GameObject CurrentlySelectedObject;
        public static List<string> AvailableScenes = new List<string>();
        public static List<string> AvailablePrefabs = new List<string>();

        public static void UpdateScenesList()
        {
            AvailableScenes = Directory.GetFiles(MainPath + "Scenes").ToList();
        }
        
        public static void UpdatePrefabList()
        {
            AvailablePrefabs = Directory.GetFiles(MainPath + "Prefabs").ToList();
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
        public static bool DrawWireframe = false;
        public static bool DrawSelectFrustum = false;
        public static bool DrawExplored = false;
        public static bool DrawVisibility = false;
#endif
    }
}
