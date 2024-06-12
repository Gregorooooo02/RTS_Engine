using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Animation.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace AnimationPipeline.Pipeline
{
    [ContentTypeWriter]
    internal class AnimationClipWriter : ContentTypeWriter<AnimationClipContent>
    {
        protected override void Write(ContentWriter output, AnimationClipContent value)
        {
            value.Write(output);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(AnimationClipReader).AssemblyQualifiedName;
        }
    }
}