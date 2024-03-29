using System.Collections.Generic;
using ImGuiNET;

namespace RTS_Engine;

public class SceneManager
{
    public static SceneManager Instance;
    private List<IScene> _scenes;

    public SceneManager()
    {
        Instance = this;
        _scenes = new List<IScene>();
    }

    public void AddScene(IScene scene)
    {
        scene.Initialize();
        _scenes.Add(scene);
    }

    public void RemoveScene() 
    {
        _scenes.RemoveAt(_scenes.Count - 1);
    }

    public IScene GetCurrentScene() 
    {
        return _scenes[_scenes.Count - 1];
    }

    public void DrawSelection() {
        ImGui.Begin("Scene Selection");

        var currentScene = GetCurrentScene();
        var scenesCopy = new List<IScene>(_scenes);

        if (ImGui.Button("Add Scene"))
        {
            AddScene(new BaseScene());
        }
        ImGui.SameLine();
        if (ImGui.Button("Remove Scene"))
        {
            RemoveScene();
        }

        ImGui.Text($"Current Scene: {currentScene.GetType().Name}");
        ImGui.Separator();
        for (int i = scenesCopy.Count; i > 0; i--)
        {
            var scene = scenesCopy[i - 1];
            if (ImGui.Button(scene.GetType().Name))
            {
                _scenes.Remove(scene);
                _scenes.Add(scene);
            }
        }
        ImGui.End();
    }
}
