using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace RTS_Engine;

public class AnimationProcessor : ModelProcessor 
{
    private ModelContent _model;
    private ModelProperties _modelProperties = new ModelProperties();

    private Dictionary<MaterialContent, SkinnedMaterialContent> _toSkinnedMaterial = new Dictionary<MaterialContent, SkinnedMaterialContent>();

    public override ModelContent Process(NodeContent input, ContentProcessorContext context)
    {
        BoneContent skeleton = ProcessSkeleton(input);
        SwapSkinnedMaterial(input);
        _model = base.Process(input, context);

        ProcessAnimations(_model, input, context);

        return _model;
    }

    private BoneContent ProcessSkeleton(NodeContent input)
    {
        BoneContent skeleton = MeshHelper.FindSkeleton(input);
        if (skeleton == null)
            throw new InvalidContentException("Input skeleton not found.");
        
        FlattenTransforms(input, skeleton);

        TrimSkeleton(skeleton);

        List<NodeContent> nodes = FlattenHierarchy(input);
        IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

        Dictionary<NodeContent, int> nodeToIndex = new Dictionary<NodeContent, int>();
        for (int i = 0; i < nodes.Count; i++)
            nodeToIndex.Add(nodes[i], i);
        
        foreach (BoneContent bone in bones)
        {
            _modelProperties.Skeleton.Add(nodeToIndex[bone]);
        }

        return skeleton;
    }

    private List<NodeContent> FlattenHierarchy(NodeContent item)
    {
        List<NodeContent> nodes = new List<NodeContent>();
        nodes.Add(item);

        foreach (NodeContent child in item.Children)
        {
            FlattenHierarchy(nodes, child);
        }

        return nodes;
    }

    private void FlattenHierarchy(List<NodeContent> nodes, NodeContent item)
    {
        nodes.Add(item);

        foreach (NodeContent child in item.Children)
        {
            FlattenHierarchy(nodes, child);
        }
    }

    private void FlattenTransforms(NodeContent node, BoneContent skeleton)
    {
        foreach (NodeContent child in node.Children)
        {
            if (child == skeleton)
            {
                continue;
            }

            if (IsSkinned(child))
            {
                FlattenAllTransforms(child);
            }
        }
    }

    private void FlattenAllTransforms(NodeContent node)
    {
        MeshHelper.TransformScene(node, node.Transform);

        node.Transform = Matrix.Identity;

        foreach (NodeContent child in node.Children)
        {
            FlattenAllTransforms(child);
        }
    }

    private void TrimSkeleton(NodeContent skeleton)
    {
        List<NodeContent> toRemove = new List<NodeContent>();

        foreach (NodeContent child in skeleton.Children)
        {
            if (child.Name.EndsWith("Nub") || child.Name.EndsWith("Footsteps"))
            {
                toRemove.Add(child);
            }
            else
            {
                TrimSkeleton(child);
            }
        }

        foreach (NodeContent child in toRemove)
        {
            skeleton.Children.Remove(child);
        }
    }

