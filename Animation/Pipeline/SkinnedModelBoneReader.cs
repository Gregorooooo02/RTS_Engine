using Microsoft.Xna.Framework.Content;

namespace Animation.Pipeline
{
    public class SkinnedModelBoneReader : ContentTypeReader<SkinnedModelBone>
    {
        protected override SkinnedModelBone Read(ContentReader input, SkinnedModelBone existingInstance)
        {
            return SkinnedModelBone.Read(input);
        }
    }
}