using System;
using Animation;

namespace AnimationPipeline
{
    public class AnimationKeyframeContent : IComparable<AnimationKeyframeContent>
    {
        private TimeSpan time;
        private Pose pose;

        #region Properties

        public TimeSpan Time
        {
            get { return time; }
        }

        public Pose Pose
        {
            get { return pose; }
        }

        #endregion

        internal AnimationKeyframeContent(TimeSpan time, Pose pose)
        {
            this.time = time;
            this.pose = pose;
        }

        public int CompareTo(AnimationKeyframeContent other)
        {
            return time.CompareTo(other.Time);
        }
    }
}