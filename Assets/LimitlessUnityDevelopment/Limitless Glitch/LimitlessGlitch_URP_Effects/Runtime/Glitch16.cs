using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch16 : ScriptableRendererFeature
{
    Glitch16Pass GlitchPass;
    public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void Create()
    {
        GlitchPass = new Glitch16Pass(Event);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2019 || UNITY_2020
        GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
        renderer.EnqueuePass(GlitchPass);
    }
    public class Glitch16Pass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Render Glitch16 Effect";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int randAmount = Shader.PropertyToID("randAmount");
        static readonly int fade = Shader.PropertyToID("fade");
        static readonly int stop = Shader.PropertyToID("stop");
        static readonly int maxiters = Shader.PropertyToID("maxiters");
        static readonly int speed = Shader.PropertyToID("speed");
        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
        static readonly int _Mask = Shader.PropertyToID("_Mask");

        static readonly int TempTargetId = Shader.PropertyToID("Glitch16");

        LimitlessGlitch16 Glitch16;
        Material Glitch16Material;
        RenderTargetIdentifier currentTarget;

        public Glitch16Pass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("LimitlessGlitch/Glitch16");
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            Glitch16Material = CoreUtils.CreateEngineMaterial(shader);

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
            if (Glitch16Material == null)
            {
                Debug.LogError("Material not created.");
                return;
            }
            var stack = VolumeManager.instance.stack;
            Glitch16 = stack.GetComponent<LimitlessGlitch16>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch16.GlobalPostProcessingSettings.value) return;

            if (Glitch16 == null) { return; }
            if (!Glitch16.IsActive()) { return; }

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
            Glitch16Material.SetFloat(maxiters, Glitch16.amount.value);
            Glitch16Material.SetFloat(fade, Glitch16.fade.value);
            Glitch16Material.SetFloat(speed, Glitch16.speed.value);
            Glitch16Material.SetInt(stop, Glitch16.stop.value ? 1 : 0);
            Glitch16Material.SetFloat(randAmount, 1 - Glitch16.randomActivateAmount.value);
            if (Glitch16.mask.value != null)
            {
                Glitch16Material.SetTexture(_Mask, Glitch16.mask.value);
                Glitch16Material.SetFloat(_FadeMultiplier, 1);
            }
            else
            {
                Glitch16Material.SetFloat(_FadeMultiplier, 0);
            }

            cmd.Blit(source, destination);
            cmd.Blit(destination, source, Glitch16Material, shaderPass);
        }
    }

}


