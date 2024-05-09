using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class Bone
{
#region Fields
    private Bone _parent = null;
    private List<Bone> _children = new List<Bone>();
    private Matrix _bindTransform = Matrix.Identity;
    private Vector3 _bindScale = Vector3.One;
    private Vector3 _translation = Vector3.Zero;
    private Quaternion _rotation = Quaternion.Identity;
    private Vector3 _scale = Vector3.One;
#endregion
    public string Name = "";
    public Matrix BindTransform { get => _bindTransform; }
    public Matrix SkinTransform { get; set; }
    public Quaternion Rotation { get => _rotation; set => _rotation = value; }
    public Vector3 Translation { get => _translation; set => _translation = value; }
    public Vector3 Scale { get => _scale; set => _scale = value; }
    public Bone Parent { get => _parent; }
    public List<Bone> Children { get => _children; }
    public Matrix AbsoluteTransform = Matrix.Identity;
#region Methods
    public Bone(string name, Matrix bindTransform, Bone parent)
    {
        this.Name = name;
        this._parent = parent;

        if (parent != null)
        {
            parent._children.Add(this);
        }

        this._bindScale = new Vector3(
            bindTransform.Right.Length(),
            bindTransform.Up.Length(),
            bindTransform.Backward.Length()
        );

        bindTransform.Right = bindTransform.Right / _bindScale.X;
        bindTransform.Up = bindTransform.Up / _bindScale.Y;
        bindTransform.Backward = bindTransform.Backward / _bindScale.Z;
        this._bindTransform = bindTransform;
        
        ComputeAbsoluteTransform();
        SkinTransform = Matrix.Invert(AbsoluteTransform);
    }

    public void ComputeAbsoluteTransform()
    {
        Matrix transform = Matrix.CreateScale(Scale * _bindScale) *
                           Matrix.CreateFromQuaternion(Rotation) *
                           Matrix.CreateTranslation(Translation) *
                           _bindTransform;

        if (_parent != null)
        {
            AbsoluteTransform = transform * _parent.AbsoluteTransform;
        }
        else
        {
            AbsoluteTransform = transform;
        }
    }

    public void SetCompleteTransform(Matrix m)
    {
        Matrix setTo = m * Matrix.Invert(BindTransform);

        Translation = setTo.Translation;
        Rotation = Quaternion.CreateFromRotationMatrix(setTo);
    }
#endregion
}