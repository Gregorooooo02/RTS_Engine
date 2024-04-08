using System.IO;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class BaseScene : IScene
{
    public string Name = "BaseScene";
    public int number = 1;
    
    private GameObject SceneRoot;

    public void Initialize()
    {
        SceneRoot = new GameObject();
        
        GameObject background = new GameObject();
        SceneRoot.AddChildObject(background);

        GameObject coin = new GameObject();
        coin.AddComponent<AnimatedSpriteRenderer>();
        coin.Transform.SetLocalPosition(new Vector3(680, 450, 0));
        coin.Transform.SetLocalScale(new Vector3(0.45f, 3.0f, 1.0f));
        SceneRoot.AddChildObject(coin);

        GameObject smiley = new GameObject();
        smiley.AddComponent<SpiteRenderer>();
        smiley.Transform.SetLocalScale(new Vector3(0.25f, 0.25f, 0.25f));
        SceneRoot.AddChildObject(smiley);

        GameObject text = new GameObject();
        text.AddComponent<TextRenderer>();
        text.Transform.SetLocalPosition(new Vector3(15, 800, 0));
        SceneRoot.AddChildObject(text);
    }

    public void Update(GameTime gameTime)
    {
        SceneRoot.Children[3].GetComponent<TextRenderer>().NewContent = "Number count: " + number;
        SceneRoot.Update();
        number++;
    }

    public void Draw(Matrix _view, Matrix _projection)
    {
        SceneRoot.Draw(_view,_projection);
    }

    public void Activate()
    {
        SceneRoot.Active = true;
    }

    public void Deactivate()
    {
        SceneRoot.Active = false;
    }

    public void AddGameObject(GameObject gameObject)
    {
        SceneRoot.AddChildObject(gameObject);
    }

    public void RemoveGameObject(GameObject gameObject)
    {
        SceneRoot.RemoveChildObject(gameObject);
    }

    public void DrawHierarchy()
    {
        SceneRoot.DrawTree();
    }

    public void SaveToFile()
    {
        StringBuilder builder = new StringBuilder();
        //Append scene metadata here if necessary
        
        builder.Append(SceneRoot.SaveSceneToXml());
        XDocument scene = XDocument.Parse(builder.ToString());
        StreamWriter streamWriter = new StreamWriter("../../../SceneManager/Scenes/" + Name + ".xml");
        scene.Save(streamWriter);
        streamWriter.Close();
    }
}
