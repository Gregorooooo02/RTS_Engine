using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace RTS_Engine;

public class Transform : Component
{
    //Hierarchy
    public Transform ParentTransform = null;
    public List<Transform> Children;
    
    public override void Initialize()
    {
        Children = new List<Transform>();
        ParentTransform = ParentObject.GetComponent<Transform>();
        ParentTransform?.AddChild(this);
    }
    public Transform()
    {
        Initialize();
    }

    ~Transform()
    {
        ParentTransform?.RemoveChild(this);
    }
    
    private void AddChild(Transform transform)
    {
        Children.Add(transform);
    }

    private void RemoveChild(Transform transform)
    {
        Children.Remove(transform);
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
        if (ParentTransform != null)
        {
            ModelMatrix = ParentTransform.ModelMatrix * GetLocalModelMatrix();
        }
        else
        {
            ModelMatrix = GetLocalModelMatrix();
        }

        foreach (var child in Children)
        {
            child.ForcedUpdate();
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
    
    private Matrix GetLocalModelMatrix()
    {
        Matrix rotationMatrix = Matrix.CreateRotationY((float)(Math.PI / 180) * _rot.Y) *
                                Matrix.CreateRotationX((float)(Math.PI / 180) * _rot.X) *
                                Matrix.CreateRotationZ((float)(Math.PI / 180) * _rot.Z);
        return Matrix.CreateTranslation(_pos) * rotationMatrix * Matrix.CreateScale(_scl);
    }
}