using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch12 : ScriptableRendererFeature
{
	Glitch12Pass GlitchPass;
	public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


	public override void Create()
	{
		GlitchPass = new Glitch12Pass(Event);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
#if UNITY_2019 || UNITY_2020
        GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
		renderer.EnqueuePass(GlitchPass);
	}
	public class Glitch12Pass : ScriptableRenderPass
	{
		static readonly string k_RenderTag = "Render Glitch12 Effect";
		static readonly int MainTexId = Shader.PropertyToID("_MainTex");
		static readonly int linesAmount = Shader.PropertyToID("linesAmount");
		static readonly int randAmount = Shader.PropertyToID("randAmount");
		static readonly int amount = Shader.PropertyToID("amount");
		static readonly int fade = Shader.PropertyToID("fade");
		static readonly int speed = Shader.PropertyToID("speed");
		static readonly int stop = Shader.PropertyToID("stop");
		static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
		static readonly int _Mask = Shader.PropertyToID("_Mask");

		static readonly int TempTargetId = Shader.PropertyToID("Glitch12");

		LimitlessGlitch12 Glitch12;
		Material Glitch12Material;
		RenderTargetIdentifier currentTarget;

		public Glitch12Pass(RenderPassEvent evt)
		{
			renderPassEvent = evt;
			var shader = Shader.Find("LimitlessGlitch/Glitch12");
			if (shader == null)
			{
				Debug.LogError("Shader not found.");
				return;
			}
			Glitch12Material = CoreUtils.CreateEngineMaterial(shader);

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
			if (Glitch12Material == null)
			{
				Debug.LogError("Material not created.");
				return;
			}
            var stack = VolumeManager.instance.stack;
			Glitch12 = stack.GetComponent<LimitlessGlitch12>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch12.GlobalPostProcessingSettings.value) return;

            if (Glitch12 == null) { return; }
			if (!Glitch12.IsActive()) { return; }

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
			Glitch12Material.SetFloat(amount, Glitch12.amount.value);
			Glitch12Material.SetFloat(speed, Glitch12.size.value);
			Glitch12Material.SetFloat(fade, Glitch12.fade.value);
			Glitch12Material.SetInt(stop, Glitch12.stop.value?1:0);
			Glitch12Material.SetFloat(randAmount, 1 - Glitch12.randomActivateAmount.value);
						if (Glitch12.mask.value != null)
			{
				Glitch12Material.SetTexture(_Mask, Glitch12.mask.value);
                Glitch12Material.SetFloat(_FadeMultiplier, 1);
			}
			else
			{
                Glitch12Material.SetFloat(_FadeMultiplier, 0);
			}
			cmd.Blit(source, destination);
			cmd.Blit(destination, source, Glitch12Material, shaderPass);
		}
	}

}


