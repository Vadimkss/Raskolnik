using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch18 : ScriptableRendererFeature
{
    Glitch18Pass GlitchPass;
    public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void Create()
    {
        GlitchPass = new Glitch18Pass(Event);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2019 || UNITY_2020
        GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
        renderer.EnqueuePass(GlitchPass);
    }
    public class Glitch18Pass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Render Glitch18 Effect";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int m_offset = Shader.PropertyToID("m_offset");
        static readonly int Speed = Shader.PropertyToID("Speed");
        static readonly int Fade = Shader.PropertyToID("fade");
        static readonly int stop = Shader.PropertyToID("stop");
        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
        static readonly int _Mask = Shader.PropertyToID("_Mask");

        static readonly int randAmount = Shader.PropertyToID("randAmount");


        static readonly int TempTargetId = Shader.PropertyToID("Glitch18");

        LimitlessGlitch18 Glitch18;
        Material Glitch18Material;
        RenderTargetIdentifier currentTarget;

        public Glitch18Pass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("LimitlessGlitch/Glitch18");
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            Glitch18Material = CoreUtils.CreateEngineMaterial(shader);

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
            if (Glitch18Material == null)
            {
                Debug.LogError("Material not created.");
                return;
            }
            var stack = VolumeManager.instance.stack;
            Glitch18 = stack.GetComponent<LimitlessGlitch18>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch18.GlobalPostProcessingSettings.value) return;

            if (Glitch18 == null) { return; }
            if (!Glitch18.IsActive()) { return; }

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
    
            Glitch18Material.SetFloat(Speed, Glitch18.speed.value);

            Glitch18Material.SetFloat(Fade, Glitch18.fade.value);
            Glitch18Material.SetFloat(m_offset, Glitch18.offset.value);
            Glitch18Material.SetInt(stop, Glitch18.stop.value ? 1 : 0);
            Glitch18Material.SetFloat(randAmount, 1 - Glitch18.randomActivateAmount.value);
            if (Glitch18.mask.value != null)
            {
                Glitch18Material.SetTexture(_Mask, Glitch18.mask.value);
                Glitch18Material.SetFloat(_FadeMultiplier, 1);
            }
            else
            {
                Glitch18Material.SetFloat(_FadeMultiplier, 0);
            }

            cmd.Blit(source, destination);
            cmd.Blit(destination, source, Glitch18Material, shaderPass);
        }
    }

}


