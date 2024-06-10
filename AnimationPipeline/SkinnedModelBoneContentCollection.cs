using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AnimationPipeline
{
    public class SkinnedModelBoneContentCollection : ReadOnlyCollection<SkinnedModelBoneContent>
    {
        internal SkinnedModelBoneContentCollection(IList<SkinnedModelBoneContent> list)
            : base(list)
        {
        }
    }
}