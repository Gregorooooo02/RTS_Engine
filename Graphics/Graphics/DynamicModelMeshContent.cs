using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Pipeline.Graphics
{
    public class DynamicModelMeshContent
    {   
        protected internal ModelMeshContent Source { get; protected set; }
        
        // Summary:
        //     Gets the mesh name.
        public string Name { get { return Source.Name; } }

        // Summary:
        //     Gets the parent bone.
        [ContentSerializerIgnore]
        public ModelBoneContent ParentBone { get { return Source.ParentBone; } }

        // Summary:
        //     Gets the bounding sphere for this mesh.
        public BoundingSphere BoundingSphere { get { return Source.BoundingSphere; } }

        // Summary:
        //     Gets the children mesh parts associated with this mesh.
        [ContentSerializerIgnore]
        public List<DynamicModelMeshPartContent> MeshParts { get; private set; }
        
        // Summary:
        //     Gets a user defined tag object.
        [ContentSerializer(SharedResource = true)]
        public object Tag { get { return Source.Tag; } set { Source.Tag = value; } }


        public DynamicModelMeshContent(ModelMeshContent source)
        {
            this.Source = source;
            
            //deep clone MeshParts
            MeshParts = new List<DynamicModelMeshPartContent>(source.MeshParts.Count);
            foreach (var mesh in source.MeshParts)
                MeshParts.Add(new DynamicModelMeshPartContent(mesh));
        }

    }
}