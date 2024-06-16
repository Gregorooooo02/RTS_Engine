using System.Collections.Generic;
using System.Diagnostics;
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

    public void CreateMissionScene()
    {
        Scene missionScene = new LoadedScene();
        
        missionScene.Name = "MissionScene";
        Debug.WriteLine("Created scene");
        
        GameObject missionRoot = new GameObject();
        missionScene.SceneRoot = missionRoot;
        Debug.WriteLine("Created scene root");
        missionRoot.Name = "Root";
        missionRoot.AddComponent<WorldRenderer>();

        var currentWorld = missionRoot.GetComponent<WorldRenderer>();
        
        Debug.WriteLine("Added World Renderer");

        GameObject camera = new GameObject();
        camera.Name = "Camera";
        missionRoot.AddChildObject(camera);
        camera.AddComponent<Camera>();
        camera.Transform.SetLocalPosition(new Vector3(120, 50, 160));
        Debug.WriteLine("Added Camera");
        
#if _WINDOWS
        missionRoot.LoadPrefab(Globals.MainPath + "/Prefabs/UI.xml");
#else
        missionRoot.LoadPrefab("Prefabs/UI.xml");
#endif

        GameObject civilians = new GameObject();
        civilians.Name = "Civilians";
        missionRoot.AddChildObject(civilians);
        for (int i = 0; i < 15; i++)
        {
#if _WINDOWS
            civilians.LoadPrefab(Globals.MainPath + "/Prefabs/Civilian.xml");
#else
            civilians.LoadPrefab("Prefabs/Civilian.xml");
#endif
        }
        Debug.WriteLine("Added Civilians");

        GameObject chairs = new GameObject();
        chairs.Name = "Chairs";
        missionRoot.AddChildObject(chairs);
        for (int i = 0; i < 5; i++)
        {
#if _WINDOWS
            chairs.LoadPrefab(Globals.MainPath + "/Prefabs/Chair.xml");
#else
            chairs.LoadPrefab("Prefabs/Chair.xml");
#endif
            
            Vector3 chairPos = chairs.Children.Last().Transform.Pos;
            Vector2 posXZ = new(chairPos.X, chairPos.Z + 2 * i);
            
            var height = PickingManager.InterpolateWorldHeight(posXZ, currentWorld);
            chairs.Children.Last().Transform.Move(new Vector3(0, height,2 * i));
        }
        
        Debug.WriteLine("Added units");
        AddScene(missionScene);
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
                ChangeScene(i);
                Globals.AgentsManager.Units.Clear();
                Globals.PickingManager.SinglePickingActive = false;
                Globals.PickingManager.BoxPickingActive = false;
                Globals.PickingManager.GroundPickingActive = false;
                Globals.PickingManager.EnemyPickingActive = false;
            }
        }
        
        if (InputManager.Instance.GetAction(GameAction.CREATE_MISSION)?.state == ActionState.RELEASED)
        {
            Globals.PickingManager.SinglePickingActive = true;
            Globals.PickingManager.BoxPickingActive = true;
            Globals.PickingManager.GroundPickingActive = true;
            Globals.PickingManager.EnemyPickingActive = true;
            CreateMissionScene();
            ChangeScene(2);
            Globals.AgentsManager.Initialize();
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
