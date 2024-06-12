using System.Collections.Generic;
using Animation;

namespace AnimationPipeline
{
    public class AnimationChannelContentDictionary : ReadOnlyDictionary<string, AnimationChannelContent>
    {
        internal AnimationChannelContentDictionary(
            IDictionary<string, AnimationChannelContent> dictionary)
            : base(dictionary)
        {
        }
    }
}