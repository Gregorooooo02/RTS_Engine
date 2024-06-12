using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace RTS_Engine;

public class SongData
{
    public int CurrentSongIndex;
    public readonly List<Song> Songs = new();
    public string SongPath;
    
    public SongData(ContentManager manager, string songPath)
    {
        LoadSong(manager, songPath);
        CurrentSongIndex = 0;
    }
    
    public void LoadSong(ContentManager manager, string songPath)
    {
        Songs.Add(manager.Load<Song>(songPath));
        SongPath = songPath;
    }
}