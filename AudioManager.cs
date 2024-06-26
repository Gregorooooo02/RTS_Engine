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
   
    public static SoundEffect MissionTheme = AssetManager.MissionTheme;
    public static SoundEffectInstance MissionThemeInstance = MissionTheme.CreateInstance();
    


    public AudioManager()
    {
       
    }

    // public static SoundEffect RandomSound(List<SoundEffect> sounds)
    // {
    //     Random random = new Random();
    //     int index = random.Next(sounds.Count);
    //     return sounds[index];
    // }
     
    public static void Apply3D()
    {
        
    }
    
    public static void PlayMissionTheme()
    {
        MissionThemeInstance.IsLooped = true;
        MissionThemeInstance.Play();
    }
    
    //TODO: Zmiana muzyki w tle w zaleznosci od sceny (tytul, glosnosc)
    public static void PlaySoundtrack()
    {
        
    }
    
    
    
   
    
    
    
    
   
}