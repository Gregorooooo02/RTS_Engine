﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class AssetManager
{
    private static AssetManager _instance;
    private readonly ContentManager _content;

    private readonly List<ModelData> _models;
    private readonly List<SpriteData> _sprites;
    private readonly List<FontData> _fonts;

    public static Model DefaultModel {get; private set;}
    public static Texture2D DefaultSprite{get; private set;}
    public static Texture2D DefaultAnimatedSprite{get; private set;}
    public static SpriteFont DefaultFont{get; private set;}

    private class ModelData
    {
        public readonly Model Model;
        public int Uses;
        public readonly string Name;

        public ModelData(Model model,string name)
        {
            Model = model;
            Name = name;
        }
    }
    
    private class SpriteData
    {
        public readonly Texture2D Sprite;
        public int Uses;
        public readonly string Name;

        public SpriteData(Texture2D sprite,string name)
        {
            Sprite = sprite;
            Name = name;
        }
    }
    
    private class FontData
    {
        public readonly SpriteFont Font;
        public int Uses;
        public readonly string Name;

        public FontData(SpriteFont font,string name)
        {
            Font = font;
            Name = name;
        }
    }
    
    public static void Initialize(ContentManager content)
    {
        _instance = new AssetManager(content);
    }

    private AssetManager(ContentManager content)
    {
        this._content = content;
        _models = new List<ModelData>();
        _sprites = new List<SpriteData>();
        _fonts = new List<FontData>();

        DefaultModel = this._content.Load<Model>("defaultCube");
        DefaultSprite = this._content.Load<Texture2D>("smile");
        DefaultAnimatedSprite = this._content.Load<Texture2D>("coin");
        DefaultFont = this._content.Load<SpriteFont>("defaultFont");
    }

    public static Model GetModel(string name)
    {
        ModelData temp = _instance._models.Find(x => x.Name == name);
        if (temp == null)
        {
            _instance._models.Add(new ModelData(_instance._content.Load<Model>(name),name));
            _instance._models.Last().Uses++;
            return _instance._models.Last().Model;
        }
        temp.Uses++;
        return temp.Model;
    }

    public static Texture2D GetSprite(string name)
    {
        SpriteData temp = _instance._sprites.Find(x => x.Name == name);
        if (temp == null)
        {
            _instance._sprites.Add(new SpriteData(_instance._content.Load<Texture2D>(name), name));
            _instance._sprites.Last().Uses++;
            return _instance._sprites.Last().Sprite;
        }

        temp.Uses++;
        return temp.Sprite;
    }

    public static SpriteFont GetFont(string name)
    {
        FontData temp = _instance._fonts.Find(x => x.Name == name);
        if (temp == null)
        {
            _instance._fonts.Add(new FontData(_instance._content.Load<SpriteFont>(name),name));
            _instance._fonts.Last().Uses++;
            return _instance._fonts.Last().Font;
        }

        temp.Uses++;
        return temp.Font;
    }

    public static void FreeModel(Model model)
    {
        ModelData data = _instance._models.Find(x => x.Model == model);
        if(data == null) return;
        
        data.Uses--;
        if (data.Uses == 0)
        {
            _instance._models.Remove(data);
        }
    }

    public static void FreeSprite(Texture2D sprite)
    {
        SpriteData data = _instance._sprites.Find(x => x.Sprite == sprite);
        if(data == null) return;
        data.Uses--;
        if (data.Uses == 0)
        {
            _instance._sprites.Remove(data);
        }
    }

    public static void FreeFont(SpriteFont font)
    {
        FontData data = _instance._fonts.Find(x => x.Font == font);
        if(data == null) return;
        data.Uses--;
        if (data.Uses == 0)
        {
            _instance._fonts.Remove(data);
        }
    }
}