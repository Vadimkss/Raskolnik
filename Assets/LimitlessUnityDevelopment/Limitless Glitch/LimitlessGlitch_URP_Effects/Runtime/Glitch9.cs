using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch9 : ScriptableRendererFeature
{
	Glitch9Pass GlitchPass;
	public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;
	public override void Create()
	{
		GlitchPass = new Glitch9Pass(Event);
	}
	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
#if UNITY_2019 || UNITY_2020
		GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
		renderer.EnqueuePass(GlitchPass);
	}
	public class Glitch9Pass : ScriptableRenderPass
	{
		static readonly string k_RenderTag = "Render Glitch9 Effect";
		static readonly int MainTexId = Shader.PropertyToID("_MainTex");
		static readonly int fade = Shader.PropertyToID("fade");
		static readonly int randAmount = Shader.PropertyToID("randAmount");
		static readonly int amount = Shader.PropertyToID("amount");
		static readonly int size = Shader.PropertyToID("size");
		static readonly int light = Shader.PropertyToID("light");
		static readonly int TempTargetId = Shader.PropertyToID("Glitch9");
		static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
		static readonly int _Mask = Shader.PropertyToID("_Mask");

		LimitlessGlitch9 Glitch9;
		Material Glitch9Material;
		RenderTargetIdentifier currentTarget;

		public Glitch9Pass(RenderPassEvent evt)
		{
			renderPassEvent = evt;
			var shader = Shader.Find("LimitlessGlitch/Glitch9");
			if (shader == null)
			{
				Debug.LogError("Shader not found.");
				return;
			}
			Glitch9Material = CoreUtils.CreateEngineMaterial(shader);

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
			if (Glitch9Material == null)
			{
				Debug.LogError("Material not created.");
				return;
			}
            var stack = VolumeManager.instance.stack;
			Glitch9 = stack.GetComponent<LimitlessGlitch9>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch9.GlobalPostProcessingSettings.value) return;

            if (Glitch9 == null) { return; }
			if (!Glitch9.IsActive()) { return; }

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
			Glitch9Material.SetFloat(randAmount, 1 - Glitch9.randomActivateAmount.value);
			Glitch9Material.SetFloat(fade, Glitch9.fade.value);
			Glitch9Material.SetFloat(amount, Glitch9.amount.value);
			Glitch9Material.SetVector(size, Glitch9.size.value);
			Glitch9Material.SetFloat(light, Glitch9.light.value ? 1 : 0);
			if (Glitch9.mask.value != null)
			{
				Glitch9Material.SetTexture(_Mask, Glitch9.mask.value);
				Glitch9Material.SetFloat(_FadeMultiplier, 1);
			}
			else
			{
				Glitch9Material.SetFloat(_FadeMultiplier, 0);
			}
			cmd.Blit(source, destination);
			cmd.Blit(destination, source, Glitch9Material, shaderPass);
		}
	}

}


