using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = System.Numerics.Vector2;

namespace RTS_Engine;

public class PuzzlePiece
{
    
    #region PlacementParameters
    public float Depth;
    private Rectangle _dest;
    private Point _position;
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
    
    private Texture2D PuzzleTexture;
    private Rectangle SourceRectangle;

    public PuzzlePiece(Texture2D puzzleTexture,Rectangle sourceRectangle, Point startingPos, int pieceSize, float depth)
    {
        PuzzleTexture = puzzleTexture;
        SourceRectangle = sourceRectangle;
        Position = startingPos;
        Size = pieceSize;
        Depth = depth;
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
        Globals.SpriteBatch.Draw(PuzzleTexture,_dest,SourceRectangle,Color.White,0,Microsoft.Xna.Framework.Vector2.Zero, SpriteEffects.None,Depth);
    }
}