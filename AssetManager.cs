using System.Collections.Generic;
#if DEBUG
using System;
using System.IO;
#endif
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Animation;

namespace RTS_Engine;

public class AssetManager
{
#if DEBUG
#region Loading asset names
    public static List<string> ModelPaths;
    public static List<string> SpriteNames;
    public static List<string> FontNames;


    private static readonly string[] ModelFormats = { "fbx", "obj" };
    private static readonly string[] SpriteFormats = { "jpg", "png", "bmp" };
    private static void LoadNames()
    {
#if _WINDOWS
        StreamReader sr = new StreamReader("../../../Content/Content.mgcb");
#else
        StreamReader sr = new StreamReader("Content/Content.mgcb");
#endif
        string line = sr.ReadLine();
        while (line != null)
        {
            if (line.Contains("/build:"))
            {
                line = line.Substring(7);
                int separatorIndex = line.IndexOf(";");
                if (separatorIndex != -1) line = line.Substring(separatorIndex + 1);
                string[] result = line.Split('.');
                if(result[1].Equals("spritefont")) FontNames.Add(result[0]);
                else if (ModelFormats.Contains(result[1]))
                {
                    int index = result[0].LastIndexOf('/');
                    if(index != -1)result[0] = result[0].Substring(0,index);
                    int k; 
                    string ending = result[0].Substring(result[0].LastIndexOf('/') + 1);
                    if (int.TryParse(ending,out k))
                    {
                        result[0] = result[0].Substring(0,result[0].LastIndexOf('/'));
                        if(!ModelPaths.Contains(result[0])) ModelPaths.Add(result[0]);
                    }
                    else
                    {
                        ModelPaths.Add(result[0]);
                    }
                    
                }
                else if (SpriteFormats.Contains(result[1])) SpriteNames.Add(result[0]);
            }
            line = sr.ReadLine();
        }
        sr.Close();
    }
#endregion
#endif
    
    private static AssetManager _instance;
    private ContentManager _content;

    private readonly List<ModelPointer> _models;
    private readonly List<AnimatedModelPointer> _animatedModels;
    private readonly List<SpriteData> _sprites;
    private readonly List<FontData> _fonts;

    public static ModelData DefaultModel {get; private set;}
    public static AnimatedModelData DefaultAnimatedModel { get; private set; }
    public static List<Texture2D> DefaultTextureMaps { get; private set; }
    public static Texture2D DefaultSprite{get; private set;}
    public static Texture2D DefaultAnimatedSprite{get; private set;}
    public static Texture2D DefaultHeightMap{get; private set;}
    public static Texture2D DefaultWaveNormalMap{get; private set;}
    public static List<Texture2D> DefaultHeightMaps{get; private set;}
    public static List<Texture2D> DefaultTerrainTextrues{get; private set;}
    public static TextureCube DefaultSkybox{get; private set;}
    public static SpriteFont DefaultFont{get; private set;}
    
    // Bloom effects
    public static Effect BloomExtractEffect { get; private set; }
    public static Effect BloomCombineEffect { get; private set; }
    public static Effect GaussianBlurEffect { get; private set; }

    private class ModelPointer
    {
        public readonly ModelData ModelData;
        public int Uses;

        public ModelPointer(ContentManager manager, string modelPath)
        {
            ModelData = new ModelData(manager, modelPath);
        }
    }

    private class AnimatedModelPointer
    {
        public readonly AnimatedModelData AnimatedModelData;
        public int Uses;

        public AnimatedModelPointer(ContentManager manager, string modelPath)
        {
            AnimatedModelData = new AnimatedModelData(manager, modelPath);
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
        _models = new List<ModelPointer>();
        _animatedModels = new List<AnimatedModelPointer>();
        _sprites = new List<SpriteData>();
        _fonts = new List<FontData>();
        
#if DEBUG
        ModelPaths = new List<string>();
        SpriteNames = new List<string>();
        FontNames = new List<string>();
        
        LoadNames();
#endif
        DefaultModel = new ModelData(this._content,"defaultModel");
        DefaultTextureMaps = DefaultModel.Textures[0][0];
        
        DefaultAnimatedModel = new AnimatedModelData(this._content, "minion");
        DefaultSprite = this._content.Load<Texture2D>("smile");
        DefaultAnimatedSprite = this._content.Load<Texture2D>("coin");
        DefaultHeightMap = this._content.Load<Texture2D>("heightmap");
        DefaultWaveNormalMap = this._content.Load<Texture2D>("TerrainTextures/woda/wave0");
        
        //DefaultHeightMaps = new List<Texture2D>
        //{
        //    this._content.Load<Texture2D>("heightmap_0"),
        //    this._content.Load<Texture2D>("heightmap_1"),
        //    this._content.Load<Texture2D>("heightmap_2"),
        //    this._content.Load<Texture2D>("heightmap_3"),
        //};

        DefaultTerrainTextrues = new List<Texture2D>
        {
            this._content.Load<Texture2D>("TerrainTextures/woda/albedo"),
            this._content.Load<Texture2D>("TerrainTextures/piasek/albedo"),
            this._content.Load<Texture2D>("TerrainTextures/trawa/albedo"),
            this._content.Load<Texture2D>("TerrainTextures/kamienie/albedo"),
            this._content.Load<Texture2D>("TerrainTextures/snieg/albedo"),
        };

        DefaultSkybox = this._content.Load<TextureCube>("TerrainTextures/SkyBox");

        DefaultFont = this._content.Load<SpriteFont>("defaultFont");
        
        BloomExtractEffect = this._content.Load<Effect>("BloomExtract");
        BloomCombineEffect = this._content.Load<Effect>("BloomCombine");
        GaussianBlurEffect = this._content.Load<Effect>("GaussianBlur");
    }

    public static ModelData GetModel(string modelPath)
    {
        ModelPointer temp = _instance._models.Find(x => x.ModelData.ModelPath == modelPath);
        if (temp == null)
        {
            _instance._models.Add(new ModelPointer(_instance._content,modelPath));
            _instance._models.Last().Uses++;
            return _instance._models.Last().ModelData;
        }
        temp.Uses++;
        return temp.ModelData;
    }

    public static AnimatedModelData GetAnimatedModel(string modelPath)
    {
        AnimatedModelPointer temp = _instance._animatedModels.Find(x => x.AnimatedModelData.ModelPath == modelPath);
        if (temp == null)
        {
            _instance._animatedModels.Add(new AnimatedModelPointer(_instance._content, modelPath));
            _instance._animatedModels.Last().Uses++;
            return _instance._animatedModels.Last().AnimatedModelData;
        }
        temp.Uses++;
        return temp.AnimatedModelData;
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

    public static void FreeModel(ModelData model)
    {
        ModelPointer pointer = _instance._models.Find(x => x.ModelData == model);
        if(pointer == null) return;
        
        pointer.Uses--;
        if (pointer.Uses == 0)
        {
            _instance._models.Remove(pointer);
        }
    }

    public static void FreeAnimatedModel(AnimatedModelData animatedModel)
    {
        AnimatedModelPointer pointer = _instance._animatedModels.Find(x => x.AnimatedModelData == animatedModel);
        if (pointer == null) return;

        pointer.Uses--;
        if (pointer.Uses == 0)
        {
            _instance._animatedModels.Remove(pointer);
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