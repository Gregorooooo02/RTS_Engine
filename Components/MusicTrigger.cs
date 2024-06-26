using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework.Audio;

namespace RTS_Engine;

public class MusicTrigger : Component
{
    public enum MusicTrack
    {
        Camp,
        Mission,
        None
    }

    private MusicTrack _track;

    public override void Update()
    {
        if(!Active) return;
        switch (_track)
        {
            case MusicTrack.Camp:
                AudioManager.CurrentlyPlayedTheme?.Stop();
                AudioManager.CurrentlyPlayedTheme = AudioManager.BaseThemeInstance;
                AudioManager.CurrentlyPlayedTheme.IsLooped = true;
                AudioManager.CurrentlyPlayedTheme?.Play();
                break;
            
            case MusicTrack.Mission:
                AudioManager.CurrentlyPlayedTheme?.Stop();
                AudioManager.CurrentlyPlayedTheme = AudioManager.MissionThemeInstance;
                AudioManager.CurrentlyPlayedTheme.IsLooped = true;
                AudioManager.CurrentlyPlayedTheme?.Play();
                break;
            
            case MusicTrack.None:
                AudioManager.CurrentlyPlayedTheme?.Stop();
                break;
        }
        Active = false;
    }

    public override void Initialize()
    {
        
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>MusicTrigger</type>");
        
        builder.Append("<active>" + Active + "</active>");
        
        builder.Append("<track>" + _track + "</track>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        _track = (MusicTrack)Enum.Parse(typeof(MusicTrack), element.Element("track")?.Value);
    }

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }

    
#if DEBUG   
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("MusicTrigger"))
        {
            ImGui.Checkbox("MusicTrigger active", ref Active);
            
            ImGui.Text("Current music track: " + _track);
            ImGui.Text("Change unit type:");

            var values = Enum.GetValues(typeof(MusicTrack));

            foreach (MusicTrack type in values)
            {
                if (ImGui.Button(type.ToString()))
                {
                    _track = type;
                }
            }
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }
    }
#endif
}