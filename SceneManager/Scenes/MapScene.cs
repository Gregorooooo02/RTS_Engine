using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;

namespace RTS_Engine;

public class MapScene : Scene
{
    GameObject gameObject;
    GameObject meshObject;

    public override void Initialize()
    {
        Name = "MapScene";
        SceneRoot = new GameObject();
        SceneRoot.Name = "Root";
        
        meshObject = new GameObject();
        meshObject.Name = "WorldMesh";
        meshObject.AddComponent<WorldRenderer>();
        meshObject.Transform.SetLocalPosition(new Vector3(-64, -40, -64));
        SceneRoot.AddChildObject(meshObject);
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

    public override void DrawHierarchy()
    {
        SceneRoot.DrawTree();
    }

    public override void SaveToFile()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine(SceneRoot.SaveSceneToXml());
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
