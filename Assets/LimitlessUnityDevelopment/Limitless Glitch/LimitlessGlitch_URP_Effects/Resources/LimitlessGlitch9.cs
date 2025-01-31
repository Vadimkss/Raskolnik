using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Limitless Glitch/Glitch9")]

public class LimitlessGlitch9 : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);
	[Tooltip("Effect fade")]
	public ClampedFloatParameter fade = new ClampedFloatParameter(1f, 0f, 1f);
	[Tooltip("Effect amount")]
	public ClampedFloatParameter amount = new ClampedFloatParameter(0.2f, 0f, 1f);
	[Tooltip("Random effect activation for short period of time. 0  = never activate effect, 1 = endless effect")]
	public ClampedFloatParameter randomActivateAmount = new ClampedFloatParameter(.9f, 0f, 1f);
	[Tooltip("Glitch cell size")]
	public Vector2Parameter size = new Vector2Parameter(new Vector2(1f, 1f));
	[Tooltip("Enables light filter to make effect softer")]
	public BoolParameter light = new BoolParameter(false);
	[Space]
	[Tooltip("Mask texture")]
	public TextureParameter mask = new TextureParameter(null);
    [Space]
    [Tooltip("Use Global Post Processing Settings to enable or disable Post Processing in scene view or via camera setup. THIS SETTING SHOULD BE TURNED OFF FOR EFFECTS, IN CASE OF USING THEM FOR SEPARATE LAYERS")]
    public BoolParameter GlobalPostProcessingSettings = new BoolParameter(false);


    public bool IsActive() => (bool)enable;

	public bool IsTileCompatible() => false;
}