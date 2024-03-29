using Microsoft.Xna.Framework;

namespace RTS_Engine;

public interface IScene
{
    public void Load();
    public void Update(GameTime gameTime);
    public void Draw();
}
