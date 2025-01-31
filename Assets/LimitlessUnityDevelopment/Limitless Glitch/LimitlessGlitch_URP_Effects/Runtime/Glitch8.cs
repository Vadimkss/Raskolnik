using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch8 : ScriptableRendererFeature
{
    Glitch8Pass GlitchPass;
    public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void Create()
    {
        GlitchPass = new Glitch8Pass(Event);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2019 || UNITY_2020
        GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
        renderer.EnqueuePass(GlitchPass);
    }
    public class Glitch8Pass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Render Glitch8 Effect";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int _TimeX = Shader.PropertyToID("_TimeX");
        static readonly int Amount = Shader.PropertyToID("Amount");
        static readonly int Offset = Shader.PropertyToID("Offset");
        static readonly int resM = Shader.PropertyToID("resM");
        static readonly int alpha = Shader.PropertyToID("alpha");
        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
        static readonly int _Mask = Shader.PropertyToID("_Mask");
        static readonly int TempTargetId = Shader.PropertyToID("Glitch8");

        LimitlessGlitch8 Glitch8;
        Material Glitch8Material;
        RenderTargetIdentifier currentTarget;

        private float TimeX = 1.0f;

        public Glitch8Pass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("LimitlessGlitch/Glitch8");
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            Glitch8Material = CoreUtils.CreateEngineMaterial(shader);

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
            if (Glitch8Material == null)
            {
                Debug.LogError("Material not created.");
                return;
            }
            var stack = VolumeManager.instance.stack;
            Glitch8 = stack.GetComponent<LimitlessGlitch8>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch8.GlobalPostProcessingSettings.value) return;

            if (Glitch8 == null) { return; }
            if (!Glitch8.IsActive()) { return; }

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


            if (Glitch8.unscaledTime.value)
                TimeX += Time.unscaledDeltaTime;
            else
                TimeX += Time.deltaTime;
            if (TimeX > 100) TimeX = 0;
            Glitch8Material.SetFloat(_TimeX, TimeX * Glitch8.Speed.value);
            Glitch8Material.SetFloat(Amount, 1-Glitch8.Amount.value);
            Glitch8Material.SetFloat(Offset, Glitch8.Offset.value);
            Glitch8Material.SetFloat(resM, Glitch8.LinesWidth.value);
            Glitch8Material.SetFloat(alpha, Glitch8.alpha.value);
            if (Glitch8.mask.value != null)
            {
                Glitch8Material.SetTexture(_Mask, Glitch8.mask.value);
                Glitch8Material.SetFloat(_FadeMultiplier, 1);
            }
            else
            {
                Glitch8Material.SetFloat(_FadeMultiplier, 0);
            }

            cmd.SetGlobalTexture(MainTexId, source);

            cmd.GetTemporaryRT(destination, Screen.width, Screen.height, 0, FilterMode.Point, RenderTextureFormat.Default);


            cmd.Blit(source, destination);
            cmd.Blit(destination, source, Glitch8Material, 0);
        }
        
    }

}


