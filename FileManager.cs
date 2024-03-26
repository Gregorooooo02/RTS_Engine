using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RTS_Engine;

public class FileManager
{
    public static FileManager Instance;
    public static void Initialize()
    {
        Instance = new FileManager();
    }
    
    public Dictionary<String, String> ReadConfigFile(String path)
    {
        Dictionary<String, String> dict = new Dictionary<String, String>();
        
        // Read file and fill dictionary with it
        dict.Add("Forward", "W");
        dict.Add("Backward", "S");
        dict.Add("Left", "A");
        dict.Add("Right", "D");
        dict.Add("Exit", "Escape");
        dict.Add("LMB", "0");
        dict.Add("MMB", "1");
        dict.Add("RMB", "2");
        
        return dict;
    }
}