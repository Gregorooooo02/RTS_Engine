using System.IO;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class SecondScene : Scene
{   
    public override void Initialize()
    {
        Name = "SecondScene";
        SceneRoot = new GameObject();
    }

    public override void Update(GameTime gameTime)
    {
        SceneRoot.Update();
    }
    

    public override void Activate()
    {
        SceneRoot.Active = true;
    }

    public override void Deactivate()
    {
        SceneRoot.Active = false;
    }

    public override void AddGameObject(GameObject gameObject)
    {
        SceneRoot.AddChildObject(gameObject);
    }

    public override void RemoveGameObject(GameObject gameObject)
    {
        SceneRoot.RemoveChildObject(gameObject);
    }

#if DEBUG
    public override void DrawHierarchy()
    {
        SceneRoot.DrawTree();
    }
#endif

    public override void SaveToFile()
    {
        StringBuilder builder = new StringBuilder();
        // Append scene metadata here if necessary

        builder.Append(SceneRoot.SaveSceneToXml());
        XDocument scene = XDocument.Parse(builder.ToString());
#if _WINDOWS
        StreamWriter streamWriter = new StreamWriter("../../../Scenes/" + Name + ".xml");
#else
        StreamWriter streamWriter = new StreamWriter("Scenes/" + Name + ".xml");
#endif
        scene.Save(streamWriter);
        streamWriter.Close();
    }
}
