using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch20 : ScriptableRendererFeature
{
    Glitch20Pass GlitchPass;
    public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void Create()
    {
        GlitchPass = new Glitch20Pass(Event);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2019 || UNITY_2020
        GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
        renderer.EnqueuePass(GlitchPass);
    }
    public class Glitch20Pass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Render Glitch20 Effect";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int Strength = Shader.PropertyToID("AMPLITUDE");
        static readonly int Fade = Shader.PropertyToID("Fade");
        static readonly int stop = Shader.PropertyToID("stop");
        static readonly int speed = Shader.PropertyToID("Speed");
        static readonly int randAmount = Shader.PropertyToID("randAmount");
        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
        static readonly int _Mask = Shader.PropertyToID("_Mask");


        static readonly int TempTargetId = Shader.PropertyToID("Glitch20");

        LimitlessGlitch20 Glitch20;
        Material Glitch20Material;
        RenderTargetIdentifier currentTarget;

        public Glitch20Pass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("LimitlessGlitch/Glitch20");
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            Glitch20Material = CoreUtils.CreateEngineMaterial(shader);

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
            if (Glitch20Material == null)
            {
                Debug.LogError("Material not created.");
                return;
            }
            var stack = VolumeManager.instance.stack;
            Glitch20 = stack.GetComponent<LimitlessGlitch20>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch20.GlobalPostProcessingSettings.value) return;

            if (Glitch20 == null) { return; }
            if (!Glitch20.IsActive()) { return; }

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
            Glitch20Material.SetFloat(Strength, Glitch20.strength.value);
            Glitch20Material.SetFloat(Fade, Glitch20.fade.value);
            Glitch20Material.SetFloat(speed, Glitch20.speed.value);
            Glitch20Material.SetInt(stop, Glitch20.stop.value ? 1 : 0);
            Glitch20Material.SetFloat(randAmount, 1 - Glitch20.randomActivateAmount.value);
            if (Glitch20.mask.value != null)
            {
                Glitch20Material.SetTexture(_Mask, Glitch20.mask.value);
                Glitch20Material.SetFloat(_FadeMultiplier, 1);
            }
            else
            {
                Glitch20Material.SetFloat(_FadeMultiplier, 0);
            }

            cmd.Blit(source, destination);
            cmd.Blit(destination, source, Glitch20Material, shaderPass);
        }
    }

}


