using System;

namespace RTS_Engine;

public interface INoiseFilter<T, U>
{
    NoiseField<U> Filter(NoiseField<T> noiseField);
}
