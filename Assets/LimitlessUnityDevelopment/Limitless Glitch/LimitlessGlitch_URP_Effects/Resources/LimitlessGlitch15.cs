using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
[VolumeComponentMenu("Limitless Glitch/Glitch15")]

public class LimitlessGlitch15 : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
    [Tooltip("Dropout Intensity.")]
    public ClampedFloatParameter dropoutIntensity = new ClampedFloatParameter(0.3f, 0, 2f);
    [Tooltip("Interlace Intesnsity.")]
    public ClampedFloatParameter interlaceIntesnsity = new ClampedFloatParameter(0.21f, 0, 2f);
    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    [Space]
    [Tooltip("Use Global Post Processing Settings to enable or disable Post Processing in scene view or via camera setup. THIS SETTING SHOULD BE TURNED OFF FOR EFFECTS, IN CASE OF USING THEM FOR SEPARATE LAYERS")]
    public BoolParameter GlobalPostProcessingSettings = new BoolParameter(false);

    public bool IsActive() => (bool)enable;
    public bool IsTileCompatible() => false;
}