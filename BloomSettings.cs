namespace RTS_Engine;

public class BloomSettings
{
    #region Fields
    
    // Name of a preset bloom setting, for display to the user.
    public string Name;
    
    // Controls how bright a pixel needs to be before it will bloom.
    // Zero makes everything bloom equally, while higher values select
    // only brighter colors. Somewhere between 0.25 and 0.5 is good.
    public float BloomThreshold;
    
    // Controls how much blurring is applied to the bloom image.
    // The typical range is from 1 up to 10 or so.
    public float BlurAmount;
    
    // Controls the amount of the bloom and base images that
    // will be mixed into the final scene. Range 0 to 1.
    public float BloomIntensity;
    public float BaseIntensity;
    
    // Independently control the color saturation of the bloom and
    // base images. Zero is totally desaturated, 1.0 leaves saturation
    // unchanged, while higher values increase the saturation level.
    public float BloomSaturation;
    public float BaseSaturation;
    
    #endregion
    
    /// <summary>
    /// Constructs a new bloom settings descriptor.
    /// </summary>
    public BloomSettings(string name, float bloomThreshold, float blurAmount,
                         float bloomIntensity, float baseIntensity,
                         float bloomSaturation, float baseSaturation)
    {
        Name = name;
        BloomThreshold = bloomThreshold;
        BlurAmount = blurAmount;
        BloomIntensity = bloomIntensity;
        BaseIntensity = baseIntensity;
        BloomSaturation = bloomSaturation;
        BaseSaturation = baseSaturation;
    }
    
    /// <summary>
    /// Table of preset bloom settings, used by the sample program.
    /// </summary>
    public static BloomSettings[] PresetSettings =
    {
        //                Name           Thresh  Blur Bloom  Base  BloomSat BaseSat
        new BloomSettings("Default",     0.25f,  4,   1.25f, 1,    1,       1),
        new BloomSettings("Soft",        0,      3,   1,     1,    1,       1),
        new BloomSettings("Desaturated", 0.5f,   8,   2,     1,    0,       1),
        new BloomSettings("Saturated",   0.25f,  4,   2,     1,    2,       0),
        new BloomSettings("Blurry",      0,      2,   1,     0.1f, 1,       1),
        new BloomSettings("Subtle",      0.5f,   2,   1,     1,    1,       1),
    };
}