using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using RTS_Engine.Components.AI;

namespace RTS_Engine;

public class Task : Component
{
    public enum RewardType
    {
        Meat,
        Puzzle
    }
    
    public int Progress;
    public int Goal;
    
    public RewardType Type;
    public int Reward;

    public RewardType Type2;
    public int Reward2;
    
    public string TaskDescription = "New Task";
    
    public Agent.AgentType Target;
    
    public GameAction Status = GameAction.INCOMPLETE;
    
    // Child object of Text Renderer
    public GameObject TaskText;

    public Task() {}

    public override void Update()
    { 
        if (!Active) return;

        TaskText = ParentObject.FindGameObjectByName("Text");
        
        StringBuilder contentBuilder = new StringBuilder();
        contentBuilder.Append(TaskDescription);
        contentBuilder.Append(" (");
        contentBuilder.Append(Progress);
        contentBuilder.Append("/");
        contentBuilder.Append(Goal);
        contentBuilder.Append(")");
        
        TaskText.GetComponent<TextRenderer>().NewContent = contentBuilder.ToString();
        
        if (Status == GameAction.COMPLETE) return;
        
        switch (Target)
        {
            case Agent.AgentType.Civilian:
                Progress = Globals.AgentsManager.ClappedCivilians.Count;
                break;
            case Agent.AgentType.Soldier:
                Progress = Globals.AgentsManager.ClappedSoldiers.Count;
                break;
            case Agent.AgentType.EnemyBuilding:
                Progress = Globals.AgentsManager.ClappedBuildings.Count;
                break;
        }
        
        if (Progress >= Goal)
        {
            Status = GameAction.COMPLETE;
            ParentObject.GetComponent<SpiteRenderer>().CompleteAndIncomplete(Status);
            
            if (Type == RewardType.Puzzle)
            {
                GameManager.AddMissionPuzzle(Reward);
            } 
            else if (Type == RewardType.Meat)
            {
                GameManager.AddMissionMeat(Reward);
            }
            
            if (Type2 == RewardType.Puzzle)
            {
                GameManager.AddMissionPuzzle(Reward2);
            } 
            else if (Type2 == RewardType.Meat)
            {
                GameManager.AddMissionMeat(Reward2);
            }
        } 
        else if (Progress < Goal)
        {
            Status = GameAction.INCOMPLETE;
            ParentObject.GetComponent<SpiteRenderer>().CompleteAndIncomplete(Status);
        }
    }

    public override void Initialize()
    {
        Goal = 5;
        Type = RewardType.Meat;
        Type2 = RewardType.Puzzle;
        Reward = 100;
        Reward2 = 9;
        Target = Agent.AgentType.Civilian;
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Task</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<goal>" + Goal +"</goal>");

        builder.Append("<rewardType>" + Type + "</rewardType>");
        
        builder.Append("<reward>" + Reward +"</reward>");
        
        builder.Append("<rewardType2>" + Type2 + "</rewardType2>");
        
        builder.Append("<reward2>" + Reward2 +"</reward2>");
        
        builder.Append("<description>" + TaskDescription +"</description>");
        
        builder.Append("<target>" + Target +"</target>");
        
        builder.Append("</component>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        Goal = int.TryParse(element.Element("goal")?.Value, out int goalValue) ? goalValue : 5;
        Type = Enum.TryParse(element.Element("rewardType")?.Value, out RewardType rewardType) ? rewardType : RewardType.Meat;
        Reward = int.TryParse(element.Element("reward")?.Value, out int rewardValue) ? rewardValue : 100;
        Type2 = Enum.TryParse(element.Element("rewardType2")?.Value, out RewardType rewardType2) ? rewardType2 : RewardType.Puzzle;
        Reward2 = int.TryParse(element.Element("reward2")?.Value, out int rewardValue2) ? rewardValue2 : 9;
        TaskDescription = element.Element("description")?.Value;
        Target = (Agent.AgentType) Enum.Parse(typeof(Agent.AgentType), element.Element("target").Value);
        
        GameManager.Tasks.Add(this);
    }

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }

#if DEBUG
    public override void Inspect()
    {
        if (ImGui.CollapsingHeader("Task"))
        {
            ImGui.InputText("Description", ref TaskDescription, 100);
            ImGui.DragInt("Progress", ref Progress);
            ImGui.DragInt("Goal", ref Goal);
            
            ImGui.Separator();
            ImGui.Text("Reward1 Type: " + Type);
            if (ImGui.Button("Meat1"))
            {
                Type = RewardType.Meat;
            }
            ImGui.SameLine();
            if (ImGui.Button("Puzzle1"))
            {
                Type = RewardType.Puzzle;
            }
            ImGui.DragInt("Reward1", ref Reward);
            
            ImGui.Text("Reward2 Type: " + Type2);
            if (ImGui.Button("Meat2"))
            {
                Type2 = RewardType.Meat;
            }
            ImGui.SameLine();
            if (ImGui.Button("Puzzle2"))
            {
                Type2 = RewardType.Puzzle;
            }
            ImGui.DragInt("Reward2", ref Reward2);
            
            ImGui.Separator();
            ImGui.Text("Target: " + Target);
            ImGui.Text("Switch target to: ");
            if (ImGui.Button("Civilian"))
            {
                Target = Agent.AgentType.Civilian;
            }
            ImGui.SameLine();
            if (ImGui.Button("Soldier"))
            {
                Target = Agent.AgentType.Soldier;
            }
            if (ImGui.Button("Building")) 
            {
                Target = Agent.AgentType.EnemyBuilding;    
            }
        }
    }
#endif
}