    private bool IsSkinned(NodeContent node)
    {
        MeshContent mesh = node as MeshContent;

        if (mesh != null)
        {
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                foreach (VertexChannel vchannel in geometry.Vertices.Channels)
                {
                    if (vchannel is VertexChannel<BoneWeightCollection>)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void SwapSkinnedMaterial(NodeContent node)
    {
        MeshContent mesh = node as MeshContent;

        if (mesh != null)
        {
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                bool swap = false;
                foreach (VertexChannel vchannel in geometry.Vertices.Channels)
                {
                    if (vchannel is VertexChannel<BoneWeightCollection>)
                    {
                        swap = true;
                        break;
                    }
                }

                if (swap)
                {
                    if (_toSkinnedMaterial.ContainsKey(geometry.Material)) 
                    {
                        geometry.Material = _toSkinnedMaterial[geometry.Material];
                    }
                    else
                    {
                        SkinnedMaterialContent smaterial = new SkinnedMaterialContent();
                        BasicMaterialContent bmaterial = geometry.Material as BasicMaterialContent;

                        smaterial.Alpha = bmaterial.Alpha;
                        smaterial.DiffuseColor = bmaterial.DiffuseColor;
                        smaterial.EmissiveColor = bmaterial.EmissiveColor;
                        smaterial.SpecularColor = bmaterial.SpecularColor;
                        smaterial.SpecularPower = bmaterial.SpecularPower;
                        smaterial.Texture = bmaterial.Texture;
                        smaterial.WeightsPerVertex = 4;

                        _toSkinnedMaterial[geometry.Material] = smaterial;
                        geometry.Material = smaterial;
                    }
                }
            }
        }

        foreach (NodeContent child in node.Children)
        {
            SwapSkinnedMaterial(child);
        }
    }

#region Animation Support

    private Dictionary<string, int> _bones = new Dictionary<string, int>();
    private Matrix[] _boneTransforms;
    private Dictionary<string, AnimationClip> _clips = new Dictionary<string, AnimationClip>();

    private void ProcessAnimations(ModelContent model, NodeContent input, ContentProcessorContext context)
    {
        for (int i = 0 ; i < model.Bones.Count; i++)
        {
            _bones[model.Bones[i].Name] = i;
        }

        _boneTransforms = new Matrix[model.Bones.Count];

        ProcessAnimationRecursive(input);

        if (_modelProperties.AnimationClips.Count == 0)
        {
            AnimationClip clip = new AnimationClip();
            _modelProperties.AnimationClips.Add(clip);

            string clipName = "Take 001";

            _clips[clipName] = clip;
            clip.Name = clipName;

            foreach (ModelBoneContent bone in model.Bones)
            {
                AnimationClip.Bone clipBone = new AnimationClip.Bone();
                clipBone.Name = bone.Name;

                clip.Bones.Add(clipBone);
            }
        }

        foreach (AnimationClip clip in _modelProperties.AnimationClips)
        {
            for (int b = 0; b < _bones.Count; b++)
            {
                List<AnimationClip.Keyframe> keyframes = clip.Bones[b].Keyframes;

                if (keyframes.Count == 0 || keyframes[0].Time > 0)
                {
                    AnimationClip.Keyframe keyframe = new AnimationClip.Keyframe();
                    keyframe.Time = 0;
                    keyframe.Transform = _boneTransforms[b];
                    keyframes.Insert(0, keyframe);
                }
            }
        }
    }

    private void ProcessAnimationRecursive(NodeContent input)
    {
        int inputBoneIndex;

        if (_bones.TryGetValue(input.Name, out inputBoneIndex))
        {
            _boneTransforms[inputBoneIndex] = input.Transform;
        }

        foreach (KeyValuePair<string, AnimationContent> animation in input.Animations)
        {
            AnimationClip clip;
            string clipName = animation.Key;

            if (!_clips.TryGetValue(clipName, out clip))
            {
                clip = new AnimationClip();
                _modelProperties.AnimationClips.Add(clip);
                _clips[clipName] = clip;

                clip.Name = clipName;

                foreach (ModelBoneContent bone in _model.Bones)
                {
                    AnimationClip.Bone clipBone = new AnimationClip.Bone();
                    clipBone.Name = bone.Name;

                    clip.Bones.Add(clipBone);
                }
            }

            if (animation.Value.Duration.TotalSeconds > clip.Duration)
            {
                clip.Duration = animation.Value.Duration.TotalSeconds;
            }

            foreach (KeyValuePair<string, AnimationChannel> channel in animation.Value.Channels)
            {
                int boneIndex;

                if (!_bones.TryGetValue(channel.Key, out boneIndex))
                {
                    continue;
                }

                if (UselessAnimationTest(boneIndex))
                {
                    continue;
                }

                LinkedList<AnimationClip.Keyframe> keyframes = new LinkedList<AnimationClip.Keyframe>();

                foreach (AnimationKeyframe keyframe in channel.Value)
                {
                    Matrix transform = keyframe.Transform;

                    AnimationClip.Keyframe newKeyframe = new AnimationClip.Keyframe();
                    newKeyframe.Time = keyframe.Time.TotalSeconds;
                    newKeyframe.Transform = transform;

                    keyframes.AddLast(newKeyframe);
                }

                LinearKeyframeReduction(keyframes);

                foreach (AnimationClip.Keyframe keyframe in keyframes)
                {
                    clip.Bones[boneIndex].Keyframes.Add(keyframe);
                }
            }
        }

        foreach (NodeContent child in input.Children)
        {
            ProcessAnimationRecursive(child);
        }
    }

    private const float TinyLength = 1e-8f;
    private const float TinyCosAngle = 0.9999999f;

    private void LinearKeyframeReduction(LinkedList<AnimationClip.Keyframe> keyframes)
    {
        if (keyframes.Count < 3)
        {
            return;
        }

        for (LinkedListNode<AnimationClip.Keyframe> node = keyframes.First.Next; ;)
        {
            LinkedListNode<AnimationClip.Keyframe> next = node.Next;

            if (next == null)
            {
                break;
            }

            AnimationClip.Keyframe a = node.Previous.Value;
            AnimationClip.Keyframe b = node.Value;
            AnimationClip.Keyframe c = next.Value;

            float t = (float)((node.Value.Time - node.Previous.Value.Time) /
                (next.Value.Time - node.Previous.Value.Time));

            Vector3 translation = Vector3.Lerp(a.Translation, c.Translation, t);
            Quaternion rotation = Quaternion.Slerp(a.Rotation, c.Rotation, t);

            if ((translation - b.Translation).LengthSquared() < TinyLength &&
                Quaternion.Dot(rotation, b.Rotation) > TinyCosAngle)
            {
                keyframes.Remove(node);
            }

            node = next;
        }
    }

    private bool UselessAnimationTest(int boneIndex)
    {
        foreach (ModelMeshContent mesh in _model.Meshes)
        {
            if (mesh.ParentBone.Index == boneIndex)
            {
                return false;
            }
        }

        foreach (int b in _modelProperties.Skeleton)
        {
            if (b == boneIndex)
            {
                return false;
            }
        }

        return true;
    }

#endregion
}
