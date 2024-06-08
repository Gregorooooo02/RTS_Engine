using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = System.Numerics.Vector2;

namespace RTS_Engine;

public class PuzzlePiece
{
    
    #region PlacementParameters
    private readonly float _depth;
    public bool Snapped;
    private readonly float _snappedDepth;
    private Rectangle _dest;
    private Point _position;

    public float Depth => Snapped ? _snappedDepth : _depth;


    public Point Position
    {
        get => _position;
        set
        {
            _position = value;
            RecalculateDest();
        }
    }
    
    private Point _size;
    
    public int Size
    {
        set
        {
            _size = new Point(value);
            RecalculateDest();
        }
    }
    
    private void RecalculateDest()
    {
        _dest = new Rectangle(_position, _size);
    }

    #endregion

    public readonly int PieceId;
    
    private readonly Texture2D _puzzleTexture;
    private readonly Rectangle _sourceRectangle;

    public PuzzlePiece(Texture2D puzzleTexture,Rectangle sourceRectangle, Point startingPos, int pieceSize, float depth, int id, float snappedDepth)
    {
        _puzzleTexture = puzzleTexture;
        _sourceRectangle = sourceRectangle;
        Position = startingPos;
        Size = pieceSize;
        _depth = depth;
        PieceId = id;
        _snappedDepth = snappedDepth;
    }


    public bool IsHit(Point hitPos)
    {
        if (hitPos.X >= _position.X
            && hitPos.X <= _position.X + _size.X
            && hitPos.Y >= _position.Y
            && hitPos.Y <= _position.Y + _size.Y) 
            return true;
        return false;
    }
    
    public void Draw()
    {
        Globals.SpriteBatch.Draw(_puzzleTexture,_dest,_sourceRectangle,Color.White,0,Microsoft.Xna.Framework.Vector2.Zero, SpriteEffects.None,Snapped ? _snappedDepth : _depth);
    }
}