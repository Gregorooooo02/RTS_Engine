using Microsoft.Xna.Framework.Content;

namespace Animation.Pipeline
{
    public class SkinnedModelReader : ContentTypeReader<SkinnedModel>
    {
        protected override SkinnedModel Read(ContentReader input, SkinnedModel existingInstance)
        {
            return SkinnedModel.Read(input);
        }
    }
}