using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using RTS_Engine.Exceptions;
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
    public Vector3 Pos { get; private set; } = Vector3.Zero;
    public Vector3 Rot { get; private set; } = Vector3.Zero;
    public Vector3 Scl { get; private set; } = Vector3.One;

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
        Pos = newPosition;
        _isDirty = true;
    }
    
    public void SetLocalPositionX(float x)
    {
        Pos = new Vector3(x, Pos.Y, Pos.Z);
        _isDirty = true;
    }

    public void Move(Vector3 offset)
    {
        Pos += offset;
        _isDirty = true;
    }

    public void SetLocalRotationY(float angle)
    {
        Rot = new Vector3(Rot.X, angle, Rot.Z); 
        _isDirty = true;
    }
    
    public void SetLocalPositionY(float newHeight)
    {
        Pos = new Vector3(Pos.X, newHeight, Pos.Z); 
        _isDirty = true;
    }
    
    public void SetLocalRotation(Vector3 newRotation)
    {
        Rot = newRotation;
        _isDirty = true;
    }

    public void SetLocalScale(Vector3 newScale)
    {
        Scl = newScale;
        _isDirty = true;
    }
    
    public void SetLocalScaleX(float newScale)
    {
        Scl = new Vector3(newScale, Scl.Y, Scl.Z);
        _isDirty = true;
    }

    public Vector3 GetLocalRotation()
    {
        return Rot;
    }
    
    private Matrix GetLocalModelMatrix()
    {
        Matrix rotationMatrix = Matrix.CreateRotationY((float)(Math.PI / 180) * Rot.Y) *
                                Matrix.CreateRotationX((float)(Math.PI / 180) * Rot.X) *
                                Matrix.CreateRotationZ((float)(Math.PI / 180) * Rot.Z);
        return Matrix.CreateScale(Scl) * rotationMatrix *  Matrix.CreateTranslation(Pos);
    }
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Transform</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<position>");
        builder.Append("<x>" + Pos.X + "</x>");
        builder.Append("<y>" + Pos.Y + "</y>");
        builder.Append("<z>" + Pos.Z + "</z>");
        builder.Append("</position>");
        
        builder.Append("<rotation>");
        builder.Append("<x>" + Rot.X + "</x>");
        builder.Append("<y>" + Rot.Y + "</y>");
        builder.Append("<z>" + Rot.Z + "</z>");
        builder.Append("</rotation>");
        
        builder.Append("<scale>");
        builder.Append("<x>" + Scl.X + "</x>");
        builder.Append("<y>" + Scl.Y + "</y>");
        builder.Append("<z>" + Scl.Z + "</z>");
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

    public override void RemoveComponent()
    {
        throw new HowDidWeGetHereException("Seriously?!. Like, this method should never be called in this class. How the f did you even manage to reach this point?");
    }


#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Transform")){
            System.Numerics.Vector3 pos = Pos.ToNumerics();
            if (ImGui.DragFloat3("Position", ref pos,0.1f))
            {
                _isDirty = true;
                Pos = pos;
            }
            System.Numerics.Vector3 rot = Rot.ToNumerics();
            if (ImGui.DragFloat3("Rotation", ref rot))
            {
                _isDirty = true;
                Rot = rot;
            }
            System.Numerics.Vector3 scl = Scl.ToNumerics();
            if (ImGui.DragFloat3("Scale", ref scl,0.05f,0))
            {
                _isDirty = true;
                Scl = scl;
            }
        }
    }
#endif
}