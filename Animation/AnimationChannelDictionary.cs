using System.Collections.Generic;

namespace Animation
{
    public class AnimationChannelDictionary : ReadOnlyDictionary<string, AnimationChannel>
    {
        public AnimationChannelDictionary(IDictionary<string, AnimationChannel> dictionary)
            : base(dictionary)
        {
        }
    }
}