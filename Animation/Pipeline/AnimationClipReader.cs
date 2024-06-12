using Microsoft.Xna.Framework.Content;

namespace Animation.Pipeline
{
    public class AnimationClipReader : ContentTypeReader<AnimationClip>
    {
        protected override AnimationClip Read(ContentReader input, AnimationClip existingInstance)
        {
            return AnimationClip.Read(input);
        }
    }
}