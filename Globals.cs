﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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
    
    public enum UnitType
    {
        Cabinet,
        Candle,
        Chair,
        Chandelier,
        MiniCabinet,
        Wardrobe,
        Archer,
        Civilian,
        Knight
    }
    
    public enum SceneType
    {
        MISSION,
        BASE
    }
    
    

    public enum ScreenSize
    {
        NOTSET,
        WINDOWED,
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

#if DEBUG
            Solid = new RasterizerState() { FillMode = FillMode.Solid };
            WireFrame = new RasterizerState() { FillMode = FillMode.WireFrame};
#endif
        }
        
        //Audio
        public static AudioListener Listener = new AudioListener();
        public static SceneType CurrentSceneType = SceneType.BASE;
        
        #region Shaders

        public static Effect MainEffect;
        public static Effect TerrainEffect;
        
        public static float Gamma = 2.2f;
        #endregion
        
        #region CameraParameters

        public static Matrix View = Matrix.Identity;
        public static Matrix Projection = Matrix.Identity;
        public static Vector3 ViewPos;
        public static float ZoomDegrees = 45.0f;

        public static Camera CurrentCamera;
        
        public static BoundingFrustum BoundingFrustum = new BoundingFrustum(Matrix.Identity);
        #endregion

        #region Managers

        public static FogManager FogManager;
        public static PickingManager PickingManager;
        public static ContentManager Content;
        public static AgentsManager AgentsManager;
        public static AudioManager AudioManager;

        public static bool HitUI = false;
        public static bool UIActive = false;
        public static bool RegenerateWorld = false;
        public static bool CreatingTutorial = true;

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
        
        
        public static void Update(GameTime gameTime)
        {
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            ElapsedGameTime = gameTime.ElapsedGameTime;
            HitUI = false;

            if(InputManager.Instance.GetAction(GameAction.PAUSE)?.state == ActionState.RELEASED) IsPaused = !IsPaused;
            
            if(InputManager.Instance.GetAction(GameAction.RESIZE_FULLSCREEN)?.state == ActionState.RELEASED) ChangeScreenSize(ScreenSize.FULLSCREEN);
            if(InputManager.Instance.GetAction(GameAction.RESIZE_WINDOWED)?.state == ActionState.RELEASED) ChangeScreenSize(ScreenSize.WINDOWED);

            if (InputManager.Instance.GetAction(GameAction.DISABLE_PICKING)?.state == ActionState.RELEASED)
            {
                PickingManager.PlayerBuildingPickingActive = false;
                PickingManager.PlayerBuildingUiBuiltActive = false;
                PickingManager.PlayerMissionSelectPickingActive = false;
            }

            if (InputManager.Instance.GetAction(GameAction.ENABLE_PICKING)?.state == ActionState.RELEASED)
            {
                PickingManager.PlayerBuildingPickingActive = true;
                PickingManager.PlayerBuildingUiBuiltActive = true;
                PickingManager.PlayerMissionSelectPickingActive = true;
            }
            
            if (InputManager.Instance.GetAction(GameAction.TOGGLE_PICKING)?.state == ActionState.RELEASED)
            {
                PickingManager.PlayerBuildingPickingActive = !PickingManager.PlayerBuildingPickingActive;
                PickingManager.PlayerBuildingUiBuiltActive = !PickingManager.PlayerBuildingUiBuiltActive;
                PickingManager.PlayerMissionSelectPickingActive = !PickingManager.PlayerMissionSelectPickingActive;
            }

            if (RegenerateWorld)
            {
                SceneManager.Instance.CreateMissionScene(CreatingTutorial);
            }

            if (InputManager.Instance.GetAction(GameAction.LOWER_EFFECTS_VOLUME)?.state == ActionState.RELEASED)
            {
                float volume = SoundEffect.MasterVolume;
                volume = MathHelper.Clamp(volume - 0.05f, 0, 1);
                SoundEffect.MasterVolume = volume;
            }

            if (InputManager.Instance.GetAction(GameAction.INCREASE_EFFECTS_VOLUME)?.state == ActionState.RELEASED)
            {
                float volume = SoundEffect.MasterVolume;
                volume = MathHelper.Clamp(volume + 0.05f, 0, 1);
                SoundEffect.MasterVolume = volume;
            }
            
            if (InputManager.Instance.GetAction(GameAction.LOWER_GAMMA)?.state == ActionState.RELEASED)
            {
                Gamma = MathHelper.Clamp(Gamma - 0.2f, 1.4f, 4);
            }

            if (InputManager.Instance.GetAction(GameAction.INCREASE_GAMMA)?.state == ActionState.RELEASED)
            {
                Gamma = MathHelper.Clamp(Gamma + 0.2f, 1.4f, 4);
            }
        }
        
        private static List<Type> GetAllComponents()
        {
            Type baseType = typeof(Component);
            Type transform = typeof(Transform);
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetTypes().Where(x => baseType.IsAssignableFrom(x) && x != baseType && x != transform).ToList();
        }

        private static ScreenSize _currentSize;

        public static float Ratio = 1.0f;
        public static void ChangeScreenSize(ScreenSize newSize)
        {
            if (_currentSize != newSize)
            {
                _currentSize = newSize;
                switch (_currentSize)
                {
                    case ScreenSize.WINDOWED:
                    {
                        GraphicsDeviceManager.PreferredBackBufferWidth = 1600;
                        GraphicsDeviceManager.PreferredBackBufferHeight = 900;
                        GraphicsDeviceManager.IsFullScreen = false;
                        Ratio = 1.0f;
                        break;
                    }
                    
                    case ScreenSize.FULLSCREEN:
                    {
                        GraphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                        GraphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                        GraphicsDeviceManager.IsFullScreen = true;
                        Ratio = GraphicsDeviceManager.PreferredBackBufferWidth / 1600.0f;
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

        //Switches for debug windows UWU
        public static bool InspectorVisible = true;
        public static bool HierarchyVisible = true;
        public static bool SceneSelectionVisible = true;
        public static bool CheatMenuVisible = true;
        public static bool ShowShadowMap = false;
        public static bool ShowSelectedSilhouettes = false;
        public static bool DrawShadows = true;
        public static bool DrawMeshes = true;
        public static bool DebugCamera = false;
        public static bool DrawWireframe = false;
        public static bool DrawSelectFrustum = false;
        public static bool DrawExplored = false;
        public static bool DrawVisibility = false;
#endif
    }
}
