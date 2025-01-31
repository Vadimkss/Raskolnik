using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
[VolumeComponentMenu("Limitless Glitch/Glitch7")]
public class LimitlessGlitch7 : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);

    [Range(0f, 1f), Tooltip("Effect fade")]
    public ClampedFloatParameter Fade = new ClampedFloatParameter(1f,0f, 1f);
    [Range(0f, 1f), Tooltip("Effect speed.")]
    public ClampedFloatParameter Speed = new ClampedFloatParameter(1f, 0f, 1f);
    [Range(0f, 10f), Tooltip("Block damage offset amount.")]
    public ClampedFloatParameter Amount = new ClampedFloatParameter(1f, 0f, 10f);

    [Space]
    [Tooltip("Time.unscaledTime .")]
    public BoolParameter unscaledTime = new BoolParameter (false);
    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    [Space]
    [Tooltip("Use Global Post Processing Settings to enable or disable Post Processing in scene view or via camera setup. THIS SETTING SHOULD BE TURNED OFF FOR EFFECTS, IN CASE OF USING THEM FOR SEPARATE LAYERS")]
    public BoolParameter GlobalPostProcessingSettings = new BoolParameter(false);


    public bool IsActive() => (bool)enable;

    public bool IsTileCompatible() => false;
}