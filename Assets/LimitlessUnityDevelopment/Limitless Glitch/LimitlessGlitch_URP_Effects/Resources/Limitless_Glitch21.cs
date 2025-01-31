using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
[VolumeComponentMenu("Limitless Glitch/Glitch21")]
public class Limitless_Glitch21 : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);

    public ClampedFloatParameter range = new ClampedFloatParameter(0.05f, 0f, 1f);
    public ClampedFloatParameter noiseQuality = new ClampedFloatParameter(250f,0f, 505f);
    public ClampedFloatParameter noiseIntensity = new ClampedFloatParameter(0.0088f, 0f, 1f);
    public ClampedFloatParameter offsetIntensity = new ClampedFloatParameter(0.02f, 0f, 1f);
    public ClampedFloatParameter colorOffsetIntensity = new ClampedFloatParameter(1.3f, 0f, 10f);
    [Space]
    [Tooltip("Use Global Post Processing Settings to enable or disable Post Processing in scene view or via camera setup. THIS SETTING SHOULD BE TURNED OFF FOR EFFECTS, IN CASE OF USING THEM FOR SEPARATE LAYERS")]
    public BoolParameter GlobalPostProcessingSettings = new BoolParameter(false);


    public bool IsActive() => (bool)enable;

    public bool IsTileCompatible() => false;
}