using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class AnimatedModelRenderer
{
#region Fields
    private Model _model = null;
    private ModelProperties _modelProperties = null;
    private List<Bone> _bones = new List<Bone>();
    private Matrix[] _skeleton;
    private Matrix[] _boneTransforms;

    private string _assetName = "";
    private AnimationPlayer _animationPlayer = null;

    private bool _hasSkinnedVertexType = false;
    private bool _hasNormals = false;
    private bool _hasTexCoords = false;
#endregion

#region Properties
    public Model Model { get => _model; }
    public List<Bone> Bones { get => _bones; }
    public List<AnimationClip> AnimationClips { get => _modelProperties.AnimationClips; }

    public bool HasAnimation()
    {
        return _modelProperties != null;
    }
#endregion

    public AnimatedModelRenderer(string assetName)
    {
        this._assetName = assetName;
    }

    public bool LoadContent(ContentManager content)
    {
        bool success = false;
        this._model = content.Load<Model>(_assetName);
        _modelProperties = _model.Tag as ModelProperties;

        if (_modelProperties != null)
        {
            ObtainBones();

            _boneTransforms = new Matrix[_bones.Count];

            _skeleton = new Matrix[_modelProperties.Skeleton.Count];
            success = true;
        }

        VertexElement[] test = _model.Meshes[0].MeshParts[0].VertexBuffer.VertexDeclaration.GetVertexElements();

        for (int index = 0; index < test.Length; index++)
        {
            var t = test[index];
            if (t.VertexElementUsage == VertexElementUsage.BlendIndices)
            {
                _hasSkinnedVertexType = true;
            }
            else if (t.VertexElementUsage == VertexElementUsage.Normal)
            {
                _hasNormals = true;
            }
            else if (t.VertexElementUsage == VertexElementUsage.TextureCoordinate)
            {
                _hasTexCoords = true;
            }
        }

        return success;
    }

    private void ObtainBones()
    {
        _bones.Clear();

        foreach (ModelBone bone in _model.Bones)
        {
            Bone newBone = new Bone(bone.Name, bone.Transform, bone.Parent != null ? _bones[bone.Parent.Index] : null);

            _bones.Add(newBone);
        }
    }

    public Bone FindBone(string name)
    {
        foreach (Bone bone in Bones)
        {
            if (bone.Name == name)
            {
                return bone;
            }
        }

        return null;
    }

#region Animation Management
    public AnimationPlayer PlayClip(AnimationClip clip, bool loop = true, int keyframeStart = 0, int keyframeEnd = 0, int fps = 24)
    {
        _animationPlayer = new AnimationPlayer(clip, this, loop, keyframeStart, keyframeEnd, fps);
        return _animationPlayer;
    }
#endregion

#region Updating
    public void Update()
    {
        _animationPlayer?.Update();
    }
#endregion

#region Drawing
    public void Draw()
    {
        
    }
#endregion
}