using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch14 : ScriptableRendererFeature
{
    Glitch14Pass GlitchPass;
    public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void Create()
    {
        GlitchPass = new Glitch14Pass(Event);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2019 || UNITY_2020
        GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
        renderer.EnqueuePass(GlitchPass);
    }
    public class Glitch14Pass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Render Glitch14 Effect";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int amount = Shader.PropertyToID("amount");
        static readonly int speed = Shader.PropertyToID("speed");
        static readonly int randAmount = Shader.PropertyToID("randAmount");
        static readonly int stop = Shader.PropertyToID("stop");
        static readonly int Randspeed = Shader.PropertyToID("Rspeed");
        static readonly int fade = Shader.PropertyToID("fade");
        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
        static readonly int _Mask = Shader.PropertyToID("_Mask");


        static readonly int TempTargetId = Shader.PropertyToID("Glitch14");

        LimitlessGlitch14 Glitch14;
        Material Glitch14Material;
        RenderTargetIdentifier currentTarget;

        public Glitch14Pass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("LimitlessGlitch/Glitch14");
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            Glitch14Material = CoreUtils.CreateEngineMaterial(shader);
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
            if (Glitch14Material == null)
            {
                Debug.LogError("Material not created.");
                return;
            }
            var stack = VolumeManager.instance.stack;
            Glitch14 = stack.GetComponent<LimitlessGlitch14>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch14.GlobalPostProcessingSettings.value) return;

            if (Glitch14 == null) { return; }
            if (!Glitch14.IsActive()) { return; }

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
            Glitch14Material.SetFloat(amount, Glitch14.amount.value);
            Glitch14Material.SetFloat(speed, Glitch14.speed.value);
            Glitch14Material.SetFloat(Randspeed, Glitch14.RandomActivationSpeed.value);
            Glitch14Material.SetInt(stop, Glitch14.stop.value ? 1 : 0);
            Glitch14Material.SetFloat(randAmount, 1 - Glitch14.randomActivateAmount.value);
            Glitch14Material.SetFloat(fade, Glitch14.fade.value);
            if (Glitch14.mask.value != null)
            {
                Glitch14Material.SetTexture(_Mask, Glitch14.mask.value);
                Glitch14Material.SetFloat(_FadeMultiplier, 1);
            }
            else
            {
                Glitch14Material.SetFloat(_FadeMultiplier, 0);
            }
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, Glitch14Material, shaderPass);
        }
    }

}


