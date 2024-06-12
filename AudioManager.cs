using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace RTS_Engine;

public class AudioManager
{
    private AudioManager()
    {
       
    }
    
    public static void Play(string path)
    {
        
    }
    
    public static void PlayAmbient1()
    {
        MediaPlayer.Play(AssetManager.DefaultSong);
    }
    
   
    
    public static void Loop(SoundEffectInstance soundInstance)
    {
        soundInstance.IsLooped = true;
    }
    
    
    
   
}