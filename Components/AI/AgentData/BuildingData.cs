using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using RTS_Engine.Components.AI.Agent_States;
using RTS_Engine.Components.AI.Agent_States.Building;

namespace RTS_Engine.Components.AI.AgentData;

public class BuildingData : AgentData
{
    public readonly AgentState EntryState = new BuildingStart();
    
    public override string Serialize()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append(base.Serialize());
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        base.Deserialize(element);
    }

#if DEBUG
    public override void Inspect()
    {
        base.Inspect();
    }
#endif
}