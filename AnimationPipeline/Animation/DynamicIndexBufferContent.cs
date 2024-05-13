using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AnimationPipeline.Animation
{
    public class DynamicIndexBufferContent: Collection<int>
    {
        protected internal Collection<int> Source { get; protected set; }

        public bool IsWriteOnly = false;
                
        public int Count { get { return Source.Count; } }
        
        public DynamicIndexBufferContent(Collection<int> source)
        {
            Source = source;
        }

        public new IEnumerator<int> GetEnumerator()
        {
            return Source.GetEnumerator();
        }       
    }
}
