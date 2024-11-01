﻿using System;

namespace RTS_Engine;

public class Helpers
{
    public static float LinearInterpolation(float a, float b, float t)
    {
        return a * (1 - t) + b * t; 
    }
    
    public static float CosInterpolation(float a, float b, float t)
    {
        float mu = (1.0f - (float)Math.Cos(t * Math.PI)) / 2.0f;

        return a * (1 - mu) + b * mu;
    }

    public static float Clamp01(float value)
    {
        if (value < 0.0f) return 0.0f;
        if (value > 1.0f) return 1.0f;
        return value;
    }
}
