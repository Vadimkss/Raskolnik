using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Limitless.Enums;
[Serializable]
public sealed class OffsetAxesModeParameter : VolumeParameter<AxisMode> { };
[Serializable]
public sealed class IntervalModeParameter : VolumeParameter<IntervalMode> { };
[VolumeComponentMenu("Limitless Glitch/Glitch5")]
public class LimitlessGlitch5 : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);

    [Tooltip(" Displacement axis. ")]
    public OffsetAxesModeParameter offsetAxis = new OffsetAxesModeParameter { };
    [Tooltip("shift axis")]
    public OffsetAxesModeParameter shiftMode = new OffsetAxesModeParameter { };
    [Tooltip(" Displacement lines width.")]
    public FloatParameter stretchResolution = new FloatParameter (220);
    [Space]
    [Tooltip(" Infinite - 0. Periodic- 1, Random - 2")]
    public IntervalModeParameter interval = new IntervalModeParameter { };
    [Tooltip("min/max ranom interval, if Interval = 2.")]
    public FloatRangeParameter minMax = new FloatRangeParameter(new Vector2(0.5f, 2.4f),0f,60f);
    [Range(0f, 25f), Tooltip("Glitch periodic interval in seconds.")]
    public ClampedFloatParameter frequency = new ClampedFloatParameter(1f, 0f, 25f);
    [Range(1f, 0f), Tooltip("Glitch decrease rate due time interval (0 - infinite).")]
    public ClampedFloatParameter rate = new ClampedFloatParameter(1f, 0f, 1f);
    [Range(0f, 50f), Tooltip("Effect amount.")]
    public ClampedFloatParameter amount = new ClampedFloatParameter(1f, 0f, 50f);
    [Range(0f, 1f), Tooltip("effect speed.")]
    public ClampedFloatParameter speed = new ClampedFloatParameter(1f,0f, 1f);

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