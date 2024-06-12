using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AnimationPipeline
{
    public class AnimationChannelContent : ReadOnlyCollection<AnimationKeyframeContent>
    {
        internal AnimationChannelContent(IList<AnimationKeyframeContent> list)
            : base(list)
        {
        }
    }
}