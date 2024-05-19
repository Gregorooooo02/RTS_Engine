using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class Puzzle : Component
{
    public Texture2D PuzzleTexture;
    private readonly List<PuzzlePiece> _puzzlePieces = new();

    private PuzzlePiece selectedPuzzlePiece = null;
    private Point selectionOffset;
    
    private int _gridSize = 3;
    private int _puzzlePieceSize = 200;
    
    
    public Puzzle(){}
    
    public override void Update()
    {
        //Puzzle logic here
        if (Active)
        {
            MouseAction action = InputManager.Instance.GetMouseAction(GameAction.LMB);
            if (action is { duration: 0, state: ActionState.PRESSED })
            {
                float currentDepth = 10;
                foreach (PuzzlePiece piece in _puzzlePieces)
                {
                    if (piece.IsHit(action.StartingPosition) && piece.Depth < currentDepth)
                    {
                        currentDepth = piece.Depth;
                        selectedPuzzlePiece = piece;
                        selectionOffset = piece.Position - action.StartingPosition;
                    }
                }
            }

            if (action is { duration: > 0, state: ActionState.PRESSED } && selectedPuzzlePiece != null)
            {
                selectedPuzzlePiece.Position = InputManager.Instance.MousePosition + selectionOffset;
            }

            if (action is { state: ActionState.RELEASED })
            {
                selectedPuzzlePiece = null;
            }
        }
    }
    
    public void ActivatePuzzle()
    {
        Active = true;
        Globals.Renderer.CurrentActivePuzzle = this;
    }

    public void DeactivatePuzzle()
    {
        Active = false;
        if(Globals.Renderer.CurrentActivePuzzle == this) Globals.Renderer.CurrentActivePuzzle = null;
    }
    
    public void Draw()
    {
        foreach (PuzzlePiece piece in _puzzlePieces)
        {
            piece.Draw();
        }
    }

    public override void Initialize()
    {
        PuzzleTexture = AssetManager.DefaultSprite;
        ChangePuzzleParameters(_gridSize,_puzzlePieceSize);
    }

    public override string ComponentToXmlString()
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(XElement element)
    {
        throw new System.NotImplementedException();
    }

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
        AssetManager.FreeSprite(PuzzleTexture);
    }

    #if DEBUG
    
    private void ChangePuzzleParameters(int gridSize, int puzzlePieceSize)
    {
        _puzzlePieces.Clear();
        int offset = PuzzleTexture.Width / gridSize;
        float depth = 0;
        float depthStep = 1.0f / (gridSize * gridSize);
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                _puzzlePieces.Add(new PuzzlePiece(PuzzleTexture,
                    new Rectangle(offset * i,offset * j,offset,offset),
                    new Point((puzzlePieceSize + 20) * i,(puzzlePieceSize + 20) * j),puzzlePieceSize,depth));
                depth += depthStep;
            }
        }
    }
    
    private bool _switchingSprites = false;
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Puzzle"))
        {
            ImGui.Checkbox("Puzzle active", ref Active);
            if(ImGui.InputInt("Puzzle grid size", ref _gridSize))
            {
                ChangePuzzleParameters(_gridSize,_puzzlePieceSize);
            }

            if (ImGui.InputInt("Puzzle piece size", ref _puzzlePieceSize))
            {
                ChangePuzzleParameters(_gridSize,_puzzlePieceSize);
            }

            if (ImGui.Button("Activate"))
            {
                ActivatePuzzle();
            }
            ImGui.Text("Current puzzle: " + PuzzleTexture.Name);
            if (ImGui.Button("Switch sprite"))
            {
                _switchingSprites = true;
            }
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }
        if (_switchingSprites)
        {
            ImGui.Begin("Switching sprites");
            foreach (string n in AssetManager.SpriteNames)
            {
                if (ImGui.Button(n))
                {
                    AssetManager.FreeSprite(PuzzleTexture);
                    PuzzleTexture = AssetManager.GetSprite(n);
                    _switchingSprites = false;
                }
            }
            if (ImGui.Button("Cancel selection"))
            {
                _switchingSprites = false;
            }
            ImGui.End();
        }
    }
    #endif
}