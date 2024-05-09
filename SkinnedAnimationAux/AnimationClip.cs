using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class AnimationClip
{
    public class Keyframe
    {
        public double Time;
        public Quaternion Rotation;
        public Vector3 Translation;

        public Matrix Transform
        {
            get => Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Translation);
            set 
            {
                Matrix transform = value;
                transform.Right = Vector3.Normalize(transform.Right);
                transform.Up = Vector3.Normalize(transform.Up);
                transform.Backward = Vector3.Normalize(transform.Backward);

                Rotation = Quaternion.CreateFromRotationMatrix(transform);
                Translation = transform.Translation;
            }
        }
    }

    public class Bone
    {
        private string _name = "";
        private List<Keyframe> _keyframes = new List<Keyframe>();
        public string Name { get => _name; set => _name = value; }
        public List<Keyframe> Keyframes { get => _keyframes; }
    }

    private List<Bone> _bones = new List<Bone>();
    public string Name;
    public double Duration;
    public List<Bone> Bones { get => _bones; }
}
