using Microsoft.Xna.Framework;

namespace RTS_Engine;

public abstract class Scene
{
    public string Name;
    public GameObject SceneRoot;

    public abstract void Initialize();
    public abstract void Update(GameTime gameTime);
    public abstract void Activate();
    public abstract void Deactivate();
    public abstract void AddGameObject(GameObject gameObject);
    public abstract void RemoveGameObject(GameObject gameObject);
    public abstract void DrawHierarchy();
    public abstract void SaveToFile();
}
