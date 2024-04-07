using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ImGuiNET;

namespace RTS_Engine;

public class MeshRenderer : Component
{
    private Model _model;
    private string name;

    //---------------------------Temporary---------------------------
    // Matrix _view = Matrix.CreateLookAt(
    //     new Vector3(0, 4, 20),
    //     new Vector3(0.0f),
    //     Vector3.UnitY);

    // private Matrix _projection =
    //     Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1440.0f / 900.0f, 0.1f, 500.0f);
    //-------------------------------------------------------------------


    public MeshRenderer(GameObject parentObject)
    {
        ParentObject = parentObject;
        Initialize();
    }

    public MeshRenderer(GameObject parentObject, Model model)
    {
        ParentObject = parentObject;
        _model = model;
    }

    public MeshRenderer()
    {
        
    }
    
    public override void Update(){}

    public override void Initialize()
    {
        _model = AssetManager.DefaultModel;
        name = "defaultCube";
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

    public override void Draw(Matrix _view, Matrix _projection)
    {
        if(!Active) return;
        //TODO: Implement globally accessible View and Projection matrices. Then use them here
        DrawModel(_model, ParentObject.Transform.ModelMatrix, _view, _projection);
    }

#if DEBUG

    private bool _switchingModel = false;
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Mesh Renderer"))
        {
            ImGui.Checkbox("Mesh active", ref Active);
            ImGui.Text(name);
            if (ImGui.Button("Switch mesh"))
            {
                _switchingModel = true;
            }
            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
                AssetManager.FreeModel(_model);
            }

            if (_switchingModel)
            {
                ImGui.Begin("Switching models");
                foreach (string n in AssetManager.ModelNames)
                {
                    if (ImGui.Button(n))
                    {
                        AssetManager.FreeModel(_model);
                        _model = AssetManager.GetModel(n);
                        _switchingModel = false;
                        name = n;
                    }
                }
                
                if (ImGui.Button("Cancel selection"))
                {
                    _switchingModel = false;
                }
                ImGui.End();
            }
        }   
    }
#endif
}