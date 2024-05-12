using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class AnimationPlayer
{
#region Fields
    private float _position = 0;
    private AnimationClip _clip = null;
    private BoneInfo[] _boneInfos = null;
    private int _boneCount;
    private AnimatedModelRenderer _model = null;
    private bool _looping = false;
#endregion

#region Properties
    public float Position
    {
        get => _position;
        set
        {
            if (value > Duration)
            {
                value = Duration;
            }

            _position = value;
            foreach (BoneInfo bone in _boneInfos)
            {
                bone.SetPosition(_position);
            }
        }
    }

    [Browsable(false)]
    public AnimationClip Clip { get => _clip; }

    [Browsable(false)]
    public float Duration { get => (float)_clip.Duration; }
    public float StartDuration;
    public float EndDuration;

    [Browsable(false)]
    public bool Looping { get => _looping; set => _looping = value; }

    public AnimationPlayer(AnimationClip clip, AnimatedModelRenderer model, bool looping, int keyframeStart = 0, int keyframeEnd = 0, int fps = 24)
    {
        this._clip = clip;
        this._model = model;
        Looping = looping;

        _boneCount = _clip.Bones.Count;
        _boneInfos = new BoneInfo[_boneCount];

        if (keyframeEnd != 0)
        {
            StartDuration = keyframeStart / (float)fps;
            EndDuration = keyframeEnd / (float)fps;
        }

        for (int b = 0; b < _boneInfos.Length; b++)
        {
            _boneInfos[b] = new BoneInfo(_clip.Bones[b]);
            _boneInfos[b].SetModel(_model);
        }

        Rewind();
    }

    public void Rewind()
    {
        Position = 0;
    }

    public void Update()
    {
        if (EndDuration > 0)
        {
            Position = Position + Globals.TotalSeconds;
            if (_looping && Position >= EndDuration)
            {
                Position = StartDuration;
            }
        }

        Position = Position + Globals.TotalSeconds;
        if (_looping && Position >= Duration)
        {
            Position = 0;
        }
    }

#endregion

    private class BoneInfo
    {
    #region Fields
        private int _currentKeyframe = 0;
        private Bone _assignedBone = null;
        public bool _valid = false;
        private Quaternion rotation;
        private Vector3 translation;
        
        public AnimationClip.Keyframe Keyframe1;
        public AnimationClip.Keyframe Keyframe2;
    #endregion

    #region Properties
        public AnimationClip.Bone ClipBone { get; set; }
        public Bone ModelBone { get => _assignedBone; }
    #endregion

        public BoneInfo(AnimationClip.Bone bone)
        {
            this.ClipBone = bone;
            SetKeyframes();
            SetPosition(0);
        }

        public void SetPosition(float position)
        {
            List<AnimationClip.Keyframe> keyframes = ClipBone.Keyframes;

            if (keyframes.Count == 0)
            {
                return;
            }

            while (position < Keyframe1.Time && _currentKeyframe > 0)
            {
                _currentKeyframe--;
                SetKeyframes();
            }

            while (position >= Keyframe2.Time && _currentKeyframe < ClipBone.Keyframes.Count - 2)
            {
                _currentKeyframe++;
                SetKeyframes();
            }

            if (Keyframe1 == Keyframe2)
            {
                rotation = Keyframe1.Rotation;
                translation = Keyframe1.Translation;
            }
            else
            {
                float t = (float)((position - Keyframe1.Time) / (Keyframe2.Time - Keyframe1.Time));
                rotation = Quaternion.Slerp(Keyframe1.Rotation, Keyframe2.Rotation, t);
                translation = Vector3.Lerp(Keyframe1.Translation, Keyframe2.Translation, t);
            }

            _valid = true;

            if (_assignedBone != null)
            {
                Matrix m = Matrix.CreateFromQuaternion(rotation);
                m.Translation = translation;
                _assignedBone.SetCompleteTransform(m);
            }
        }

        private void SetKeyframes()
        {
            if (ClipBone.Keyframes.Count > 0)
            {
                Keyframe1 = ClipBone.Keyframes[_currentKeyframe];
                Keyframe2 = _currentKeyframe == ClipBone.Keyframes.Count - 1 ? Keyframe1 : ClipBone.Keyframes[_currentKeyframe + 1];
            }
            else 
            {
                Keyframe1 = null;
                Keyframe2 = null;
            }
        }

        public void SetModel(AnimatedModelRenderer model)
        {
            _assignedBone = model.FindBone(ClipBone.Name);
        }
    }
}
