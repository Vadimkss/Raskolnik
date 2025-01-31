using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch10 : ScriptableRendererFeature
{
	Glitch10Pass GlitchPass;
	public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


	public override void Create()
	{
		GlitchPass = new Glitch10Pass(Event);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
#if UNITY_2019 || UNITY_2020
		GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
		renderer.EnqueuePass(GlitchPass);
	}
	public class Glitch10Pass : ScriptableRenderPass
	{
		static readonly string k_RenderTag = "Render Glitch10 Effect";
		static readonly int MainTexId = Shader.PropertyToID("_MainTex");
		static readonly int fade = Shader.PropertyToID("fade");
		static readonly int width = Shader.PropertyToID("width");
		static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
		static readonly int _Mask = Shader.PropertyToID("_Mask");
		static readonly int TempTargetId = Shader.PropertyToID("Glitch10");

		LimitlessGlitch10 Glitch10;
		Material Glitch10Material;
		RenderTargetIdentifier currentTarget;

		public Glitch10Pass(RenderPassEvent evt)
		{
			renderPassEvent = evt;
			var shader = Shader.Find("LimitlessGlitch/Glitch10");
			if (shader == null)
			{
				Debug.LogError("Shader not found.");
				return;
			}
			Glitch10Material = CoreUtils.CreateEngineMaterial(shader);

		}
#if UNITY_2019 || UNITY_2020

#elif UNITY_2021
		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			var renderer = renderingData.cameraData.renderer;
			currentTarget = renderer.cameraColorTarget;
		}
#else
		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			var renderer = renderingData.cameraData.renderer;
			currentTarget = renderer.cameraColorTargetHandle;
		}
#endif

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (Glitch10Material == null)
			{
				Debug.LogError("Material not created.");
				return;
			}
            var stack = VolumeManager.instance.stack;
			Glitch10 = stack.GetComponent<LimitlessGlitch10>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch10.GlobalPostProcessingSettings.value) return;

            if (Glitch10 == null) { return; }
			if (!Glitch10.IsActive()) { return; }
			var cmd = CommandBufferPool.Get(k_RenderTag);
			Render(cmd, ref renderingData);
			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public void Setup(in RenderTargetIdentifier currentTarget)
		{
			this.currentTarget = currentTarget;
		}

		void Render(CommandBuffer cmd, ref RenderingData renderingData)
		{
			ref var cameraData = ref renderingData.cameraData;
			var source = currentTarget;
			int destination = TempTargetId;
			int shaderPass = 0;
			cmd.SetGlobalTexture(MainTexId, source);
			cmd.GetTemporaryRT(destination, Screen.width, Screen.height, 0, FilterMode.Point, RenderTextureFormat.Default);
			Glitch10Material.SetFloat(fade, Glitch10.fade.value);
			Glitch10Material.SetFloat(width, .4f - Glitch10.width.value);
			if (Glitch10.mask.value != null)
			{
				Glitch10Material.SetTexture(_Mask, Glitch10.mask.value);
				Glitch10Material.SetFloat(_FadeMultiplier, 1);
			}
			else
			{
				Glitch10Material.SetFloat(_FadeMultiplier, 0);
			}
			cmd.Blit(source, destination);
			cmd.Blit(destination, source, Glitch10Material, shaderPass);
		}
	}

}


