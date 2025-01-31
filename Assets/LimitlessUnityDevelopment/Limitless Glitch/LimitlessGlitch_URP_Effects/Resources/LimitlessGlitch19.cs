using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Limitless Glitch/Glitch19")]
public class LimitlessGlitch19 : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
    public TextureParameter NoiseTexture = new TextureParameter(null);
    [Tooltip("Stops effect without disabling it. Use it in your effect manipulation script")]
    public BoolParameter stop = new BoolParameter(false);
    [Tooltip("Effect Shift Seed value")]
    public ClampedFloatParameter ShiftSeed = new ClampedFloatParameter(16f, 6, 16f);
    [Tooltip("Effect speed")]
    public ClampedFloatParameter speed = new ClampedFloatParameter(0.11f, 0, 1f);
    [Tooltip("Effect amplitude")]
    public ClampedFloatParameter amplitude = new ClampedFloatParameter(0.1f, 0, 1f);
    [Tooltip("Effect Fade")]
    public ClampedFloatParameter fade = new ClampedFloatParameter(1f, 0, 1f);
    [Tooltip("Random effect activation for short period of time. 0  = never activate effect, 1 = endless effect")]
    public ClampedFloatParameter randomActivateAmount = new ClampedFloatParameter(.9f, 0, 1);
    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    [Space]
    [Tooltip("Use Global Post Processing Settings to enable or disable Post Processing in scene view or via camera setup. THIS SETTING SHOULD BE TURNED OFF FOR EFFECTS, IN CASE OF USING THEM FOR SEPARATE LAYERS")]
    public BoolParameter GlobalPostProcessingSettings = new BoolParameter(false);


    public bool IsActive() => (bool)enable;

    public bool IsTileCompatible() => false;

}