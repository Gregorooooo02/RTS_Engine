using System.Collections.Generic;
using ImGuiNET;

namespace RTS_Engine;

public static class GameManager
{
    public static int MeatNumber;
    public static int PuzzleNumber;

    private static GameAction _status = GameAction.NONE;

    public static readonly List<Task> Tasks = new();
    
    public static void Initialize()
    {
        MeatNumber = 0;
        PuzzleNumber = 0;
    }

    public static GameAction CheckTasks()
    {
        for (int i = 0; i < Tasks.Count; i++)
        {
            if (Tasks[i].Status == GameAction.COMPLETE)
            {
                _status = GameAction.WIN;
            }
            else if (Tasks[i].Status == GameAction.INCOMPLETE)
            {
                continue;
            }
        }

        return _status;
    }

    public static GameAction CheckIfGameOver()
    {
        for (int i = 0; i < Globals.AgentsManager.Units.Count; i++)
        {
            if (Globals.AgentsManager.Units[i].AgentData.Alive) return GameAction.NONE;
            
            _status = GameAction.GAME_OVER;
        }
        
        return _status;
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
        ImGui.DragInt("Meat Number", ref MeatNumber);
        ImGui.DragInt("Puzzle Number", ref PuzzleNumber);
        ImGui.End();
    }
}