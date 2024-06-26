using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class BuiltBuilding : Component
{
    public enum BonusType
    {
        None,
        Damage,
        Health
    }
    
    public int MeatCost;
    public int Level;

    private GameObject _ui;
    private GameObject _upgradeButton;
    private GameObject _cancelButton;
    private GameObject _costText;

    private BonusType _bonusType;

    public override void Update()
    {
        if (!Active) return;
        
        if (_upgradeButton == null) return;
        
        if (_cancelButton.GetComponent<Button>().IsPressed)
        {
            Globals.PickingManager.PlayerBuildingPickingActive = true;
            Globals.PickingManager.PlayerBuildingBuiltPickingActive = true;
            Globals.PickingManager.PlayerMissionSelectPickingActive = true;
            
            _cancelButton.GetComponent<Button>().IsPressed = false;
            Globals.UIActive = false;
        }

        GameObject temp = _ui.FindGameObjectByName("Cost");
        if(temp == null) return;
        _costText = temp.Children[0];
        
        _costText.GetComponent<TextRenderer>().Content = MeatCost.ToString();
        
        if (GameManager.MeatNumber - MeatCost >= 0)
        {
            _upgradeButton.Children[0].GetComponent<TextRenderer>().Color = Color.White;
            _costText.GetComponent<TextRenderer>().Color = Color.White;
            
            if (_upgradeButton.GetComponent<Button>().IsPressed)
            {
                GameManager.MeatNumber -= MeatCost;
                Level++;
                if (_bonusType == BonusType.Damage)
                {
                    GameManager.DamageMultiplier = 1.0f + (Level - 1) * 0.1f;
                }
                else
                {
                    GameManager.HealthMultiplier = 1.0f + (Level - 1) * 0.1f;
                }
                MeatCost += 100;
                _upgradeButton.GetComponent<Button>().IsPressed = false;
            }
        }
        else
        {
            _upgradeButton.Children[0].GetComponent<TextRenderer>().Color = Color.Red;
            _costText.GetComponent<TextRenderer>().Color = Color.Red;
        }
    }

    public BuiltBuilding() {}

    public void OnClick()
    {
        ActivateUi();
    }

    public override void Initialize()
    {
        MeatCost = 100;
        Level = 1;
    }

    private void ActivateUi()
    {
        _ui = ParentObject.FindGameObjectByName("UI_Built");
        _upgradeButton = _ui.FindGameObjectByName("Upgrade_Button");
        _cancelButton = _ui.FindGameObjectByName("Cancel_Button");

        if (Globals.PickingManager.PlayerBuildingUiBuiltActive)
        {
            _ui.Active = true;
            Globals.UIActive = true;
            Globals.PickingManager.PlayerBuildingPickingActive = false;
            Globals.PickingManager.PlayerBuildingBuiltPickingActive = false;
            Globals.PickingManager.PlayerMissionSelectPickingActive = false;
        }
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>BuiltBuilding</type>");
        
        builder.Append("<active>" + Active + "</active>");
        
        builder.Append("<meatCost>" + MeatCost + "</meatCost>");
        
        builder.Append("<level>" + Level + "</level>");
        
        builder.Append("<bonusType>" + _bonusType + "</bonusType>");
        
        builder.Append("</component>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        MeatCost = int.TryParse(element.Element("meatCost")?.Value, out MeatCost) ? MeatCost : 100;
        Level = int.TryParse(element.Element("level")?.Value, out Level) ? Level : 1;
        XElement bonus = element.Element("bonusType");
        _bonusType = bonus != null ?(BonusType)Enum.Parse(typeof(BonusType), element.Element("bonusType")?.Value) : BonusType.None;
    }
    
    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }
    
#if DEBUG
    private bool _changingBonus = false;
    public override void Inspect()
    {
        if (ImGui.CollapsingHeader("BuiltBuilding"))
        {
            ImGui.Checkbox("BuiltBuilding active", ref Active);
            ImGui.SliderInt("Meat cost", ref MeatCost, 0, 1000);
            ImGui.SliderInt("Level", ref Level, 1, 5);
            ImGui.Text("Current bonus: " + _bonusType);
            ImGui.SameLine();
            if (ImGui.Button("Switch bonus"))
            {
                _changingBonus = true;
            }
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }
        if (_changingBonus)
        {
            ImGui.Begin("Change bonus");
            var values = Enum.GetValues(typeof(BonusType));
            foreach (BonusType type in values)
            {
                if (ImGui.Button(type.ToString()))
                {
                    if (_bonusType == type)
                    {
                        _changingBonus = false;
                        ImGui.End();
                        return;
                    }
                    _bonusType = type;
                    _changingBonus = false;
                }
            }
            if (ImGui.Button("Cancel"))
            {
                _changingBonus = false;
            }
            ImGui.End();
        }
    }
#endif
}