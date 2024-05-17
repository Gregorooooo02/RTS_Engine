using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace RTS.Animation
{
    public struct Keyframe
    {
        internal int _bone;
        internal TimeSpan _time;
        internal Matrix _transform;
        
        public int Bone 
        { 
            get {return _bone;}
            internal set { _bone = value; }
        }

        public TimeSpan Time
        {
            get { return _time; }
            internal set { _time = value; }
        }

        public Matrix Transform
        {
            get { return _transform; }
            internal set { _transform = value; }
        }
        
        public Keyframe(int bone, TimeSpan time, Matrix transform)
        {
            this._bone = bone;
            this._time = time;
            this._transform = transform;
        }

        public override string ToString()
        {
            return string.Format("{{Time:{0} Bone:{1}}}",
                new object[] { Time, Bone });
        }
    }
}
