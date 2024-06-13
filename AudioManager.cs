using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace RTS_Engine;

public class AudioManager
{
   
    public static SoundEffect _soundEffect = AssetManager.DefaultAmbientMusic;
    public static SoundEffectInstance _soundEffectInstance = _soundEffect.CreateInstance();
    public AudioListener listener = new AudioListener();
    public List<GameObject> GOEmitters = new List<GameObject>();


    public AudioManager(AudioListener listener, List<GameObject> GOEmitters)
    {
        this.listener = listener;
        this.GOEmitters = GOEmitters;
    }
     
    public static void Apply3D()
    {
        
    }
    
    public static void PlayAmbient1()
    {
        // MediaPlayer.Play(AssetManager.DefaultSong);
    }
    
    //TODO: Zmiana muzyki w tle w zaleznosci od sceny (tytul, glosnosc)
    public static void PlaySoundtrack()
    {
        
    }
    
    
    
   
    
    
    
    
   
}