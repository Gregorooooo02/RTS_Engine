using System.Collections.Generic;
using Animation;

namespace AnimationPipeline
{
    public class SkinnedModelBoneContentDictionary : ReadOnlyDictionary<string, SkinnedModelBoneContent>
    {
        internal SkinnedModelBoneContentDictionary(
            IDictionary<string, SkinnedModelBoneContent> dictionary)
            : base(dictionary)
        {
        }
    }
}