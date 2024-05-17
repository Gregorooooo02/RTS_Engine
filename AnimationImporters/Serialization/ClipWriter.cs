using System;
using System.Collections.Generic;
using RTS.Animation;
using Animation.ContentReaders;
using AnimationImporters.Animation;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace AnimationImporters.Serialization
{
    [ContentTypeWriter]
    class ClipWriter : ContentTypeWriter<ClipContent>
    {
        protected override void Write(ContentWriter output, ClipContent value)
        {
            WriteDuration(output, value.Duration);
            WriteKeyframes(output, value.Keyframes);
        }

        private void WriteDuration(ContentWriter output, TimeSpan duration)
        {
            output.Write(duration.Ticks);
        }

        private void WriteKeyframes(ContentWriter output, IList<KeyframeContent> keyframes)
        {
            Int32 count = keyframes.Count;
            output.Write((Int32)count);

            for (int i = 0; i < count; i++)
            {
                KeyframeContent keyframe = keyframes[i];
                output.Write(keyframe.Bone);
                output.Write(keyframe.Time.Ticks);
                output.Write(keyframe.Transform.M11);
                output.Write(keyframe.Transform.M12);
                output.Write(keyframe.Transform.M13);
                output.Write(keyframe.Transform.M21);
                output.Write(keyframe.Transform.M22);
                output.Write(keyframe.Transform.M23);
                output.Write(keyframe.Transform.M31);
                output.Write(keyframe.Transform.M32);
                output.Write(keyframe.Transform.M33);
                output.Write(keyframe.Transform.M41);
                output.Write(keyframe.Transform.M42);
                output.Write(keyframe.Transform.M43);
            }

            return;
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            var type = typeof(Clip);
            var readerType = type.Namespace + ".Clip, " + type.Assembly.FullName;
            return readerType;
          //  return "Animation.Animation.Clip, Aether.Animation";// "tainicom.Aether.Animation.Clip, Aether.Animation";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            var type = typeof(ClipReader);
            var readerType = type.Namespace + ".ClipReader, " + type.Assembly.FullName;
            return readerType;
        }
    }
    
}