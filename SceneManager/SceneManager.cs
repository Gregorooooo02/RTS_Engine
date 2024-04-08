using System.Collections.Generic;
using ImGuiNET;

namespace RTS_Engine;

public class SceneManager
{
    public static SceneManager Instance;
    private List<Scene> _scenes;

    public SceneManager()
    {
        Instance = this;
        _scenes = new List<Scene>();
    }

    public void AddScene(Scene scene)
    {
        scene.Initialize();
        _scenes.Add(scene);
    }

    public void RemoveScene() 
    {
        _scenes.RemoveAt(_scenes.Count - 1);
    }

    public Scene GetCurrentScene() 
    {
        return _scenes[_scenes.Count - 1];
    }

    public void DrawSelection() {
        ImGui.Begin("Scene Selection");

        var currentScene = GetCurrentScene();
        var scenesCopy = new List<Scene>(_scenes);

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
        if (ImGui.Button("Save to file"))
        {
            currentScene.SaveToFile();
        }
        ImGui.End();
    }
}
