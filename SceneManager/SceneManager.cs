using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class SceneManager
{
    private GameAction[] sceneChangeActions = {GameAction.SCENE0, GameAction.SCENE1, GameAction.SCENE2, GameAction.SCENE3 };
    public static SceneManager Instance;
    private List<Scene> _scenes;
    public Scene CurrentScene = null;

    public SceneManager()
    {
        Instance = this;
        _scenes = new List<Scene>();
    }

    public void CreateMissionScene(bool isTutorial = false)
    {
            Globals.RegenerateWorld = false;
            Console.WriteLine("Mission generation start!");
            GameObject missionRoot = new GameObject();
            missionRoot.AddComponent<WorldRenderer>();

            ChangeScene(1);

            Console.WriteLine("Launching new task!");
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                    try
                    {
                        Scene missionScene = new LoadedScene();
                        missionScene.IsMissionScene = true;
                        missionScene.Name = "MissionScene";
                        Console.WriteLine("Created scene");
                        missionScene.SceneRoot = missionRoot;
                        Console.WriteLine("Created scene root");
                        missionRoot.Name = "Root";

                        var currentWorld = missionRoot.GetComponent<WorldRenderer>();

                        bool result = currentWorld.GenerateWorld(isTutorial);
                        if (result)
                        {
                            Console.WriteLine("Created World");

                            GameObject camera = new GameObject();
                            camera.Name = "Camera";
                            missionRoot.AddChildObject(camera);
                            camera.AddComponent<Camera>();
                            Camera cameraComponent = camera.GetComponent<Camera>();
                            cameraComponent.IsWorldCamera = true;
                            camera.Transform.SetLocalPosition(new Vector3(120, 50, 160));
                            if (Globals.AgentsManager.Units.Count > 0)
                                cameraComponent.MoveCameraToPosition(
                                    Globals.AgentsManager.Units[0].ParentObject.Transform.Pos, currentWorld);
                            Console.WriteLine("Added Camera");
                            if (isTutorial)
                            {
#if _WINDOWS
                                missionRoot.LoadPrefab(Globals.MainPath + "/Prefabs/TutorialUI.xml");
                                missionRoot.LoadPrefab(Globals.MainPath + "/Prefabs/UI.xml");
#else
                                missionRoot.LoadPrefab("Prefabs/TutorialUI.xml");
                                missionRoot.LoadPrefab("Prefabs/UI.xml");
#endif
                            }
                            else
                            {
#if _WINDOWS
                                missionRoot.LoadPrefab(Globals.MainPath + "/Prefabs/UI.xml");
#else
                                missionRoot.LoadPrefab("Prefabs/UI.xml");
#endif
                            }
#if _WINDOWS
                            missionRoot.LoadPrefab(Globals.MainPath + "/Prefabs/Marker.xml");
#else
                            missionRoot.LoadPrefab("Prefabs/Marker.xml");
#endif
                        
#if _WINDOWS
                            missionRoot.LoadPrefab(Globals.MainPath + "/Prefabs/MissionMusic.xml");
#else
                            missionRoot.LoadPrefab("Prefabs/MissionMusic.xml");
#endif
                            
                            AddScene(missionScene);

                            Globals.PickingManager.SinglePickingActive = true;
                            Globals.PickingManager.BoxPickingActive = true;
                            Globals.PickingManager.GroundPickingActive = true;
                            Globals.PickingManager.EnemyPickingActive = true;
                            Globals.FogManager.FogActive = true;
                            ChangeScene(_scenes.Count - 1);
                            Globals.AgentsManager.Initialize();
                            Globals.CreatingTutorial = false;
                        }
                        else
                        {
                            Console.WriteLine("Scheduling regeneration!");
                            Globals.RegenerateWorld = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
            });
    }

    public void AddScene(Scene scene)
    {
        scene.Initialize();
        _scenes.Add(scene);
        if (CurrentScene == null)
        {
            ChangeScene(0);
        }
    }
    public void CheckForSceneChanges()
    {
        for (int i = 0;i < sceneChangeActions.Length;i++) {
            if (InputManager.Instance.GetAction(sceneChangeActions[i])?.state == ActionState.RELEASED)
            {
                if (CurrentScene.IsMissionScene)
                {
                    RemoveScene(CurrentScene);
                    Globals.AgentsManager.ClappedCivilians.Clear();
                    Globals.AgentsManager.ClappedBuildings.Clear();
                    Globals.AgentsManager.ClappedSoldiers.Clear();
                    Globals.AgentsManager.SelectedUnits.Clear();
                    GameManager.CurrentAwareness = 0;
                }
                Globals.AgentsManager.Units.Clear();
                Globals.PickingManager.SinglePickingActive = false;
                Globals.PickingManager.BoxPickingActive = false;
                Globals.PickingManager.GroundPickingActive = false;
                Globals.PickingManager.EnemyPickingActive = false;
                Globals.FogManager.FogActive = false;
                Globals.UIActive = false;
                
                GameManager.TransferRewards();
                GameManager.ClearMissionRewards();
                
                ChangeScene(i);
            }
        }
        
        if (InputManager.Instance.GetAction(GameAction.CREATE_MISSION)?.state == ActionState.RELEASED)
        {
            CreateMissionScene();
            
            Globals.GraphicsDevice.SetRenderTarget(Globals.FogManager.PermanentMaskTarget);
            Globals.GraphicsDevice.Clear(new Color(0,0,0,255));
            Globals.GraphicsDevice.SetRenderTarget(null);
        }
        
        if (InputManager.Instance.GetAction(GameAction.CREATE_TUTORIAL)?.state == ActionState.RELEASED)
        {
            CreateMissionScene(true);
            
            Globals.GraphicsDevice.SetRenderTarget(Globals.FogManager.PermanentMaskTarget);
            Globals.GraphicsDevice.Clear(new Color(0,0,0,255));
            Globals.GraphicsDevice.SetRenderTarget(null);
            GameManager.TutorialDone = true;
            Globals.UIActive = true;
        }

        if (InputManager.Instance.GetAction(GameAction.RESET)?.state == ActionState.RELEASED)
        {
            //Reset camp scene
            RemoveScene(2);
            AddScene(FileManager.PopulateScene("BaseScene"));
            
            //Reset player stats
            Globals.CreatingTutorial = true;
            GameManager.MeatNumber = 0;
            GameManager.PuzzleNumber = 0;
            GameManager.CurrentAwareness = 0;
            GameManager.DamageMultiplier = 1.0f;
            GameManager.HealthMultiplier = 1.0f;
            GameManager.UnitsSelectedForMission = 0;
            GameManager.TutorialDone = false;
            GameManager.MissionTutorialStep = 1;
            GameManager.ClearMissionRewards();
            //TODO: Add any necessary resets here
        }
    }

    public void ChangeScene(int i)
    {
        Globals.Renderer.Clear();
        CurrentScene = _scenes[i];
        OnLoad(CurrentScene.SceneRoot);
    }

    private void OnLoad(GameObject gameObject)
    {
        //World Renderer
        WorldRenderer worldRenderer = gameObject.GetComponent<WorldRenderer>();
        if (worldRenderer != null) Globals.Renderer.WorldRenderer = worldRenderer;
        
        //Spite Renderer
        SpiteRenderer spiteRenderer = gameObject.GetComponent<SpiteRenderer>();
        if(spiteRenderer != null) Globals.Renderer.Sprites.Add(spiteRenderer);
        
        //Text Renderer
        TextRenderer textRenderer = gameObject.GetComponent<TextRenderer>();
        if(textRenderer != null) Globals.Renderer.Texts.Add(textRenderer);
        
        //AnimatedSpriteRenderer
        AnimatedSpriteRenderer animatedSpriteRenderer = gameObject.GetComponent<AnimatedSpriteRenderer>();
        if(animatedSpriteRenderer != null) Globals.Renderer.AnimatedSprites.Add(animatedSpriteRenderer);

        InstancedRendererController instancedRendererController =
            gameObject.GetComponent<InstancedRendererController>();
        if (instancedRendererController != null) Globals.Renderer.InstancedRendererControllers.Add(instancedRendererController);

        foreach (GameObject objectChild in gameObject.Children)
        {
            OnLoad(objectChild);
        }
    }

    public void RemoveScene(Scene scene)
    {
        GameObject.ClearObject(scene.SceneRoot);
        _scenes.Remove(scene);
    }
    
    public void RemoveScene(int index)
    {
        GameObject.ClearObject(_scenes[index].SceneRoot);
        _scenes.RemoveAt(index);
    }
    
#if DEBUG
    private bool confirmationWindowOpen = false;
    private int sceneToClose;

    private bool loadingScene = false;
    
    public void DrawSelection() {
        ImGui.Begin("Scene Selection");
        ImGui.Text("Current scene: " + CurrentScene.Name);
        ImGui.Separator();
        if (ImGui.Button("New scene"))
        {
            AddScene(new LoadedScene());
            _scenes.Last().Name = "Scene#" + _scenes.Count;
            _scenes.Last().TempName = "Scene#" + _scenes.Count;
        }
        ImGui.SameLine();
        if (ImGui.Button("Load scene"))
        {
            loadingScene = true;
            Globals.UpdateScenesList();
        }
        ImGui.Separator();
        for (int i = 0; i < _scenes.Count; i++)
        {
            if (ImGui.CollapsingHeader(_scenes[i].Name))
            {
                ImGui.InputText("Scene name",ref _scenes[i].TempName, 25);
                ImGui.SameLine();
                if(ImGui.Button("Apply name"))
                {
                    _scenes[i].Name = _scenes[i].TempName;
                }
                if (ImGui.Button("Select " + _scenes[i].Name + " scene"))
                {
                    ChangeScene(i);
                }
                ImGui.SameLine();
                if (ImGui.Button("Remove " + _scenes[i].Name + " scene"))
                {
                    if (_scenes.Count > 1)
                    {
                        confirmationWindowOpen = true;
                        sceneToClose = i;
                    }
                }
            }
            ImGui.Separator();
        }
        
        if (ImGui.Button("Save to file"))
        {
            CurrentScene.SaveToFile();
        }

        if (confirmationWindowOpen)
        {
            ImGui.Begin("Close scene?");
            if (ImGui.Button("Close"))
            {
                if (CurrentScene == _scenes[sceneToClose]) CurrentScene = _scenes[0];
                RemoveScene(sceneToClose);
                confirmationWindowOpen = false;
            }
            ImGui.SameLine();
            if (ImGui.Button("Save and Close"))
            {
                _scenes[sceneToClose].SaveToFile();
                if (CurrentScene == _scenes[sceneToClose]) CurrentScene = _scenes[0];
                RemoveScene(sceneToClose);
                confirmationWindowOpen = false;
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                confirmationWindowOpen = false;
            }
            ImGui.End();
        }

        if (loadingScene)
        {
            ImGui.Begin("Load scene");
            foreach (string path in Globals.AvailableScenes)
            { 
#if _WINDOWS
                string name = path.Substring(path.LastIndexOf('\\') + 1);
#else
                string name = path.Substring(path.LastIndexOf('/') + 1);
#endif
                name = name[..^4];
                if (ImGui.Button(name))
                {
                    loadingScene = false;
                    Scene scene = new LoadedScene();
                    scene.Name = name;
                    scene.TempName = name;
                    scene.SceneRoot = FileManager.DeserializeScene(path);
                    AddScene(scene);
                }
            }

            if (ImGui.Button("Cancel"))
            {
                loadingScene = false;
            }
            ImGui.End();
        }
        ImGui.End();
    }
    #endif
}
