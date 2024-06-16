using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;

namespace RTS_Engine;

public class Building : Component
{
    public int PuzzleCost;
    public int UpgradeCost;

    public override void Update()
    {
        if (!Active) return;
    }

    public Building() {}

    public void OnClick()
    {
        if (GameManager.PuzzleNumber - PuzzleCost >= 0)
        {
            CheckAndActivatePuzzle();
        }
    }

    public override void Initialize()
    {
        PuzzleCost = 0;
        UpgradeCost = 100;
    }
    
    private void CheckAndActivatePuzzle()
    {
        // Get the puzzle component from the child object
        Puzzle puzzle = ParentObject.Children.Find(child => child.HasComponent<Puzzle>())?.GetComponent<Puzzle>();
        if (puzzle != null && !puzzle.Completed)
        {
            puzzle.PuzzleCompleted += () =>
            {
                ParentObject.FindGameObjectByName("Building").Active = true;
                ParentObject.RemoveFirstComponentOfType<MeshRenderer>();
                Globals.PickingManager.PlayerBuildingPickingActive = true;
                Globals.PickingManager.PlayerMissionSelectPickingActive = true;
                
                GameManager.RemovePuzzle(PuzzleCost);
            };
            
            Globals.PickingManager.PlayerBuildingPickingActive = false;
            Globals.PickingManager.PlayerMissionSelectPickingActive = false;
            puzzle.ActivatePuzzle();
        }
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Building</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<puzzleCost>" + PuzzleCost + "</puzzleCost>");
        
        builder.Append("<upgradeCost>" + UpgradeCost + "</upgradeCost>");
        
        builder.Append("</component>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        PuzzleCost = int.Parse(element.Element("puzzleCost")?.Value);
        UpgradeCost = int.Parse(element.Element("upgradeCost")?.Value);
    }

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }

#if DEBUG
    public override void Inspect()
    {
        if (ImGui.CollapsingHeader("Building"))
        {
            ImGui.Checkbox("Building active", ref Active);
            ImGui.SliderInt("Puzzle cost", ref PuzzleCost, 0, 16);
            ImGui.SliderInt("Upgrade cost", ref UpgradeCost, 100, 500);
        }
    }
#endif
}