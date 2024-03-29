using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace RTS_Engine;

public class BaseScene : IScene
{
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

    public void Draw()
    {
        foreach (GameObject gameObject in GameObjects)
        {
            gameObject.Draw();
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
