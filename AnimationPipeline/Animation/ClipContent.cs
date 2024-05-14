using System;

namespace Pipeline.Animation
{
    public class ClipContent
    {
        public TimeSpan Duration { get; internal set; }
        public KeyframeContent[] Keyframes { get; private set; }

        internal ClipContent(TimeSpan duration, KeyframeContent[] keyframes)
        {
            Duration = duration;
            Keyframes = keyframes;
        }
    }
}