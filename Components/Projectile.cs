using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI;
using RTS_Engine.Exceptions;

namespace RTS_Engine;

public class Projectile : Component
{
    public bool Delete = false;
    
    private readonly float _speed = 25.0f;
    private readonly float _damage = 10.0f;

    private readonly float _hitDistance = 1.0f;

    private readonly Agent _target = null;


    public Projectile(GameObject parentObject,Agent target ,float flySpeed, float dmg, float hitDistance, Vector3 startingPos)
    {
        ParentObject = parentObject;
        _target = target;
        _speed = flySpeed;
        _damage = dmg;
        _hitDistance = hitDistance;
        ParentObject.Transform.SetLocalPosition(startingPos);
    }
    
    public Projectile(){}
    public override void Update()
    {
        if(_target != null && !Globals.IsPaused)
        {
            Vector3 direction = _target.Position - ParentObject.Transform.ModelMatrix.Translation;
            if (direction.Length() <= _hitDistance)
            {
                Delete = true;
                _target.AgentData.DealDamage(_damage);
                return;
            }
            direction.Normalize();
            ParentObject.Transform.Move(direction * Globals.DeltaTime * _speed);
        }
    }
    
    public override void Initialize()
    {
        throw new HowDidWeGetHereException("Projectile should never be added to scene using template AddComponent method! If you see this exception stop your vile acts right now!");
    }

    public override string ComponentToXmlString()
    {
        throw new HowDidWeGetHereException("This component as well as the object it's attached to are temporary and should never be serialized!");
    }

    public override void Deserialize(XElement element)
    {
        throw new HowDidWeGetHereException("Like, really, how the f*** did you get here!? Serialize method for this component does not work so you either directly modified save data or caused some divine miracle for it to serialize regardless");
    }

    public override void RemoveComponent()
    {
        //TODO: Add removing from projectile manager when said manager is implemented
        ParentObject.RemoveComponent(this);
    }

    #if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Projectile"))
        {
            ImGui.Checkbox("Projectile active", ref Active);
            ImGui.Text("Projectile alive: " + !Delete);
            ImGui.Text("Projectile target: " + _target.ParentObject.Name);
            ImGui.Text("Speed: " + _speed);
            ImGui.Text("Damage: " + -_damage);
            ImGui.Text("Hit distance: " + _hitDistance);
        }   
    }
    #endif
}