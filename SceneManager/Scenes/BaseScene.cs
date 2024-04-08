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
        background.AddComponent<AnimatedSpriteRenderer>();
        background.GetComponent<AnimatedSpriteRenderer>().SetSpriteSheet(AssetManager.Background);
        background.GetComponent<AnimatedSpriteRenderer>().SetFrames(12);
        background.GetComponent<AnimatedSpriteRenderer>().SetFrameTime(0.05f);
        background.Transform.SetLocalScale(new Vector3(1.0f, 10.0f, 1.0f));
        background.Transform.SetLocalPosition(new Vector3(0, 0, 0));


        GameObject coin = new GameObject();
        coin.AddComponent<AnimatedSpriteRenderer>();
        coin.GetComponent<AnimatedSpriteRenderer>().SetSpriteSheet(AssetManager.DefaultAnimatedSprite);
        coin.GetComponent<AnimatedSpriteRenderer>().SetFrames(6);
        coin.GetComponent<AnimatedSpriteRenderer>().SetFrameTime(0.1f);
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
#if _WINDOWS
        StreamWriter streamWriter = new StreamWriter("../../../SceneManager/Scenes/" + Name + ".xml");
#else
        StreamWriter streamWriter = new StreamWriter("SceneManager/Scenes/" + Name + ".xml");
#endif
        scene.Save(streamWriter);
        streamWriter.Close();
    }
}
