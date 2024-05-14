using System;
using Microsoft.Xna.Framework;

namespace Pipeline.Animation
{
    public struct KeyframeContent
    {
        public int Bone;
        public TimeSpan Time;
        public Matrix Transform;

        public KeyframeContent(int bone, TimeSpan time, Matrix transform)
        {
            this.Bone = bone;
            this.Time = time;
            this.Transform = transform;
        }	

        public override string ToString()
        {
            return string.Format("{{Time:{0} Bone:{1}}}",
                new object[] { Time, Bone });
        }
    }
}