using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
[VolumeComponentMenu("Limitless Glitch/Glitch4")]
public class LimitlessGlitch4 : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);

    [Range(0f, 21f), Tooltip("Glitch periodic interval in seconds.")]
    public ClampedFloatParameter interval = new ClampedFloatParameter(1f,0f, 21f);
    [Range(1f, 0f), Tooltip("Glitch decrease rate due time interval (0 - infinite).")]
    public ClampedFloatParameter rate = new ClampedFloatParameter(1f,0f, 1f);
    [Range(0f, 50f), Tooltip("color shift.")]
    public ClampedFloatParameter RGBSplit = new ClampedFloatParameter(1f,0f, 50f);
    [Range(0f, 1f), Tooltip("effect speed.")]
    public ClampedFloatParameter speed = new ClampedFloatParameter(1f,0f, 1f);
    [Range(0f, 2f), Tooltip("effect amount.")]
    public ClampedFloatParameter amount = new ClampedFloatParameter(1f,0f, 2f);
    [Tooltip(" true - Enables ability to adjust resolution. false - screen resolution.")]
    public BoolParameter customResolution = new BoolParameter (false);
    [Tooltip("jitter resolution.")]
    public Vector2Parameter resolution = new Vector2Parameter (new Vector2(640f, 480f));
    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    [Space]
    [Tooltip("Use Global Post Processing Settings to enable or disable Post Processing in scene view or via camera setup. THIS SETTING SHOULD BE TURNED OFF FOR EFFECTS, IN CASE OF USING THEM FOR SEPARATE LAYERS")]
    public BoolParameter GlobalPostProcessingSettings = new BoolParameter(false);


    public bool IsActive() => (bool)enable;

    public bool IsTileCompatible() => false;
}