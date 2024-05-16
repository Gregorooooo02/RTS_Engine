using System.Collections.Generic;
using System.Linq;
using ImGuiNET;

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
            }
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
        if (worldRenderer != null) Globals.Renderer.WorldMesh = worldRenderer;
        
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
                _scenes.RemoveAt(sceneToClose);
                confirmationWindowOpen = false;
            }
            ImGui.SameLine();
            if (ImGui.Button("Save and Close"))
            {
                _scenes[sceneToClose].SaveToFile();
                if (CurrentScene == _scenes[sceneToClose]) CurrentScene = _scenes[0];
                _scenes.RemoveAt(sceneToClose);
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
                //TODO: Check if line below works on mac. Might not work because of '/' or '\' problem.
                string name = path.Substring(path.LastIndexOf('\\') + 1);
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
