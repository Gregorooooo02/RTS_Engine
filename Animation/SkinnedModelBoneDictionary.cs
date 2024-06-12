using System.Collections.Generic;

namespace Animation
{
    public class SkinnedModelBoneDictionary : ReadOnlyDictionary<string, SkinnedModelBone>
    {
        public SkinnedModelBoneDictionary(IDictionary<string, SkinnedModelBone> dictionary)
            : base(dictionary)
        {
        }
    }
}