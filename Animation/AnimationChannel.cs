using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Animation
{
    public class AnimationChannel : ReadOnlyCollection<AnimationChannelKeyframe>
    {
        internal AnimationChannel(IList<AnimationChannelKeyframe> list)
            : base(list)
        {
        }

        /// <summary>
        /// Return the nearest keyframe for the given time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public int GetKeyframeIndexByTime(TimeSpan time)
        {
            if (Count == 0)
                throw new InvalidOperationException("empty channel");

            /*
            int keyframeIndex = 0;
            while (keyframeIndex < Count && Items[keyframeIndex].Time <= time)
                keyframeIndex++;

            keyframeIndex--;
            */

            int keyframeIndex = 0;
            int startIndex = 0;
            int endIndex = Items.Count - 1;

            while (endIndex >= startIndex)
            {
                keyframeIndex = (startIndex + endIndex) / 2;

                if (Items[keyframeIndex].Time < time)
                    startIndex = keyframeIndex + 1;
                else if (Items[keyframeIndex].Time > time)
                    endIndex = keyframeIndex - 1;
                else
                    break;
            }

            if (Items[keyframeIndex].Time > time)
                keyframeIndex--;

            return keyframeIndex;
        }

        public AnimationChannelKeyframe GetKeyframeByTime(TimeSpan time)
        {
            int index = GetKeyframeIndexByTime(time);
            return Items[index];
        }
    }
}