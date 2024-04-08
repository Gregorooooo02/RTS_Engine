using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class BaseScene : IScene
{
    public string Name = "BaseScene";
    private List<GameObject> GameObjects;
    public int number = 1;

    GameObject background;
    GameObject coin;
    GameObject smiley;
    GameObject text;

    public void Initialize() {
        GameObjects = new List<GameObject>();
        
        background = new GameObject();

        coin = new GameObject();
        coin.AddComponent<AnimatedSpriteRenderer>();
        coin.Transform.SetLocalPosition(new Vector3(680, 450, 0));
        coin.Transform.SetLocalScale(new Vector3(0.45f, 3.0f, 1.0f));
        GameObjects.Add(coin);

        smiley = new GameObject();
        smiley.AddComponent<SpiteRenderer>();
        smiley.Transform.SetLocalScale(new Vector3(0.25f, 0.25f, 0.25f));
        GameObjects.Add(smiley);

        text = new GameObject();
        text.AddComponent<TextRenderer>();
        text.Transform.SetLocalPosition(new Vector3(15, 800, 0));
        GameObjects.Add(text);
    }

    public void Update(GameTime gameTime)
    {
        text.GetComponent<TextRenderer>().NewContent = "Number count: " + number;
        foreach (GameObject gameObject in GameObjects)
        {
            gameObject.Update();
        }
        number++;
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
