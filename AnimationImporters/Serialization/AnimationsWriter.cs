using System;
using System.Collections.Generic;
using RTS.Animation;
using Animation.ContentReaders;
using AnimationImporters.Animation;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace AnimationImporters.Serialization 
{   
    [ContentTypeWriter]
    class AnimationsDataWriter : ContentTypeWriter<AnimationsContent>
    {
        protected override void Write(ContentWriter output, AnimationsContent value)
        {
            WriteClips(output, value.Clips);
            WriteBindPose(output, value.BindPose);
            WriteInvBindPose(output, value.InvBindPose);
            WriteSkeletonHierarchy(output, value.SkeletonHierarchy);
            WriteBoneNames(output, value.BoneNames);
        }

        private void WriteClips(ContentWriter output, Dictionary<string, ClipContent> clips)
        {
            Int32 count = clips.Count;
            output.Write((Int32)count);

            foreach (var clip in clips)
            {
                output.Write(clip.Key);
                output.WriteObject<ClipContent>(clip.Value);
            }            

            return;
        }

        private void WriteBindPose(ContentWriter output, List<Microsoft.Xna.Framework.Matrix> bindPoses)
        {
            Int32 count = bindPoses.Count;
            output.Write((Int32)count);

            for (int i = 0; i < count; i++)
                output.Write(bindPoses[i]);

            return;
        }

        private void WriteInvBindPose(ContentWriter output, List<Microsoft.Xna.Framework.Matrix> invBindPoses)
        {
            Int32 count = invBindPoses.Count;
            output.Write((Int32)count);

            for (int i = 0; i < count; i++)
                output.Write(invBindPoses[i]);

            return;
        }

        private void WriteSkeletonHierarchy(ContentWriter output, List<int> skeletonHierarchy)
        {
            Int32 count = skeletonHierarchy.Count;
            output.Write((Int32)count);

            for (int i = 0; i < count; i++)
                output.Write((Int32)skeletonHierarchy[i]);

            return;
        }
    
        private void WriteBoneNames(ContentWriter output, List<string> boneNames)
        {
            Int32 count = boneNames.Count;
            output.Write((Int32)count);
            
            for (int boneIndex = 0; boneIndex < count; boneIndex++)
            {
                var boneName = boneNames[boneIndex];
                output.Write(boneName);
            }

            return;
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            var type = typeof(Animations);
            var readerType = type.Namespace + ".Animations, " + type.Assembly.FullName;
            return readerType;
          //  return "Animation.Animation.Animations, CreateAetherExtras.Animation";// "tainicom.Aether.Animation.Animations, Aether.Animation";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            var type = typeof(AnimationsReader);
            var readerType = type.Namespace + ".AnimationsReader, " + type.Assembly.FullName;
            return readerType;
        }
    }
        
}