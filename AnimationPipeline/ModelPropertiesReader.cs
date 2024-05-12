using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace RTS_Engine;

public class ModelPropertiesReader : ContentTypeReader<ModelProperties>
{
    protected override ModelProperties Read(ContentReader input, ModelProperties instance)
    {
        ModelProperties properties = new ModelProperties();

        properties.Skeleton = input.ReadObject<List<int>>();
        properties.AnimationClips = input.ReadObject<List<AnimationClip>>();
        
        return properties;
    }
}
