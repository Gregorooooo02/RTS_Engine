using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class AssetManager
{
    private static AssetManager Instance;
    private ContentManager content;

    private List<ModelData> _models;
    private List<SpriteData> _sprites;
    private List<FontData> _fonts;

    public static Model DefaultModel;
    public static Texture2D DefaultSprite;
    public static SpriteFont DefaultFont;

    private class ModelData
    {
        public Model Model;
        public int Uses;
        public string Name;

        public ModelData(Model model,string name)
        {
            Model = model;
            Name = name;
        }
    }
    
    private class SpriteData
    {
        public Texture2D Sprite;
        public int Uses;
        public string Name;

        public SpriteData(Texture2D sprite,string name)
        {
            Sprite = sprite;
            Name = name;
        }
    }
    
    private class FontData
    {
        public SpriteFont Font;
        public int Uses;
        public string Name;

        public FontData(SpriteFont font,string name)
        {
            Font = font;
            Name = name;
        }
    }
    
    public static void Initialize(ContentManager content)
    {
        Instance = new AssetManager(content);
    }

    private AssetManager(ContentManager content)
    {
        this.content = content;
        _models = new List<ModelData>();
        _sprites = new List<SpriteData>();
        _fonts = new List<FontData>();

        DefaultModel = this.content.Load<Model>("defaultCube");
        DefaultSprite = this.content.Load<Texture2D>("smile");
        DefaultFont = this.content.Load<SpriteFont>("defaultFont");
    }

    public static Model GetModel(string name)
    {
        Model temp = Instance._models.Find(x => x.Name == name)?.Model;
        if (temp == null)
        {
            Instance._models.Add(new ModelData(Instance.content.Load<Model>(name),name));
            Instance._models.Last().Uses++;
            temp = Instance._models.Last().Model;
        }
        return temp;
    }

    public static Texture2D GetSprite(string name)
    {
        Texture2D temp = Instance._sprites.Find(x => x.Name == name)?.Sprite;
        if (temp == null)
        {
            temp = Instance.content.Load<Texture2D>(name);
            Instance._sprites.Add(new SpriteData(temp, name));
        }
        return temp;
    }

    public static SpriteFont GetFont(string name)
    {
        SpriteFont temp = Instance._fonts.Find(x => x.Name == name)?.Font;
        if (temp == null)
        {
            temp = Instance.content.Load<SpriteFont>(name);
            Instance._fonts.Add(new FontData(temp,name));
        }
        return temp;
    }

    public static void FreeModel(Model model)
    {
        ModelData data = Instance._models.Find(x => x.Model == model);
        if(data == null) return;
        
        data.Uses--;
        if (data.Uses == 0)
        {
            Instance._models.Remove(data);
        }
    }

    public static int test()
    {
        return Instance._models.Count;
    }
}