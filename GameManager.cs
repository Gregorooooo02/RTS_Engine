using System;
using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public static class GameManager
{
    public static int MeatNumber;
    public static int PuzzleNumber;
    public static int MissionMeat = 0;
    public static int MissionPuzzle = 0;
    public static float CurrentAwareness = 0;
    public static float AwarenessLimit = 100.0f;
    public static float DamageMultiplier = 1.0f;
    public static float HealthMultiplier = 1.0f;
    
    public static bool TutorialDone = false;
    
    public static int MissionTutorialStep = 1;
    public static int MissionTutorialStepMax = 4;
    
    public static int BaseTutorialStep = 1;
    public static int BaseTutorialStepMax = 4;
    
    // 0000 0000
    // 1st bit - Wardrobe
    // 2nd bit - Cabinet
    // 3rd bit - Chandelier
    public static byte UnitsSelectedForMission = 0;

    private static GameAction _status = GameAction.NONE;

    public static readonly List<Task> Tasks = new();
    
    public static void Initialize()
    {
        MeatNumber = 0;
        PuzzleNumber = 0;
    }

    public static void ChangeAwareness(float change)
    {
        CurrentAwareness = MathHelper.Clamp(CurrentAwareness + change, 0, AwarenessLimit);
    }
    
    public static void AddMissionMeat(int amount)
    {
        MissionMeat += amount;
    }
    
    public static void AddMissionPuzzle(int amount)
    {
        MissionPuzzle += amount;
    }

    public static void TransferRewards()
    {
        MeatNumber += MissionMeat;
        PuzzleNumber += MissionPuzzle;
    }

    public static void ClearMissionRewards()
    {
        MissionMeat = 0;
        MissionPuzzle = 0;
    }
    
    public static GameAction CheckTasks()
    {
        for (int i = 0; i < Tasks.Count; i++)
        {
            if (Tasks[i].Status == GameAction.INCOMPLETE)
            {
                return GameAction.NONE;
            }
        }
        if (Tasks.Count == 0) return GameAction.NONE;
        return GameAction.WIN;
    }

    public static GameAction CheckIfGameOver()
    {
        /*
        for (int i = 0; i < Globals.AgentsManager.Units.Count; i++)
        {
            if (Globals.AgentsManager.Units[i].AgentData.Alive) return GameAction.NONE;
            
            _status = GameAction.GAME_OVER;
        }
        */
        if (CheckTasks() == GameAction.WIN) return GameAction.WIN;
        return Globals.AgentsManager.Units.Count == 0 || AwarenessLimit <= CurrentAwareness ? GameAction.GAME_OVER : GameAction.NONE;
        //return _status;
    }
    
    public static void AddMeat(int amount)
    {
        MeatNumber += amount;
    }
    
    public static void AddPuzzle(int amount)
    {
        PuzzleNumber += amount;
    }
    
    public static void RemoveMeat(int amount)
    {
        MeatNumber -= amount;
    }
    
    public static void RemovePuzzle(int amount)
    {
        PuzzleNumber -= amount;
    }

    public static void CheatMenu()
    {
        ImGui.Begin("Cheat Menu");
        ImGui.Checkbox("Tutorial done", ref TutorialDone);
        ImGui.DragInt("Meat Number", ref MeatNumber);
        ImGui.DragInt("Puzzle Number", ref PuzzleNumber);
        ImGui.DragFloat("Current awareness", ref CurrentAwareness);
        ImGui.DragFloat("Maximum awareness", ref AwarenessLimit);
        ImGui.DragFloat("Damage multiplier", ref DamageMultiplier, 0.1f, 1.0f);
        ImGui.DragFloat("Health multiplier", ref HealthMultiplier, 0.1f, 1.0f);
        ImGui.Text("Selected units mask " + UnitsSelectedForMission);
        ImGui.End();
    }
}