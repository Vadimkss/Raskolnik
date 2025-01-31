using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Limitless Glitch/Glitch11")]
public class LimitlessGlitch11 : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);
	[Tooltip("Effect amount")]
	public ClampedFloatParameter amount = new ClampedFloatParameter(0.0021f, 0, .02f);
	[Tooltip("Noise ")]
	public BoolParameter Noise = new BoolParameter(true);
	[Tooltip("Floating Lines Amount")]
	public ClampedFloatParameter linesAmount = new ClampedFloatParameter(1f, 0, 10);
	[Tooltip("Lines speed")]
	public ClampedFloatParameter speed = new ClampedFloatParameter(1f, 0, 10);
	[Space]
	[Tooltip("Mask texture")]
	public TextureParameter mask = new TextureParameter(null);
    [Space]
    [Tooltip("Use Global Post Processing Settings to enable or disable Post Processing in scene view or via camera setup. THIS SETTING SHOULD BE TURNED OFF FOR EFFECTS, IN CASE OF USING THEM FOR SEPARATE LAYERS")]
    public BoolParameter GlobalPostProcessingSettings = new BoolParameter(false);


    public bool IsActive() => (bool)enable;

	public bool IsTileCompatible() => false;
}