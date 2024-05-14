using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Pipeline.Graphics
{
    public class DynamicVertexBufferContent: VertexBufferContent 
    {
        protected internal VertexBufferContent Source { get; protected set; }

        public bool IsWriteOnly = false;
                
        public VertexDeclarationContent VertexDeclaration { get { return Source.VertexDeclaration; } }

        public byte[] VertexData { get { return Source.VertexData; } }

        public DynamicVertexBufferContent(VertexBufferContent source):base()
        {
            Source = source;
        }

        public DynamicVertexBufferContent(VertexBufferContent source, int size):base(size)
        {
            Source = source;
        }


    }
}