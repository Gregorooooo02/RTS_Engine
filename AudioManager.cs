using System;
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
    public AudioListener _listener = new AudioListener();


    public AudioManager()
    {
        _listener.Position = new Vector3(0, 0, 0);  
    }
    
    public static void Play(string path)
    {
        
    }
    
    public static void PlayAmbient1()
    {
        MediaPlayer.Play(AssetManager.DefaultSong);
    }
    
    //TODO: Zmiana muzyki w tle w zaleznosci od sceny (tytul, glosnosc)
    public static void PlaySoundtrack()
    {
        
    }
    
    
    
   
    
    
    
    
   
}