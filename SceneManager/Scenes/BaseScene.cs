using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class BaseScene : IScene
{
    public string Name = "BaseScene";
    private List<GameObject> GameObjects;

    public void Initialize() {
        GameObjects = new List<GameObject>();
        GameObject gameObject = new GameObject();

        gameObject.AddComponent(new MeshRenderer(gameObject, Globals.Instance.defaultModel));
        GameObjects.Add(gameObject);
    }

    public void Update(GameTime gameTime)
    {
        foreach (GameObject gameObject in GameObjects)
        {
            gameObject.Update();
        }
    }

    public void Draw(Matrix _view, Matrix _projection)
    {
        foreach (GameObject gameObject in GameObjects)
        {
            gameObject.Draw(_view, _projection);
        }
    }

    public void Activate()
    {
        foreach (GameObject gameObject in GameObjects)
        {
            gameObject.Active = true;
        }
    }

    public void Deactivate()
    {
        foreach (GameObject gameObject in GameObjects)
        {
            gameObject.Active = false;
        }
    }

    public void AddGameObject(GameObject gameObject)
    {
        GameObjects.Add(gameObject);
    }

    public void RemoveGameObject(GameObject gameObject)
    {
        GameObjects.Remove(gameObject);
    }

    public void DrawHierarchy()
    {
        foreach (GameObject gameObject in GameObjects)
        {
            gameObject.DrawTree();
        }
    }
}
