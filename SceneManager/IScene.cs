using Microsoft.Xna.Framework;

namespace RTS_Engine;

public interface IScene
{
    public void Initialize();
    public void Update(GameTime gameTime);
    public void Draw(Matrix _view, Matrix _projection);
    public void Activate();
    public void Deactivate();
    public void AddGameObject(GameObject gameObject);
    public void RemoveGameObject(GameObject gameObject);
    public void DrawHierarchy();

    public void SaveToFile();
}
