using System;

namespace Animation
{
    public struct AnimationChannelKeyframe
    {
        private readonly TimeSpan time;
        private readonly Pose pose;

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

        internal AnimationChannelKeyframe(TimeSpan time, Pose pose)
        {
            this.time = time;
            this.pose = pose;
        }
    }
}