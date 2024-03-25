using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class MeshRenderer : Component
{
    private Model _model;
    
    public MeshRenderer(GameObject parentObject)
    {
        ParentObject = parentObject;
    }

    public MeshRenderer(GameObject parentObject, Model model)
    {
        ParentObject = parentObject;
        _model = model;
    }

    public MeshRenderer()
    {
        Initialize();
    }
    
    public override void Update()
    {
        //TODO: Implement globally accessible View and Projection matrices. Then use them here
        DrawModel(_model,ParentObject.GetComponent<Transform>().ModelMatrix,Matrix.Identity, Matrix.Identity);
    }

    public override void Initialize()
    {
        //TODO: Load default model here
    }

    //TODO: This method is just copy-pasted from somewhere else. May require some tweaking.
    private void DrawModel(Model model, Matrix wrld, Matrix vw, Matrix proj)
    {
        foreach (ModelMesh mesh in model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.World = wrld;
                effect.View = vw;
                effect.Projection = proj;
            }
            mesh.Draw();
        }
    }
}