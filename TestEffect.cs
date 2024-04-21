using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class TestEffect : Effect, IEffectMatrices
{
    #region Parameters

    private EffectParameter world;
    private EffectParameter view;
    private EffectParameter projection;
    private EffectParameter WdVwPr;
    

    #endregion
    
    
    protected TestEffect(Effect cloneSource) : base(cloneSource)
    {
        CacheEffectParameters(cloneSource);
    }

    public TestEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
    {
        CacheEffectParameters(null);
    }

    public TestEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
    {
        CacheEffectParameters(null);
    }

    private void CacheEffectParameters(Effect cloneSource)
    {
        world = Parameters["World"];
        view = Parameters["View"];
        projection = Parameters["Projection"];
        WdVwPr = Parameters["WorldViewProjection"];
    }
    
    
    public Matrix Projection { get; set; }
    public Matrix View { get; set; }
    public Matrix World { get; set; }

    protected override void OnApply()
    {
        //world.SetValue(World);
        //view.SetValue(View);
        //projection.SetValue(Projection);


        Matrix wd = World, vw = View, proj = Projection;
        Matrix.Multiply(ref wd, ref vw, out var worldView);
        Matrix.Multiply(ref worldView, ref proj, out var worldViewProj);
        
        WdVwPr.SetValue(worldViewProj);
    }
    
}