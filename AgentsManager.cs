﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine;

public class AgentsManager
{
    public readonly PatrolPathManager PatrolManager = new();
    
    public List<Agent> Units = new();
    public List<Agent> Enemies = new();

    public List<Agent> SelectedUnits = new();

    public void CheckForOrders()
    {
        //Probe for unit selection
        List<Pickable> units = Globals.PickingManager.PickUnits();
        //If unit picking attempt happened and CTRL key was not pressed then clear selected units list
        if(Globals.PickingManager.PickedUnits && !InputManager.Instance.IsActive(GameAction.CTRL))
        { 
            SelectedUnits.Clear();
        }
        //Then add all selected units to list
        foreach (Pickable unit in units)
        {
            Agent temp = unit.ParentObject.GetComponent<Agent>();
            if(temp != null) SelectedUnits.Add(temp);
        }

        Pickable selectedEnemy = Globals.PickingManager.PickEnemy();
        if (Globals.PickingManager.PickedEnemy && selectedEnemy == null)
        {
            //If RMB was to pick enemy but no enemy was picked then pick ground.
            Vector3? point = Globals.PickingManager.PickGround(InputManager.Instance.MousePosition, 0.1f);
            if (point.HasValue)
            {
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