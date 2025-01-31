using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch21 : ScriptableRendererFeature
{
    Glitch21Pass GlitchPass;
    public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void Create()
    {
        GlitchPass = new Glitch21Pass(Event);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2019 || UNITY_2020
        GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
        renderer.EnqueuePass(GlitchPass);
    }
    public class Glitch21Pass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Render Glitch21 Effect";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int range = Shader.PropertyToID("range");
        static readonly int noiseQuality = Shader.PropertyToID("noiseQuality");
        static readonly int noiseIntensity = Shader.PropertyToID("noiseIntensity");
        static readonly int offsetIntensity = Shader.PropertyToID("offsetIntensity");
        static readonly int colorOffsetIntensity = Shader.PropertyToID("colorOffsetIntensity");
        static readonly int _Mask = Shader.PropertyToID("_Mask");
        static readonly int TempTargetId = Shader.PropertyToID("Glitch3");
        Limitless_Glitch21 Glitch21;
        Material Glitch21Material;
        RenderTargetIdentifier currentTarget;


        public Glitch21Pass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("LimitlessGlitch/Glitch21");
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            Glitch21Material = CoreUtils.CreateEngineMaterial(shader);

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
            if (Glitch21Material == null)
            {
                Debug.LogError("Material not created.");
                return;
            }
            var stack = VolumeManager.instance.stack;
            Glitch21 = stack.GetComponent<Limitless_Glitch21>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch21.GlobalPostProcessingSettings.value) return;

            if (Glitch21 == null) { return; }
            if (!Glitch21.IsActive()) { return; }

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

            Glitch21Material.SetFloat(range, Glitch21.range.value);
            Glitch21Material.SetFloat(noiseQuality, Glitch21.noiseQuality.value);
            Glitch21Material.SetFloat(noiseIntensity, Glitch21.noiseIntensity.value);
            Glitch21Material.SetFloat(offsetIntensity, Glitch21.offsetIntensity.value);
            Glitch21Material.SetFloat(colorOffsetIntensity, Glitch21.colorOffsetIntensity.value);

            cmd.SetGlobalTexture(MainTexId, source);

            cmd.GetTemporaryRT(destination, Screen.width, Screen.height, 0, FilterMode.Point, RenderTextureFormat.Default);


            cmd.Blit(source, destination);
            cmd.Blit(destination, source, Glitch21Material, shaderPass);
        }
        
    }

}


