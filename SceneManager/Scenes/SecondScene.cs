using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class SecondScene : IScene
{
    private List<GameObject> GameObjects;

    public void Initialize()
    {
        GameObjects = new List<GameObject>();
        GameObject gameObject = new GameObject();

        gameObject.AddComponent(new MeshRenderer(gameObject, Globals.Instance.monkeModel));
        gameObject.Transform.SetLocalRotation(new Vector3(-90, 0, 0));
        GameObjects.Add(gameObject);
    }

    public void Update(GameTime gameTime)
    {
        foreach (GameObject gameObject in GameObjects)
        {
            gameObject.Update();
        }
    }

    public void Draw()
    {
        foreach (GameObject gameObject in GameObjects)
        {
            gameObject.Draw();
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
