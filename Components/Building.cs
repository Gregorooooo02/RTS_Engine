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

    private GameObject _uiBuilt;
    private GameObject _upgradeButton;
    private GameObject _cancelUpgradeButton;
    private GameObject _costTextBuilt;

    public override void Update()
    {
        if (!Active) return;

        if (_buildButton == null) return;

        if (_cancelButton.GetComponent<Button>().IsPressed)
        {
            Globals.PickingManager.PlayerBuildingPickingActive = true;
            Globals.PickingManager.PlayerMissionSelectPickingActive = true;
            
            _cancelButton.GetComponent<Button>().IsPressed = false;
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

    public void OnClickBuilt()
    {
        ActivateUiBuilt();
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
                ParentObject.FindGameObjectByName("Building").Active = true;
                ParentObject.GetComponent<Pickable>().Active = false;
                ParentObject.RemoveFirstComponentOfType<MeshRenderer>();
                Globals.PickingManager.PlayerBuildingUiActive = true;
                Globals.PickingManager.PlayerBuildingPickingActive = true;
                Globals.PickingManager.PlayerMissionSelectPickingActive = true;
                
                GameManager.RemovePuzzle(PuzzleCost);
            };

            _ui.Active = false;
            Globals.PickingManager.PlayerBuildingUiActive = false;
            Globals.PickingManager.PlayerBuildingPickingActive = false;
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
            Globals.PickingManager.PlayerBuildingPickingActive = false;
            Globals.PickingManager.PlayerMissionSelectPickingActive = false;
        }
    }
    
    private void ActivateUiBuilt()
    {
        _uiBuilt = ParentObject.FindGameObjectByName("UI_Built");
        _upgradeButton = _uiBuilt.FindGameObjectByName("Upgrade_Button");
        _cancelUpgradeButton = _uiBuilt.FindGameObjectByName("Cancel__Button");
        
        if (Globals.PickingManager.PlayerBuildingUiBuiltActive)
        {
            _uiBuilt.Active = true;
            Globals.PickingManager.PlayerBuildingPickingActive = false;
            Globals.PickingManager.PlayerMissionSelectPickingActive = false;
            Globals.PickingManager.PlayerBuildingUiBuiltActive = false;
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