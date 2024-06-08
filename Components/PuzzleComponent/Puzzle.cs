using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class Puzzle : Component
{
    public bool Completed = false;
    
    private Texture2D _puzzleTexture;
    private readonly List<PuzzlePiece> _puzzlePieces = new();

    private List<Point> _backgroundTilesPositions = new();
    private Point size;
    private Color _tileColor = new(64,64,64,255);
    
    #region SelectedPiece
    private PuzzlePiece _selectedPuzzlePiece = null;
    private Point _selectionOffset;
    #endregion
    #region SerializedParameters
    private int _gridSize = 3;
    private int _puzzlePieceSize = 200;
    private int _rimSize = 3;
    private float _snappingDistance = 30.0f;
    private int _innerRimSize = 1;
    #endregion
    
    private int[,] _gridValues;
    
    private Texture2D _background = Globals.Content.Load<Texture2D>("blank");
    private Rectangle _backgroundDest;

    private bool _drawInMiddle = false;
    
    private Point currentSize;

    public event Action PuzzleCompleted;
    
    public Puzzle(){}
    
    private void Resize()
    {
        float ratio = Globals.GraphicsDeviceManager.PreferredBackBufferWidth / (float)currentSize.X;
        ParentObject.Transform.SetLocalPosition(ParentObject.Transform.Pos * ratio);
        _puzzlePieceSize = (int)(_puzzlePieceSize * ratio);
        currentSize = new Point(Globals.GraphicsDeviceManager.PreferredBackBufferWidth,
            Globals.GraphicsDeviceManager.PreferredBackBufferHeight);
        ChangePuzzleParameters();
    }
    
    public void Draw()
    {
        if(!Active) return;
        Globals.SpriteBatch.Draw(_background, _backgroundDest,null,Color.Black,0,Vector2.Zero,SpriteEffects.None,1);
        for (int i = 0; i < _gridSize * _gridSize; i++)
        {
            Globals.SpriteBatch.Draw(_background,new Rectangle(_backgroundTilesPositions[i],size),null,_tileColor,0,Vector2.Zero,SpriteEffects.None,0.99f);
        }
        foreach (PuzzlePiece piece in _puzzlePieces)
        {
            piece.Draw();
        }
    }
    
    public override void Update()
    {
        if(currentSize.X != Globals.GraphicsDeviceManager.PreferredBackBufferWidth || currentSize.Y != Globals.GraphicsDeviceManager.PreferredBackBufferHeight) Resize();
        //Puzzle logic here
        if (Active)
        {
            ActionData exitAction = InputManager.Instance.GetAction(GameAction.EXITPUZZLE);
            if (exitAction is { state: ActionState.RELEASED })
            {
                DeactivatePuzzle();
                Globals.PickingManager.PlayerBuildingPickingActive = true;
                return;
            }
            MouseAction action = InputManager.Instance.GetMouseAction(GameAction.LMB);
            if (action is { duration: 0, state: ActionState.PRESSED })
            {
                float currentDepth = 10;
                foreach (PuzzlePiece piece in _puzzlePieces)
                {
                    if (piece.IsHit(action.StartingPosition) && piece.Depth < currentDepth)
                    {
                        currentDepth = piece.Depth;
                        _selectedPuzzlePiece = piece;
                        _selectionOffset = piece.Position - action.StartingPosition;
                    }
                }

                if (_selectedPuzzlePiece != null)
                {
                    UnregisterSnap(_selectedPuzzlePiece.PieceId);
                    _selectedPuzzlePiece.Snapped = false;
                }
            }

            if (action is { duration: > 0, state: ActionState.PRESSED } && _selectedPuzzlePiece != null)
            {
                _selectedPuzzlePiece.Position = InputManager.Instance.MousePosition + _selectionOffset;
            }

            //Snapping
            if (action is { state: ActionState.RELEASED } && _selectedPuzzlePiece != null)
            {
                Point localOffset = new Point((int)ParentObject.Transform.Pos.X, (int)ParentObject.Transform.Pos.Y);
                Point localPos = _selectedPuzzlePiece.Position - localOffset;
                int row = localPos.Y / _puzzlePieceSize;
                int column = localPos.X / _puzzlePieceSize;
                if (localPos is { X: >= 0, Y: >= 0 } && row < _gridSize && column < _gridSize)
                {
                    Point snapPoint = new Point(column * _puzzlePieceSize + _rimSize, row * _puzzlePieceSize + _rimSize);
                    if (PointDist(localPos,snapPoint) <= _snappingDistance && _gridValues[row,column] == 0)
                    {
                        _selectedPuzzlePiece.Position = snapPoint + localOffset;
                        RegisterSnap(row,column,_selectedPuzzlePiece.PieceId);
                        _selectedPuzzlePiece.Snapped = true;
                    }
                }
                _selectedPuzzlePiece = null;
            }
        }
    }

    private void RegisterSnap(int row, int column, int value)
    {
        _gridValues[row,column] = value;
        if (CheckForWin())
        {
            Completed = true;
            PuzzleCompleted?.Invoke();
                        
            DeactivatePuzzle();
            Console.WriteLine("WIN");
        }
    }

    private bool CheckForWin()
    {
        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                if (_gridValues[j, i] != i * _gridSize + j + 1)
                {
                    return false;
                }
            }
        }
        return true;
    }
    
    private void UnregisterSnap(int value)
    {
        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                if (_gridValues[i, j] == value)
                {
                    _gridValues[i, j] = 0;
                    return;
                }
            }
        }
    }
    
    private float PointDist(Point point1, Point point2)
    {
        Point diff = point1 - point2;
        return MathF.Sqrt((diff.X * diff.X) + (diff.Y * diff.Y));
    }
    
    public void ActivatePuzzle()
    {
        Active = true;
        ChangeActive(ParentObject,true);
        Globals.Renderer.CurrentActivePuzzle = this;
        ChangePuzzleParameters();
    }

    private void ChangeActive(GameObject gameObject,bool state)
    {
        foreach (GameObject child in gameObject.Children)
        {
            child.Active = state;
            ChangeActive(child,state);
        }  
    }
    
    public void DeactivatePuzzle()
    {
        Active = false;
        ChangeActive(ParentObject,false);
        if(Globals.Renderer.CurrentActivePuzzle == this) Globals.Renderer.CurrentActivePuzzle = null;
    }
    
    public override void Initialize()
    {
        _puzzleTexture = AssetManager.DefaultSprite;
        currentSize = new Point(Globals.GraphicsDeviceManager.PreferredBackBufferWidth,
            Globals.GraphicsDeviceManager.PreferredBackBufferHeight);
        ChangePuzzleParameters();
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Puzzle</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<sprite>" + _puzzleTexture.Name + "</sprite>");
        
        builder.Append("<gridSize>" + _gridSize + "</gridSize>");
        
        builder.Append("<pieceSize>" + _puzzlePieceSize + "</pieceSize>");
        
        builder.Append("<rimSize>" + _rimSize + "</rimSize>");
        
        builder.Append("<snappingDist>" + _snappingDistance + "</snappingDist>");
        
        builder.Append("<screenSizeX>" + currentSize.X + "</screenSizeX>");
        builder.Append("<screenSizeY>" + currentSize.Y + "</screenSizeY>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        LoadSprite(element?.Element("sprite")?.Value);
        _gridSize = int.TryParse(element.Element("gridSize")?.Value, out int size) ? size : 3;
        _puzzlePieceSize = int.TryParse(element.Element("pieceSize")?.Value, out int pieceSize) ? pieceSize : 200;
        _rimSize = int.TryParse(element.Element("rimSize")?.Value, out int rimSize) ? rimSize : 3;
        _snappingDistance = float.TryParse(element.Element("snappingDist")?.Value, out float snappingDist) ? snappingDist : 25.0f;
        
        currentSize =
            int.TryParse(element.Element("screenSizeX")?.Value, out int x) &&
            int.TryParse(element.Element("screenSizeY")?.Value, out int y)
                ? new Point(x, y)
                : new Point(1600, 900);
    }

    private void LoadSprite(string name)
    {
        _puzzleTexture = AssetManager.GetSprite(name);
    }
    
    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
        AssetManager.FreeSprite(_puzzleTexture);
    }

    private void ChangePuzzleParameters()
    {
        if (!Active) return;
        _puzzlePieces.Clear();
        _backgroundTilesPositions.Clear();

        if (_drawInMiddle)
        {
            int windowSize = _gridSize * _puzzlePieceSize + _rimSize * 2 + _innerRimSize * (_gridSize - 1);
            int offsetX = (Globals.GraphicsDeviceManager.PreferredBackBufferWidth - windowSize) / 2;
            int offsetY = (Globals.GraphicsDeviceManager.PreferredBackBufferHeight - windowSize) / 2;
            ParentObject.Transform.SetLocalPosition(new Vector3(offsetX, offsetY, 0));
        }

        size = new Point(_puzzlePieceSize);
        
        int offset = _puzzleTexture.Width / _gridSize;
        float depth = 0f;
        float depthStep = 0.9f / (_gridSize * _gridSize);
        Random random = new Random();

        _backgroundDest = new Rectangle((int)ParentObject.Transform.Pos.X, (int)ParentObject.Transform.Pos.Y,
            _gridSize * _puzzlePieceSize + _rimSize * 2 + _innerRimSize * (_gridSize - 1), _gridSize * _puzzlePieceSize + _rimSize * 2 + _innerRimSize * (_gridSize - 1));
        
        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                int posX = random.Next((int)ParentObject.Transform.Pos.X + _rimSize * 2,
                    (int)ParentObject.Transform.Pos.X + _backgroundDest.Width - _puzzlePieceSize - (_rimSize * 2));
                int posY = random.Next((int)ParentObject.Transform.Pos.Y + _rimSize * 2,
                    (int)ParentObject.Transform.Pos.Y + _backgroundDest.Height - _puzzlePieceSize - (_rimSize * 2));

                _puzzlePieces.Add(new PuzzlePiece(_puzzleTexture,
                    new Rectangle(offset * i, offset * j, offset, offset),
                    new Point(posX, posY),
                    _puzzlePieceSize,
                    depth,
                    i * _gridSize + j + 1,
                    0.98f
                    ));
                depth += depthStep;
                
                _backgroundTilesPositions.Add(new Point((int)ParentObject.Transform.Pos.X + (i * _puzzlePieceSize) + i * _innerRimSize + _rimSize,(int)ParentObject.Transform.Pos.Y + (j * _puzzlePieceSize) + j * _innerRimSize + _rimSize));
            }
        }
        _gridValues = new int[_gridSize, _gridSize];
    }

#if DEBUG
    private bool _switchingSprites = false;
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Puzzle"))
        {
            ImGui.Checkbox("Puzzle active", ref Active);
            if (ImGui.Checkbox("Snap to middle", ref _drawInMiddle))
            {
                ChangePuzzleParameters();
            }
            if(ImGui.InputInt("Puzzle grid size", ref _gridSize))
            {
                ChangePuzzleParameters();
            }

            if (ImGui.InputInt("Puzzle piece size", ref _puzzlePieceSize))
            {
                ChangePuzzleParameters();
            }

            ImGui.InputFloat("Minimal snap distance", ref _snappingDistance);
            if(ImGui.InputInt("Rim size", ref _rimSize))
            {
                ChangePuzzleParameters();   
            }

            if (ImGui.Button("Activate"))
            {
                ActivatePuzzle();
            }
            if (ImGui.Button("Deactivate"))
            {
                DeactivatePuzzle();
            }

            if (ImGui.Button("Re-roll"))
            {
                ChangePuzzleParameters();
            }
            ImGui.Text("Current puzzle: " + _puzzleTexture.Name);
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
                    AssetManager.FreeSprite(_puzzleTexture);
                    _puzzleTexture = AssetManager.GetSprite(n);
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