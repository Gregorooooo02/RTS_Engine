using System;
using System.Drawing;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Color = Microsoft.Xna.Framework.Color;

namespace RTS_Engine;

public class Building : Component
{
    public int PuzzleCost;

    private GameObject _ui;
    private GameObject _puzzle;
    private GameObject _buildButton;
    private GameObject _cancelButton;
    private GameObject _costText;

    public override void Update()
    {
        if (!Active) return;

        if (_buildButton == null) return;

        if (_cancelButton.GetComponent<Button>().IsPressed)
        {
            Globals.PickingManager.PlayerBuildingPickingActive = true;
            Globals.PickingManager.PlayerBuildingBuiltPickingActive = true;
            Globals.PickingManager.PlayerMissionSelectPickingActive = true;
            
            _cancelButton.GetComponent<Button>().IsPressed = false;
            Globals.UIActive = false;
        }
        
        _costText = _ui.FindGameObjectByName("Cost").Children[0];
        _costText.GetComponent<TextRenderer>().Content = "Cost " + PuzzleCost.ToString();
        
        if (GameManager.PuzzleNumber - PuzzleCost >= 0)
        {
            _buildButton.Children[0].GetComponent<TextRenderer>().Color = Color.White;
            _costText.GetComponent<TextRenderer>().Color = Color.White;
            
            if (_buildButton.GetComponent<Button>().IsPressed)
            {
                CheckAndActivatePuzzle();
                _buildButton.GetComponent<Button>().IsPressed = false;
            }
        }
        else
        {
            _buildButton.Children[0].GetComponent<TextRenderer>().Color = Color.Red;
            _costText.GetComponent<TextRenderer>().Color = Color.Red;
        }
    }

    public Building() {}

    public void OnClick()
    {
        ActivateUi();
    }

    public override void Initialize()
    {
        PuzzleCost = 0;
    }
    
    private void CheckAndActivatePuzzle()
    {
        // Get the puzzle component from the child object
        _puzzle = ParentObject.FindGameObjectByName("Puzzle");
        
        if (_puzzle != null && !_puzzle.GetComponent<Puzzle>().Completed)
        {
            _puzzle.GetComponent<Puzzle>().PuzzleCompleted += () =>
            {
                var building = ParentObject.FindGameObjectByName("Building");
                building.Active = true;

                foreach (var child in building.Children)
                {
                    child.Active = false;
                }
                
                ParentObject.GetComponent<Pickable>().Active = false;
                ParentObject.RemoveFirstComponentOfType<MeshRenderer>();
                Globals.PickingManager.PlayerBuildingUiActive = true;
                Globals.PickingManager.PlayerBuildingPickingActive = true;
                Globals.PickingManager.PlayerBuildingUiBuiltActive = true;
                Globals.PickingManager.PlayerBuildingBuiltPickingActive = true;
                Globals.PickingManager.PlayerMissionSelectPickingActive = true;
                
                GameManager.RemovePuzzle(PuzzleCost);
                Globals.UIActive = false;
            };

            Globals.UIActive = true;
            _ui.Active = false;
            Globals.PickingManager.PlayerBuildingUiActive = false;
            Globals.PickingManager.PlayerBuildingPickingActive = false;
            Globals.PickingManager.PlayerBuildingUiBuiltActive = false;
            Globals.PickingManager.PlayerBuildingBuiltPickingActive = false;
            Globals.PickingManager.PlayerMissionSelectPickingActive = false;
            _puzzle.GetComponent<Puzzle>().ActivatePuzzle();
        }
    }

    private void ActivateUi()
    {
        _ui = ParentObject.FindGameObjectByName("UI");
        _buildButton = _ui.FindGameObjectByName("Build_Button");
        _cancelButton = _ui.FindGameObjectByName("Cancel_Button");
        
        if (Globals.PickingManager.PlayerBuildingUiActive)
        {
            _ui.Active = true;
            Globals.UIActive = true;
            Globals.PickingManager.PlayerBuildingPickingActive = false;
            Globals.PickingManager.PlayerBuildingUiBuiltActive = false;
            Globals.PickingManager.PlayerMissionSelectPickingActive = false;
        }
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Building</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<puzzleCost>" + PuzzleCost + "</puzzleCost>");
        
        builder.Append("</component>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        PuzzleCost = int.TryParse(element.Element("puzzleCost")?.Value, out PuzzleCost) ? PuzzleCost : 9;
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
        }
    }
#endif
}