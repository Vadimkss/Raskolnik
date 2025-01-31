using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch7 : ScriptableRendererFeature
{
    Glitch7Pass GlitchPass;
    public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void Create()
    {
        GlitchPass = new Glitch7Pass(Event);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2019 || UNITY_2020
        GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
        renderer.EnqueuePass(GlitchPass);
    }
    public class Glitch7Pass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Render Glitch7 Effect";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int _TimeX = Shader.PropertyToID("_TimeX");
        static readonly int Offset = Shader.PropertyToID("Offset");
        static readonly int Fade = Shader.PropertyToID("Fade");
        static readonly int _ScreenResolution = Shader.PropertyToID("_ScreenResolution");
        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
        static readonly int _Mask = Shader.PropertyToID("_Mask");
        static readonly int TempTargetId = Shader.PropertyToID("Glitch7");

        LimitlessGlitch7 Glitch7;
        Material Glitch7Material;
        RenderTargetIdentifier currentTarget;

        private float TimeX = 1.0f;

        public Glitch7Pass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("LimitlessGlitch/Glitch7");
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            Glitch7Material = CoreUtils.CreateEngineMaterial(shader);

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
            if (Glitch7Material == null)
            {
                Debug.LogError("Material not created.");
                return;
            }
            var stack = VolumeManager.instance.stack;
            Glitch7 = stack.GetComponent<LimitlessGlitch7>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch7.GlobalPostProcessingSettings.value) return;

            if (Glitch7 == null) { return; }
            if (!Glitch7.IsActive()) { return; }

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


            if (Glitch7.unscaledTime.value)
                TimeX += Time.unscaledDeltaTime;
            else
                TimeX += Time.deltaTime;
            if (TimeX > 100) TimeX = 0;
            Glitch7Material.SetFloat(_TimeX, TimeX * Glitch7.Speed.value);

            Glitch7Material.SetFloat(Offset, Glitch7.Amount.value);
            Glitch7Material.SetFloat(Fade, Glitch7.Fade.value);
            Glitch7Material.SetVector(_ScreenResolution, new Vector4(Screen.width, Screen.height, 0.0f, 0.0f));

            if (Glitch7.mask.value != null)
            {
                Glitch7Material.SetTexture(_Mask, Glitch7.mask.value);
                Glitch7Material.SetFloat(_FadeMultiplier, 1);
            }
            else
            {
                Glitch7Material.SetFloat(_FadeMultiplier, 0);
            }
            cmd.SetGlobalTexture(MainTexId, source);

            cmd.GetTemporaryRT(destination, Screen.width, Screen.height, 0, FilterMode.Point, RenderTextureFormat.Default);


            cmd.Blit(source, destination);
            cmd.Blit(destination, source, Glitch7Material, 0);
        }
        
    }

}


