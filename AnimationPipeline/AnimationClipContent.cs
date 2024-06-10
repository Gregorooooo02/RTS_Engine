using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace AnimationPipeline
{
    public class AnimationClipContent
    {
        private readonly string name;
        private readonly TimeSpan duration;
        private readonly AnimationChannelContentDictionary channels;

        #region Properties

        public string Name
        {
            get { return name; }
        }

        public TimeSpan Duration
        {
            get { return duration; }
        }

        public AnimationChannelContentDictionary Channels
        {
            get { return channels; }
        }

        #endregion

        internal AnimationClipContent(string name, AnimationChannelContentDictionary channels,
            TimeSpan duration)
        {
            this.name = name;
            this.channels = channels;
            this.duration = duration;
        }

        internal void Write(ContentWriter output)
        {
            output.Write(name);
            output.WriteObject<TimeSpan>(duration);

            // Write animation clip channels
            output.Write(channels.Count);
            foreach (KeyValuePair<string, AnimationChannelContent> pair in channels)
            {
                output.Write(pair.Key);
                AnimationChannelContent animationChannel = pair.Value;

                // Write the animation channel keyframes
                output.Write(animationChannel.Count);
                foreach (AnimationKeyframeContent keyframe in animationChannel)
                {
                    output.WriteObject<TimeSpan>(keyframe.Time);

                    // Write pose
                    output.Write(keyframe.Pose.Translation);
                    output.Write(keyframe.Pose.Orientation);
                    output.Write(keyframe.Pose.Scale);
                }
            }
        }
    }
}