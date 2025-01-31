using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch11 : ScriptableRendererFeature
{
	Glitch11Pass GlitchPass;
	public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


	public override void Create()
	{
		GlitchPass = new Glitch11Pass(Event);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
#if UNITY_2019 || UNITY_2020
		GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
		renderer.EnqueuePass(GlitchPass);
	}
	public class Glitch11Pass : ScriptableRenderPass
	{
		static readonly string k_RenderTag = "Render Glitch11 Effect";
		static readonly int MainTexId = Shader.PropertyToID("_MainTex");
		static readonly int linesAmount = Shader.PropertyToID("linesAmount");
		static readonly int amount = Shader.PropertyToID("amount");
		static readonly int Noise = Shader.PropertyToID("noiseA");
		static readonly int speed = Shader.PropertyToID("speed");
		static readonly int TempTargetId = Shader.PropertyToID("Glitch11");
		static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
		static readonly int _Mask = Shader.PropertyToID("_Mask");
		LimitlessGlitch11 Glitch11;
		Material Glitch11Material;
		RenderTargetIdentifier currentTarget;

		public Glitch11Pass(RenderPassEvent evt)
		{
			renderPassEvent = evt;
			var shader = Shader.Find("LimitlessGlitch/Glitch11");
			if (shader == null)
			{
				Debug.LogError("Shader not found.");
				return;
			}
			Glitch11Material = CoreUtils.CreateEngineMaterial(shader);

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
			if (Glitch11Material == null)
			{
				Debug.LogError("Material not created.");
				return;
			}
            var stack = VolumeManager.instance.stack;
			Glitch11 = stack.GetComponent<LimitlessGlitch11>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch11.GlobalPostProcessingSettings.value) return;

            if (Glitch11 == null) { return; }
			if (!Glitch11.IsActive()) { return; }

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
			Glitch11Material.SetFloat(linesAmount, Glitch11.linesAmount.value);
			Glitch11Material.SetFloat(amount, Glitch11.amount.value);
			Glitch11Material.SetFloat(Noise, Glitch11.Noise.value?1:0);
			if (Glitch11.mask.value != null)
			{
				Glitch11Material.SetTexture(_Mask, Glitch11.mask.value);
				Glitch11Material.SetFloat(_FadeMultiplier, 1);
			}
			else
			{
				Glitch11Material.SetFloat(_FadeMultiplier, 0);
			}
			Glitch11Material.SetFloat(speed, Glitch11.speed.value);
			cmd.Blit(source, destination);
			cmd.Blit(destination, source, Glitch11Material, shaderPass);
		}
	}

}


