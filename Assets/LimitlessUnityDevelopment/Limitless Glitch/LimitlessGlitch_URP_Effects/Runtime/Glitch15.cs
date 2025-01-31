using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch15 : ScriptableRendererFeature
{
    Glitch15Pass GlitchPass;
    public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void Create()
    {
        GlitchPass = new Glitch15Pass(Event);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2019 || UNITY_2020
        GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
        renderer.EnqueuePass(GlitchPass);
    }
    public class Glitch15Pass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Render Glitch15 Effect";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int dropoutIntensity = Shader.PropertyToID("dropoutIntensity");
        static readonly int interlaceIntesnsity = Shader.PropertyToID("interlaceIntesnsity");
        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
        static readonly int _Mask = Shader.PropertyToID("_Mask");

        static readonly int TempTargetId = Shader.PropertyToID("Glitch15");

        LimitlessGlitch15 Glitch15;
        Material Glitch15Material;
        RenderTargetIdentifier currentTarget;

        public Glitch15Pass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("LimitlessGlitch/Glitch15");
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            Glitch15Material = CoreUtils.CreateEngineMaterial(shader);

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
            if (Glitch15Material == null)
            {
                Debug.LogError("Material not created.");
                return;
            }
            var stack = VolumeManager.instance.stack;
            Glitch15 = stack.GetComponent<LimitlessGlitch15>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch15.GlobalPostProcessingSettings.value) return;

            if (Glitch15 == null) { return; }
            if (!Glitch15.IsActive()) { return; }

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
            Glitch15Material.SetFloat(interlaceIntesnsity, Glitch15.interlaceIntesnsity.value);
            Glitch15Material.SetFloat(dropoutIntensity, Glitch15.dropoutIntensity.value);
            if (Glitch15.mask.value != null)
            {
                Glitch15Material.SetTexture(_Mask, Glitch15.mask.value);
                Glitch15Material.SetFloat(_FadeMultiplier, 1);
            }
            else
            {
                Glitch15Material.SetFloat(_FadeMultiplier, 0);
            }
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, Glitch15Material, shaderPass);
        }
    }

}


