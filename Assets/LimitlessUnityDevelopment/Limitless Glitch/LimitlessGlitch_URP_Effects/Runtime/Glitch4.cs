using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch4 : ScriptableRendererFeature
{
    Glitch4Pass GlitchPass;
    public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void Create()
    {
        GlitchPass = new Glitch4Pass(Event);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2019 || UNITY_2020
        GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
        renderer.EnqueuePass(GlitchPass);
    }
    public class Glitch4Pass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Render Glitch4 Effect";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");

        static readonly int _GlitchInterval = Shader.PropertyToID("_GlitchInterval");
        static readonly int _GlitchRate = Shader.PropertyToID("_GlitchRate");
        static readonly int _RGBSplit = Shader.PropertyToID("_RGBSplit");
        static readonly int _Speed = Shader.PropertyToID("_Speed");
        static readonly int _Amount = Shader.PropertyToID("_Amount");
        static readonly int _Res = Shader.PropertyToID("_Res");

        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
        static readonly int _Mask = Shader.PropertyToID("_Mask");

        static readonly int TempTargetId = Shader.PropertyToID("Glitch4");

        LimitlessGlitch4 Glitch4;
        Material Glitch4Material;
        RenderTargetIdentifier currentTarget;


        public Glitch4Pass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("LimitlessGlitch/Glitch4");
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            Glitch4Material = CoreUtils.CreateEngineMaterial(shader);

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
            if (Glitch4Material == null)
            {
                Debug.LogError("Material not created.");
                return;
            }
            var stack = VolumeManager.instance.stack;
            Glitch4 = stack.GetComponent<LimitlessGlitch4>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch4.GlobalPostProcessingSettings.value) return;

            if (Glitch4 == null) { return; }
            if (!Glitch4.IsActive()) { return; }

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

            Glitch4Material.SetFloat(_GlitchInterval, Glitch4.interval.value);
            Glitch4Material.SetFloat(_GlitchRate, 1-Glitch4.rate.value);
            Glitch4Material.SetFloat(_RGBSplit, Glitch4.RGBSplit.value);
            Glitch4Material.SetFloat(_Speed, Glitch4.speed.value);
            Glitch4Material.SetFloat(_Amount, Glitch4.amount.value);

            if (Glitch4.customResolution.value)
                Glitch4Material.SetVector(_Res, Glitch4.resolution.value);
            else
                Glitch4Material.SetVector(_Res, new Vector2(Screen.width, Screen.height));

            if (Glitch4.mask.value != null)
            {
                Glitch4Material.SetTexture(_Mask, Glitch4.mask.value);
                Glitch4Material.SetFloat(_FadeMultiplier, 1);
            }
            else
            {
                Glitch4Material.SetFloat(_FadeMultiplier, 0);
            }

            cmd.SetGlobalTexture(MainTexId, source);

            cmd.GetTemporaryRT(destination, Screen.width, Screen.height, 0, FilterMode.Point, RenderTextureFormat.Default);


            cmd.Blit(source, destination);
            cmd.Blit(destination, source, Glitch4Material, shaderPass);
        }
        
    }

}


