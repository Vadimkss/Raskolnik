using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Glitch17 : ScriptableRendererFeature
{
    Glitch17Pass GlitchPass;
    public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void Create()
    {
        GlitchPass = new Glitch17Pass(Event);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2019 || UNITY_2020
        GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
        renderer.EnqueuePass(GlitchPass);
    }
    public class Glitch17Pass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Render Glitch17 Effect";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int Strength = Shader.PropertyToID("Strength");
        static readonly int Size1 = Shader.PropertyToID("Size1");
        static readonly int Speed = Shader.PropertyToID("Speed");
        static readonly int Fade = Shader.PropertyToID("Fade");
        static readonly int stop = Shader.PropertyToID("stop");
        static readonly int randAmount = Shader.PropertyToID("randAmount");
        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
        static readonly int _rgb_split = Shader.PropertyToID("rgb_split");
        static readonly int _Mask = Shader.PropertyToID("_Mask");
        

        static readonly int TempTargetId = Shader.PropertyToID("Glitch17");

        LimitlessGlitch17 Glitch17;
        Material Glitch17Material;
        RenderTargetIdentifier currentTarget;

        public Glitch17Pass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("LimitlessGlitch/Glitch17");
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }

            Glitch17Material = CoreUtils.CreateEngineMaterial(shader);

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
            if (Glitch17Material == null)
            {
                Debug.LogError("Material not created.");
                return;
            }
            var stack = VolumeManager.instance.stack;
            Glitch17 = stack.GetComponent<LimitlessGlitch17>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch17.GlobalPostProcessingSettings.value) return;

            if (Glitch17 == null) { return; }
            if (!Glitch17.IsActive()) { return; }

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
            Glitch17Material.SetFloat(Strength, Glitch17.strength.value);
            Glitch17Material.SetFloat(Speed, Glitch17.speed.value);
            Glitch17Material.SetFloat(Size1, Glitch17.size.value);
            Glitch17Material.SetFloat(Fade, Glitch17.fade.value);
            Glitch17Material.SetInt(stop, Glitch17.stop.value ? 1 : 0);
            Glitch17Material.SetFloat(randAmount, 1 - Glitch17.randomActivateAmount.value);
            Glitch17Material.SetFloat(_rgb_split, Glitch17.RGBSplit.value);
            if (Glitch17.mask.value != null)
            {
                Glitch17Material.SetTexture(_Mask, Glitch17.mask.value);
                Glitch17Material.SetFloat(_FadeMultiplier, 1);
            }
            else
            {
                Glitch17Material.SetFloat(_FadeMultiplier, 0);
            }


            cmd.Blit(source, destination);
            cmd.Blit(destination, source, Glitch17Material, shaderPass);
        }
    }

}


