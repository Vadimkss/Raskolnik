using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch19 : ScriptableRendererFeature
{
    Glitch19Pass GlitchPass;
    public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void Create()
    {
        GlitchPass = new Glitch19Pass(Event);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2019 || UNITY_2020
        GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
        renderer.EnqueuePass(GlitchPass);
    }
    public class Glitch19Pass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Render Glitch19 Effect";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int NoiseTexId = Shader.PropertyToID("_NoiseTex");
        static readonly int AMPLITUDE = Shader.PropertyToID("AMPLITUDE");
        static readonly int ShiftSeed = Shader.PropertyToID("ShiftSeed");
        static readonly int Speed = Shader.PropertyToID("SPEED");
        static readonly int Fade = Shader.PropertyToID("fade");
        static readonly int stop = Shader.PropertyToID("stop");
        static readonly int randAmount = Shader.PropertyToID("randAmount");


        static readonly int TempTargetId = Shader.PropertyToID("Glitch19");

        LimitlessGlitch19 Glitch19;
        Material Glitch19Material;
        RenderTargetIdentifier currentTarget;

        public Glitch19Pass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("LimitlessGlitch/Glitch19");
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            Glitch19Material = CoreUtils.CreateEngineMaterial(shader);

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
            if (Glitch19Material == null)
            {
                Debug.LogError("Material not created.");
                return;
            }
            var stack = VolumeManager.instance.stack;
            Glitch19 = stack.GetComponent<LimitlessGlitch19>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch19.GlobalPostProcessingSettings.value) return;

            if (Glitch19 == null) { return; }
            if (!Glitch19.IsActive()) { return; }

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
            Glitch19Material.SetFloat(ShiftSeed, Glitch19.ShiftSeed.value);
            if (Glitch19.NoiseTexture.value != null)
                Glitch19Material.SetTexture(NoiseTexId, Glitch19.NoiseTexture.value);
            else
                Debug.Log("Limitless Glitch 19 effect; Please insert Noise texture for proper work!");
            Glitch19Material.SetFloat(Speed, Glitch19.speed.value);
            Glitch19Material.SetFloat(AMPLITUDE, Glitch19.amplitude.value);
            Glitch19Material.SetFloat(Fade, Glitch19.fade.value);
            Glitch19Material.SetInt(stop, Glitch19.stop.value ? 1 : 0);
            Glitch19Material.SetFloat(randAmount, 1 - Glitch19.randomActivateAmount.value);


            cmd.Blit(source, destination);
            cmd.Blit(destination, source, Glitch19Material, shaderPass);
        }
    }

}


