using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Limitless.Enums;

public class Glitch6 : ScriptableRendererFeature
{
    Glitch6Pass GlitchPass;
    public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void Create()
    {
        GlitchPass = new Glitch6Pass(Event);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2019 || UNITY_2020
        GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
        renderer.EnqueuePass(GlitchPass);
    }
    public class Glitch6Pass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Render Glitch6 Effect";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int time_ = Shader.PropertyToID("time_");
        static readonly int jitterVFreq = Shader.PropertyToID("jitterVFreq");
        static readonly int jitterVRate = Shader.PropertyToID("jitterVRate");
        static readonly int jitterVAmount = Shader.PropertyToID("jitterVAmount");
        static readonly int jitterVSpeed = Shader.PropertyToID("jitterVSpeed");
        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
        static readonly int _Mask = Shader.PropertyToID("_Mask");
        static readonly int TempTargetId = Shader.PropertyToID("Glitch6");
        LimitlessGlitch6 Glitch6;
        Material Glitch6Material;
        RenderTargetIdentifier currentTarget;

        private float _time;
        private float tempVFR;
        float t;

        public Glitch6Pass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("LimitlessGlitch/Glitch6");
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            Glitch6Material = CoreUtils.CreateEngineMaterial(shader);

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
            if (Glitch6Material == null)
            {
                Debug.LogError("Material not created.");
                return;
            }
            var stack = VolumeManager.instance.stack;
            Glitch6 = stack.GetComponent<LimitlessGlitch6>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch6.GlobalPostProcessingSettings.value) return;

            if (Glitch6 == null) { return; }
            if (!Glitch6.IsActive()) { return; }

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

            if (Glitch6.interval.value == IntervalMode.Random)
            {
                t -= Time.deltaTime;
                if (t <= 0)
                {
                    tempVFR = UnityEngine.Random.Range(Glitch6.minMax.value.x, Glitch6.minMax.value.y);
                    t = tempVFR;
                }
            }

            if (Glitch6.unscaledTime.value) { _time = Time.unscaledTime; }
            else _time = Time.time;

            Glitch6Material.SetFloat(time_, _time);

            Glitch6Material.EnableKeyword("VHS_JITTER_V_ON");

            if (Glitch6.interval.value == IntervalMode.Infinite) Glitch6Material.EnableKeyword("JITTER_V_CUSTOM"); else Glitch6Material.DisableKeyword("JITTER_V_CUSTOM");
            if (Glitch6.interval.value == IntervalMode.Random)
                Glitch6Material.SetFloat(jitterVFreq, tempVFR);
            else
                Glitch6Material.SetFloat(jitterVFreq, Glitch6.frequency.value);

            Glitch6Material.SetFloat(jitterVRate, 1-Glitch6.rate.value);

            Glitch6Material.SetFloat(jitterVAmount, Glitch6.amount.value);
            Glitch6Material.SetFloat(jitterVSpeed, Glitch6.speed.value);

            if (Glitch6.mask.value != null)
            {
                Glitch6Material.SetTexture(_Mask, Glitch6.mask.value);
                Glitch6Material.SetFloat(_FadeMultiplier, 1);
            }
            else
            {
                Glitch6Material.SetFloat(_FadeMultiplier, 0);
            }

            cmd.SetGlobalTexture(MainTexId, source);

            cmd.GetTemporaryRT(destination, Screen.width, Screen.height, 0, FilterMode.Point, RenderTextureFormat.Default);

            cmd.Blit(source, destination);
            cmd.Blit(destination, source, Glitch6Material, 0);
        }
        
    }

}


