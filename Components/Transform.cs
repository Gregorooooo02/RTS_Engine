using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
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
    public Vector3 _pos { get; private set; } = Vector3.Zero;
    public Vector3 _rot { get; private set; } = Vector3.Zero;
    public Vector3 _scl { get; private set; } = Vector3.One;

    public Matrix ModelMatrix { get; private set; } = Matrix.Identity;

    private bool _isDirty = true;
    
    public override void Update()
    {
        if (_isDirty && Active)
        {
            _isDirty = false;
            ForcedUpdate();
        }
    }
    
    private void ForcedUpdate()
    {
        if (ParentObject.Parent != null)
        {
            ModelMatrix = GetLocalModelMatrix() * ParentObject.Parent.Transform.ModelMatrix;
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

    public void Move(Vector3 offset)
    {
        _pos += offset;
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
        return Matrix.CreateScale(_scl) * rotationMatrix *  Matrix.CreateTranslation(_pos);
    }

    public override void Draw(Matrix _view, Matrix _projection)
    {
        
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Transform</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<position>");
        builder.Append("<x>" + _pos.X + "</x>");
        builder.Append("<y>" + _pos.Y + "</y>");
        builder.Append("<z>" + _pos.Z + "</z>");
        builder.Append("</position>");
        
        builder.Append("<rotation>");
        builder.Append("<x>" + _rot.X + "</x>");
        builder.Append("<y>" + _rot.Y + "</y>");
        builder.Append("<z>" + _rot.Z + "</z>");
        builder.Append("</rotation>");
        
        builder.Append("<scale>");
        builder.Append("<x>" + _scl.X + "</x>");
        builder.Append("<y>" + _scl.Y + "</y>");
        builder.Append("<z>" + _scl.Z + "</z>");
        builder.Append("</scale>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        XElement position = element.Element("position");
        SetLocalPosition(new Vector3(float.Parse(position.Element("x").Value), float.Parse(position.Element("y").Value), float.Parse(position.Element("z").Value)));
        XElement rotation = element.Element("rotation");
        SetLocalRotation(new Vector3(float.Parse(rotation.Element("x").Value),float.Parse(rotation.Element("y").Value),float.Parse(rotation.Element("z").Value)));
        XElement scale = element.Element("scale");
        SetLocalScale(new Vector3(float.Parse(scale.Element("x").Value),float.Parse(scale.Element("y").Value),float.Parse(scale.Element("z").Value)));
    }


#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Transform")){
            System.Numerics.Vector3 pos = _pos.ToNumerics();
            if (ImGui.DragFloat3("Position", ref pos,0.1f))
            {
                _isDirty = true;
                _pos = pos;
            }
            System.Numerics.Vector3 rot = _rot.ToNumerics();
            if (ImGui.DragFloat3("Rotation", ref rot))
            {
                _isDirty = true;
                _rot = rot;
            }
            System.Numerics.Vector3 scl = _scl.ToNumerics();
            if (ImGui.DragFloat3("Scale", ref scl,0.05f,0))
            {
                _isDirty = true;
                _scl = scl;
            }
        }
    }
#endif
}