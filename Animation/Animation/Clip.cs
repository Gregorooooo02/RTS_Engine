using System;

namespace RTS.Animation
{
    public class Clip
    {
        public TimeSpan Duration { get; internal set; }
        public Keyframe[] Keyframes { get; private set; }

        internal Clip(TimeSpan duration, Keyframe[] keyframes)
        {
            Duration = duration;
            Keyframes = keyframes;
        }
    }
}
