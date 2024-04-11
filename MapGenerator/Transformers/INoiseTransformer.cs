using System;

namespace RTS_Engine;

public interface INoiseTransformer<T, U>
{
    U Transform(NoiseField<T> field);
}
