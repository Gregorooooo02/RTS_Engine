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

        GenerateMap.GenerateNoiseTexture();

        gameObject = new GameObject();
        gameObject.Name = "MapTexture";
        gameObject.Transform.SetLocalScale(new Vector3(2, 2, 1));
        gameObject.AddComponent<SpiteRenderer>();
        gameObject.GetComponent<SpiteRenderer>().Sprite = GenerateMap.noiseTexture;
        gameObject.Transform.SetLocalPosition(new Vector3(0, 0, 0));
        SceneRoot.AddChildObject(gameObject);

        meshObject = new GameObject();
        meshObject.Name = "WorldMesh";
        meshObject.AddComponent<WorldRenderer>();
        meshObject.Transform.SetLocalPosition(new Vector3(-64, -20, -64));
        SceneRoot.AddChildObject(meshObject);
    }

    public override void Update(GameTime gameTime)
    {
        gameObject.GetComponent<SpiteRenderer>().Sprite = GenerateMap.noiseTexture;
        SceneRoot.Update();
    }

    public override void Draw(Matrix _view, Matrix _projection)
    {
        SceneRoot.Draw(_view, _projection);
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
