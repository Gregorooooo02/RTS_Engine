using System.Collections.Generic;

namespace RTS_Engine;

public class SceneManager
{
    private readonly Stack<IScene> _scenes;

    public SceneManager()
    {
        _scenes = new Stack<IScene>();
    }

    public void AddScene(IScene scene)
    {
        scene.Initialize();
        _scenes.Push(scene);
    }

    public void RemoveScene() 
    {
        _scenes.Pop();
    }

    public IScene GetCurrentScene() 
    {
        return _scenes.Peek();
    }
}
