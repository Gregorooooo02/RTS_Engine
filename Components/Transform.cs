using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace RTS_Engine;

public class Transform : Component
{
    public override void Initialize(){}
    public Transform(GameObject parent)
    {
        ParentObject = parent;
    }
    //Actual data
    private Vector3 _pos = Vector3.Zero;
    private Vector3 _rot = Vector3.Zero;
    private Vector3 _scl = Vector3.One;

    public Matrix ModelMatrix = Matrix.Identity;

    private bool _isDirty = true;
    
    public override void Update()
    {
        if (_isDirty)
        {
            _isDirty = false;
            ForcedUpdate();
        }
    }
    
    private void ForcedUpdate()
    {
        if (ParentObject.Parent != null)
        {
            ModelMatrix = ParentObject.Parent.Transform.ModelMatrix * GetLocalModelMatrix();
        }
        else
        {
            ModelMatrix = GetLocalModelMatrix();
        }

        

        foreach (GameObject child in ParentObject.Children)
        {
            child.Transform.ForcedUpdate();
        }
    }

    public void SetLocalPosition(Vector3 newPosition)
    {
        _pos = newPosition;
        _isDirty = true;
    }
    
    public void SetLocalRotation(Vector3 newRotation)
    {
        _rot = newRotation;
        _isDirty = true;
    }

    public void SetLocalScale(Vector3 newScale)
    {
        _scl = newScale;
        _isDirty = true;
    }

    public Vector3 GetLocalRotation()
    {
        return _rot;
    }
    
    private Matrix GetLocalModelMatrix()
    {
        Matrix rotationMatrix = Matrix.CreateRotationY((float)(Math.PI / 180) * _rot.Y) *
                                Matrix.CreateRotationX((float)(Math.PI / 180) * _rot.X) *
                                Matrix.CreateRotationZ((float)(Math.PI / 180) * _rot.Z);
        return Matrix.CreateTranslation(_pos) * rotationMatrix * Matrix.CreateScale(_scl);
    }

    public override void Draw()
    {
        
    }
}