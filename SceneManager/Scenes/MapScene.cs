using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class MapScene : Scene
{
    NoiseField<float> perlinNoise;
    Texture2D noiseTexture;

    public void GenerateNoiseTexture()
    {
        PerlinNoiseGenerator perlinGen = new PerlinNoiseGenerator();
        perlinGen.Octaves = 4;
        perlinGen.Persistance = 0.5f;
        perlinGen.Interpolation = Helpers.CosInterpolation;

        perlinNoise = perlinGen.GeneratePerlinNoise(512, 512);

        CustomGradientFilter filter = new CustomGradientFilter();
        Texture2DTransformer transformer = new Texture2DTransformer(Globals.Instance.GraphicsDevice);

        filter.AddColorPoint(0.0f, 0.4f, Color.RoyalBlue);
        filter.AddColorPoint(0.4f, 0.5f, new Color(255, 223, 135));
        filter.AddColorPoint(0.5f, 0.7f, new Color(117, 255, 89));
        filter.AddColorPoint(0.7f, 0.9f, new Color(117, 105, 89));
        filter.AddColorPoint(0.9f, 1.0f, Color.White);

        noiseTexture = transformer.Transform(filter.Filter(perlinNoise));
    }

    public override void Initialize()
    {
        Name = "BaseScene";
        SceneRoot = new GameObject();

        GenerateNoiseTexture();

        GameObject gameObject = new GameObject();
        gameObject.AddComponent<SpiteRenderer>();
        gameObject.GetComponent<SpiteRenderer>().Sprite = noiseTexture;
        gameObject.Transform.SetLocalPosition(new Vector3(500, 200, 0));
        SceneRoot.AddChildObject(gameObject);
    }

    public override void Update(GameTime gameTime)
    {
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
        StreamWriter streamWriter = new StreamWriter("../../../SceneManager/Scenes/" + Name + ".xml");
#else
        StreamWriter streamWriter = new StreamWriter("SceneManager/Scenes/" + Name + ".xml");
#endif
        scene.Save(streamWriter);
        streamWriter.Close();
    }
}
