﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine;

public class AgentsManager
{
    public readonly PatrolPathManager PatrolManager = new();
    public readonly ProjectileManager ProjectileManager = new();
    
    public List<Agent> Units = new();
    public List<Agent> Enemies = new();

    public List<Agent> ClappedCivilians = new();
    public List<Agent> ClappedSoldiers = new();
    public List<Agent> ClappedBuildings = new();
    
    public List<Agent> SelectedUnits = new();
    
    public Vector3 UiOffset = new(120.0f, 0.0f, 0.0f);
    public Vector3 BackgroundOffset = new(0.0f, 0.0f, 0.1f);

    private readonly Vector3 _iconsStart = new Vector3(275, 770 , 0);
    private readonly Vector3 _healthbarsStart = new Vector3(272, 740,0);

    public Marker Marker = null;

    public void Initialize()
    {
        // Initialize the UI for all units with the offset
        PlacePortraits(false);
    }

    public void PlacePortraits(bool scaleStartWithScreenSize = true)
    {
        for (int i = 0; i < Units.Count; i++)
        {
            var uiObject = Units[i].ParentObject.FindGameObjectByName("UI");
            var icon = uiObject?.FindGameObjectByName("Icon");
            var healthBar = uiObject?.FindGameObjectByName("HP");
            var healthStatus = healthBar?.Children[0];
            
            if (icon == null || healthBar == null || healthStatus == null) continue;

            if (scaleStartWithScreenSize)
            {
                icon?.Transform.SetLocalPosition(_iconsStart * Globals.Ratio + UiOffset * i * Globals.Ratio);
                healthBar?.Transform.SetLocalPosition(_healthbarsStart * Globals.Ratio + BackgroundOffset + UiOffset * i * Globals.Ratio);
                healthStatus?.Transform.SetLocalPosition(_healthbarsStart * Globals.Ratio  + UiOffset * i * Globals.Ratio);
            }
            else
            {
                icon?.Transform.SetLocalPosition(_iconsStart + UiOffset * i);
                healthBar?.Transform.SetLocalPosition(_healthbarsStart + BackgroundOffset + UiOffset * i);
                healthStatus?.Transform.SetLocalPosition(_healthbarsStart  + UiOffset * i);
            }
        }
    }
    
    public static void ChangeUnitSelection(Agent agent,bool selected)
    {
        agent?.Icon?.GetComponent<SpiteRenderer>()?.SelectAndDeselect(selected ? GameAction.SELECT : GameAction.DESELECT);
    }

    public void DeselectAllUnits()
    {
        foreach (Agent selectedUnit in SelectedUnits)
        {
            ChangeUnitSelection(selectedUnit, false);
        }
        SelectedUnits.Clear();
    }
    
    public void CheckForOrders()
    {
        //Probe for unit selection
        List<Pickable> units = Globals.PickingManager.PickUnits();
        //If unit picking attempt happened and CTRL key was not pressed then clear selected units list
        if (!Globals.HitUI)
        {
            if(Globals.PickingManager.PickedUnits && !InputManager.Instance.IsActive(GameAction.CTRL))
            { 
                DeselectAllUnits();
            }
            //Then add all selected units to list
            foreach (Pickable unit in units)
            {
                Agent temp = unit.ParentObject.GetComponent<Agent>();
                if (temp != null && !SelectedUnits.Contains(temp))
                {
                    ChangeUnitSelection(temp, true);
                    SelectedUnits.Add(temp);
                    if (unit.ParentObject.GetComponent<Agent>().Emitter != null)
                    {
                        SelectedUnits[0]?.Emitter.PlayIdle();
                    }
                    
                }
            }
        }
        Pickable selectedEnemy = Globals.PickingManager.PickEnemy();
        if (Globals.PickingManager.PickedEnemy && selectedEnemy == null)
        {
            //If RMB was to pick enemy but no enemy was picked then pick ground.
            Vector3? point = Globals.PickingManager.PickGround(InputManager.Instance.MousePosition, 0.1f);
            if (point.HasValue)
            {
                Marker?.PlaceMarker(point.Value);
                Vector2 dest = new Vector2(point.Value.X,point.Value.Z);
                //TODO: Pass order 'move to point' to all selected units
                foreach (Agent selectedUnit in SelectedUnits)
                {
                    PlayerUnitData data = (PlayerUnitData)selectedUnit.AgentData;
                    data.Destination = dest;
                }
            }
        }
        else if(selectedEnemy != null)
        {
            Agent enemy = selectedEnemy.ParentObject.GetComponent<Agent>();
            if (enemy != null)
            {
                foreach (Agent selectedUnit in SelectedUnits)
                {
                    PlayerUnitData data = (PlayerUnitData)selectedUnit.AgentData;
                    data.Target = enemy;
                }
            }
        }
    }
}