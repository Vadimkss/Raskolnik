using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch13 : ScriptableRendererFeature
{
    Glitch13Pass GlitchPass;
    public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void Create()
    {
        GlitchPass = new Glitch13Pass(Event);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2019 || UNITY_2020
        GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
        renderer.EnqueuePass(GlitchPass);
    }
    public class Glitch13Pass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Render Glitch13 Effect";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int SPEED = Shader.PropertyToID("SPEED");
        static readonly int randAmount = Shader.PropertyToID("randAmount");

        static readonly int fade = Shader.PropertyToID("fade");
        static readonly int val3 = Shader.PropertyToID("val3");
        static readonly int val2 = Shader.PropertyToID("val2");
        static readonly int val1 = Shader.PropertyToID("val1");
        static readonly int stop = Shader.PropertyToID("stop");
        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
        static readonly int _Mask = Shader.PropertyToID("_Mask");

        static readonly int TempTargetId = Shader.PropertyToID("Glitch13");

        LimitlessGlitch13 Glitch13;
        Material Glitch13Material;
        RenderTargetIdentifier currentTarget;

        public Glitch13Pass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("LimitlessGlitch/Glitch13");
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            Glitch13Material = CoreUtils.CreateEngineMaterial(shader);

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
            if (Glitch13Material == null)
            {
                Debug.LogError("Material not created.");
                return;
            }
            var stack = VolumeManager.instance.stack;
            Glitch13 = stack.GetComponent<LimitlessGlitch13>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch13.GlobalPostProcessingSettings.value) return;

            if (Glitch13 == null) { return; }
            if (!Glitch13.IsActive()) { return; }

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
            Glitch13Material.SetFloat(val1, Glitch13.linesWidth.value);
            Glitch13Material.SetFloat(val2, Glitch13.LinesAmount.value);
            Glitch13Material.SetFloat(val3, Glitch13.offset.value);
            Glitch13Material.SetFloat(fade, Glitch13.fade.value);
            Glitch13Material.SetInt(stop, Glitch13.stop.value ? 1 : 0);

            Glitch13Material.SetFloat(SPEED, Glitch13.speed.value);
            Glitch13Material.SetFloat(randAmount, 1 - Glitch13.randomActivateAmount.value);
            if (Glitch13.mask.value != null)
            {
                Glitch13Material.SetTexture(_Mask, Glitch13.mask.value);
                Glitch13Material.SetFloat(_FadeMultiplier, 1);
            }
            else
            {
                Glitch13Material.SetFloat(_FadeMultiplier, 0);
            }
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, Glitch13Material, shaderPass);
        }
    }

}


