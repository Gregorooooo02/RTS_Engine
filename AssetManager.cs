using System.Collections.Generic;
#if DEBUG
using System;
using System.IO;
#endif
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
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
    private readonly List<SongPointer> _sounds;

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
    
    public static SoundEffect DefaultAmbientMusic { get; private set; }
    public static SoundEffect DefaultSong {get; private set;}
    
    //SOUNDS
    
    //CABINET
    public static List<SoundEffect> CabinetIdle = new List<SoundEffect>();
    public static List<SoundEffect> CabinetMove = new List<SoundEffect>();
    public static List<SoundEffect> CabinetAttack = new List<SoundEffect>();
    
    //CANDLE
    public static List<SoundEffect> CandleIdle = new List<SoundEffect>();
    public static List<SoundEffect> CandleMove = new List<SoundEffect>();
    public static List<SoundEffect> CandleAttack = new List<SoundEffect>();
    
    //CHAIR
    public static List<SoundEffect> ChairIdle = new List<SoundEffect>();
    public static List<SoundEffect> ChairMove = new List<SoundEffect>();
    public static List<SoundEffect> ChairAttack = new List<SoundEffect>();
    
    //CHANDELIER
    public static List<SoundEffect> ChandelierIdle = new List<SoundEffect>();
    public static List<SoundEffect> ChandelierMove = new List<SoundEffect>();
    public static List<SoundEffect> ChandelierAttack = new List<SoundEffect>();
    
    //MINI CABINET
    public static List<SoundEffect> MiniCabinetIdle = new List<SoundEffect>();
    public static List<SoundEffect> MiniCabinetMove = new List<SoundEffect>();
    public static List<SoundEffect> MiniCabinetAttack = new List<SoundEffect>();
    
    //WARDROBE
    public static List<SoundEffect> WardrobeIdle = new List<SoundEffect>();
    public static List<SoundEffect> WardrobeMove = new List<SoundEffect>();
    public static List<SoundEffect> WardrobeAttack = new List<SoundEffect>();
    
    //ARCHER
    public static List<SoundEffect> ArcherAttack = new List<SoundEffect>();
    public static List<SoundEffect> ArcherIdle = new List<SoundEffect>();
    public static List<SoundEffect> ArcherDamage = new List<SoundEffect>();
    
    //CIVILIAN
    public static List<SoundEffect> CivilianFlee = new List<SoundEffect>();
    public static List<SoundEffect> CivilianIdle = new List<SoundEffect>();
    public static List<SoundEffect> CivilianDamage = new List<SoundEffect>();
    
    //KNIGHT
    public static List<SoundEffect> KnightAttack = new List<SoundEffect>();
    public static List<SoundEffect> KnightIdle = new List<SoundEffect>();
    public static List<SoundEffect> KnightDamage = new List<SoundEffect>();
    
    //THEMES
    public static SoundEffect MissionTheme { get; private set; }
    public static SoundEffect BaseTheme { get; private set; }
    
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
    
    private class SongPointer
    {
        public readonly SongData SongData;
        public int Uses;

        public SongPointer(ContentManager manager, string modelPath)
        {
            SongData = new SongData(manager, modelPath);
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
        _sounds = new List<SongPointer>();
        
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
        DefaultAmbientMusic = this._content.Load<SoundEffect>("gameOver");
        DefaultSong = _content.Load<SoundEffect>("amogusDrip");
        
        //SOUNDS
        
        //CABINET
        CabinetAttack.Add(_content.Load<SoundEffect>("Sounds/cabinet_attack1"));
        CabinetAttack.Add(_content.Load<SoundEffect>("Sounds/cabinet_attack2"));
        CabinetAttack.Add(_content.Load<SoundEffect>("Sounds/cabinet_attack3"));
        CabinetAttack.Add(_content.Load<SoundEffect>("Sounds/cabinet_attack4"));
        
        CabinetIdle.Add(_content.Load<SoundEffect>("Sounds/cabinet_idle1"));
        CabinetIdle.Add(_content.Load<SoundEffect>("Sounds/cabinet_idle2"));
        CabinetIdle.Add(_content.Load<SoundEffect>("Sounds/cabinet_idle3"));
        
        CabinetMove.Add(_content.Load<SoundEffect>("Sounds/cabinet_move1"));
        CabinetMove.Add(_content.Load<SoundEffect>("Sounds/cabinet_move2"));
        CabinetMove.Add(_content.Load<SoundEffect>("Sounds/cabinet_move3"));
        
        //CANDLE
        CandleAttack.Add(_content.Load<SoundEffect>("Sounds/candle_attack1"));
        CandleAttack.Add(_content.Load<SoundEffect>("Sounds/candle_attack2"));
        
        CandleIdle.Add(_content.Load<SoundEffect>("Sounds/candle_idle1"));
        CandleIdle.Add(_content.Load<SoundEffect>("Sounds/candle_idle2"));
        CandleIdle.Add(_content.Load<SoundEffect>("Sounds/candle_idle3"));
        CandleIdle.Add(_content.Load<SoundEffect>("Sounds/candle_idle4"));
        CandleIdle.Add(_content.Load<SoundEffect>("Sounds/candle_idle5"));
        
        CandleMove.Add(_content.Load<SoundEffect>("Sounds/candle_move1"));
        CandleMove.Add(_content.Load<SoundEffect>("Sounds/candle_move2"));
        CandleMove.Add(_content.Load<SoundEffect>("Sounds/candle_move3"));
        
        //CHAIR
        ChairAttack.Add(_content.Load<SoundEffect>("Sounds/chair_attack1"));
        ChairAttack.Add(_content.Load<SoundEffect>("Sounds/chair_attack2"));
        ChairAttack.Add(_content.Load<SoundEffect>("Sounds/chair_attack3"));
        ChairAttack.Add(_content.Load<SoundEffect>("Sounds/chair_attack4"));
        ChairAttack.Add(_content.Load<SoundEffect>("Sounds/chair_attack5"));
        
        ChairIdle.Add(_content.Load<SoundEffect>("Sounds/chair_idle1"));
        ChairIdle.Add(_content.Load<SoundEffect>("Sounds/chair_idle2"));
        ChairIdle.Add(_content.Load<SoundEffect>("Sounds/chair_idle3"));
        ChairIdle.Add(_content.Load<SoundEffect>("Sounds/chair_idle4"));
        ChairIdle.Add(_content.Load<SoundEffect>("Sounds/chair_idle5"));
        
        ChairMove.Add(_content.Load<SoundEffect>("Sounds/chair_move1"));
        ChairMove.Add(_content.Load<SoundEffect>("Sounds/chair_move2"));
        ChairMove.Add(_content.Load<SoundEffect>("Sounds/chair_move3"));
        ChairMove.Add(_content.Load<SoundEffect>("Sounds/chair_move4"));
        ChairMove.Add(_content.Load<SoundEffect>("Sounds/chair_move5"));
        
        //CHANDELIER
        ChandelierAttack.Add(_content.Load<SoundEffect>("Sounds/chandelier_attack1"));
        ChandelierAttack.Add(_content.Load<SoundEffect>("Sounds/chandelier_attack2"));
        ChandelierAttack.Add(_content.Load<SoundEffect>("Sounds/chandelier_attack3"));
        
        ChandelierIdle.Add(_content.Load<SoundEffect>("Sounds/chandelier_idle1"));
        ChandelierIdle.Add(_content.Load<SoundEffect>("Sounds/chandelier_idle2"));
        ChandelierIdle.Add(_content.Load<SoundEffect>("Sounds/chandelier_idle3"));
        ChandelierIdle.Add(_content.Load<SoundEffect>("Sounds/chandelier_idle4"));
        
        ChandelierMove.Add(_content.Load<SoundEffect>("Sounds/chandelier_move1"));
        ChandelierMove.Add(_content.Load<SoundEffect>("Sounds/chandelier_move2"));
        ChandelierMove.Add(_content.Load<SoundEffect>("Sounds/chandelier_move3"));
        
        //MINI CABINET
        MiniCabinetAttack.Add(_content.Load<SoundEffect>("Sounds/mini_attack1"));
        MiniCabinetAttack.Add(_content.Load<SoundEffect>("Sounds/mini_attack2"));
        MiniCabinetAttack.Add(_content.Load<SoundEffect>("Sounds/mini_attack3"));
        MiniCabinetAttack.Add(_content.Load<SoundEffect>("Sounds/mini_attack4"));
        
        MiniCabinetIdle.Add(_content.Load<SoundEffect>("Sounds/mini_idle1"));
        MiniCabinetIdle.Add(_content.Load<SoundEffect>("Sounds/mini_idle2"));
        MiniCabinetIdle.Add(_content.Load<SoundEffect>("Sounds/mini_idle3"));
        
        MiniCabinetMove.Add(_content.Load<SoundEffect>("Sounds/mini_move1"));
        MiniCabinetMove.Add(_content.Load<SoundEffect>("Sounds/mini_move2"));
        MiniCabinetMove.Add(_content.Load<SoundEffect>("Sounds/mini_move3"));
        
        //WARDROBE
        WardrobeAttack.Add(_content.Load<SoundEffect>("Sounds/wardrobe_attack1"));
        WardrobeAttack.Add(_content.Load<SoundEffect>("Sounds/wardrobe_attack2"));
        WardrobeAttack.Add(_content.Load<SoundEffect>("Sounds/wardrobe_attack3"));
        WardrobeAttack.Add(_content.Load<SoundEffect>("Sounds/wardrobe_attack4"));
        
        WardrobeIdle.Add(_content.Load<SoundEffect>("Sounds/wardrobe_idle1"));
        WardrobeIdle.Add(_content.Load<SoundEffect>("Sounds/wardrobe_idle2"));
        
        WardrobeMove.Add(_content.Load<SoundEffect>("Sounds/wardrobe_move1"));
        WardrobeMove.Add(_content.Load<SoundEffect>("Sounds/wardrobe_move2"));
        WardrobeMove.Add(_content.Load<SoundEffect>("Sounds/wardrobe_move3"));
        
        //ARCHER
        ArcherAttack.Add(_content.Load<SoundEffect>("Sounds/archer_attack1"));
        ArcherAttack.Add(_content.Load<SoundEffect>("Sounds/archer_attack2"));
        ArcherAttack.Add(_content.Load<SoundEffect>("Sounds/archer_attack3"));
        
        ArcherDamage.Add(_content.Load<SoundEffect>("Sounds/archer_damage1"));
        ArcherDamage.Add(_content.Load<SoundEffect>("Sounds/archer_damage2"));
        ArcherDamage.Add(_content.Load<SoundEffect>("Sounds/archer_damage3"));
        ArcherDamage.Add(_content.Load<SoundEffect>("Sounds/archer_damage4"));
        ArcherDamage.Add(_content.Load<SoundEffect>("Sounds/archer_damage5"));
        
        ArcherIdle.Add(_content.Load<SoundEffect>("Sounds/archer_idle1"));
        ArcherIdle.Add(_content.Load<SoundEffect>("Sounds/archer_idle2"));
        ArcherIdle.Add(_content.Load<SoundEffect>("Sounds/archer_idle3"));
        ArcherIdle.Add(_content.Load<SoundEffect>("Sounds/archer_idle4"));
        
        //CIVILIAN
        CivilianDamage.Add(_content.Load<SoundEffect>("Sounds/civilian_damage1"));
        CivilianDamage.Add(_content.Load<SoundEffect>("Sounds/civilian_damage2"));
        CivilianDamage.Add(_content.Load<SoundEffect>("Sounds/civilian_damage3"));
        CivilianDamage.Add(_content.Load<SoundEffect>("Sounds/civilian_damage4"));
        CivilianDamage.Add(_content.Load<SoundEffect>("Sounds/civilian_damage5"));
        
        CivilianFlee.Add(_content.Load<SoundEffect>("Sounds/civilian_flee1"));
        CivilianFlee.Add(_content.Load<SoundEffect>("Sounds/civilian_flee2"));
        CivilianFlee.Add(_content.Load<SoundEffect>("Sounds/civilian_flee3"));
        CivilianFlee.Add(_content.Load<SoundEffect>("Sounds/civilian_flee4"));
        
        CivilianIdle.Add(_content.Load<SoundEffect>("Sounds/civilian_idle1"));
        CivilianIdle.Add(_content.Load<SoundEffect>("Sounds/civilian_idle2"));
        CivilianIdle.Add(_content.Load<SoundEffect>("Sounds/civilian_idle3"));
        CivilianIdle.Add(_content.Load<SoundEffect>("Sounds/civilian_idle4"));
        
        //KNIGHT
        KnightAttack.Add(_content.Load<SoundEffect>("Sounds/knight_attack1"));
        KnightAttack.Add(_content.Load<SoundEffect>("Sounds/knight_attack2"));
        KnightAttack.Add(_content.Load<SoundEffect>("Sounds/knight_attack3"));
        KnightAttack.Add(_content.Load<SoundEffect>("Sounds/knight_attack4"));
        KnightAttack.Add(_content.Load<SoundEffect>("Sounds/knight_attack5"));
        
        KnightDamage.Add(_content.Load<SoundEffect>("Sounds/knight_damage1"));
        KnightDamage.Add(_content.Load<SoundEffect>("Sounds/knight_damage2"));
        KnightDamage.Add(_content.Load<SoundEffect>("Sounds/knight_damage3"));
        KnightDamage.Add(_content.Load<SoundEffect>("Sounds/knight_damage4"));
        
        KnightIdle.Add(_content.Load<SoundEffect>("Sounds/knight_idle1"));
        KnightIdle.Add(_content.Load<SoundEffect>("Sounds/knight_idle2"));
        KnightIdle.Add(_content.Load<SoundEffect>("Sounds/knight_idle3"));
        KnightIdle.Add(_content.Load<SoundEffect>("Sounds/knight_idle4"));
        
        MissionTheme = _content.Load<SoundEffect>("Sounds/Mission_Theme");
        BaseTheme = _content.Load<SoundEffect>("Sounds/Furninotura Camp");
        
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

    // public static SongData GetSong(string soundPath)
    // {
    //     SongPointer temp = _instance._sounds.Find(x => x.SongData.SongPath == soundPath);
    //     if (temp == null)
    //     {
    //         _instance._sounds.Add(new SongPointer(_instance._content, soundPath));
    //         _instance._sounds.Last().Uses++;
    //         return _instance._sounds.Last().SongData;
    //     }
    //     temp.Uses++;
    //     return temp.SongData;
    // }
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