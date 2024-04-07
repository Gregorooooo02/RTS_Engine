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

        gameObject.AddComponent<SpiteRenderer>();
        gameObject.Transform.SetLocalScale(new Vector3(0.25f, 0.25f, 0.25f));
        gameObject.Transform.SetLocalRotation(new Vector3(-90, 0, 0));
        gameObject.Transform.SetLocalPosition(new Vector3(1050, 0, 0));
        gameObject.Name = "Sprite";
        GameObjects.Add(gameObject);

        GameObject textObject = new GameObject();

        textObject.AddComponent<TextRenderer>();
        textObject.Transform.SetLocalPosition(new Vector3(620, 450, 0));
        textObject.Transform.SetLocalScale(new Vector3(1f, 1f, 1f));
        textObject.Name = "Text";

        GameObjects.Add(textObject);
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
