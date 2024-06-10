using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Animation.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace AnimationPipeline.Pipeline
{
    [ContentTypeWriter]
    internal class SkinnedModelBoneWriter : ContentTypeWriter<SkinnedModelBoneContent>
    {
        protected override void Write(ContentWriter output, SkinnedModelBoneContent value)
        {
            value.Write(output);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(SkinnedModelBoneReader).AssemblyQualifiedName;
        }
    }
}