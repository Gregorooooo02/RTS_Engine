using System.Collections.Generic;
using Animation;

namespace AnimationPipeline
{
    public class AnimationClipContentDictionary : ReadOnlyDictionary<string, AnimationClipContent>
    {
        internal AnimationClipContentDictionary(IDictionary<string, AnimationClipContent> dictionary)
            : base(dictionary)
        {
        }
    }
}