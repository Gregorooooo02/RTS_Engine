using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Animation
{
    public class SkinnedModelBoneCollection : ReadOnlyCollection<SkinnedModelBone>
    {
        public SkinnedModelBoneCollection(IList<SkinnedModelBone> list)
            : base(list)
        {
        }
    }
}