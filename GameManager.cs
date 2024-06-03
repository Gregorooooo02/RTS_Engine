using ImGuiNET;

namespace RTS_Engine;

public static class GameManager
{
    public static int MeatNumber;
    public static int PuzzleNumber;
    
    public static void Initialize()
    {
        MeatNumber = 0;
        PuzzleNumber = 0;
    }
    
    public static void Update()
    {
        
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
        ImGui.SliderInt("Meat Number", ref MeatNumber, 0, 1000);
        ImGui.SliderInt("Puzzle Number", ref PuzzleNumber, 0, 1000);
        ImGui.End();
    }
}