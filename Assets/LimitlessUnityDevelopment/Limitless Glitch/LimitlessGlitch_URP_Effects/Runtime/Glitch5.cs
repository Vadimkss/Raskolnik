using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Limitless.Enums;

public class Glitch5 : ScriptableRendererFeature
{
    Glitch5Pass GlitchPass;
    public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void Create()
    {
        GlitchPass = new Glitch5Pass(Event);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2019 || UNITY_2020
        GlitchPass.Setup(renderer.cameraColorTarget);
#else

#endif
        renderer.EnqueuePass(GlitchPass);
    }
    public class Glitch5Pass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Render Glitch5 Effect";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int screenLinesNum = Shader.PropertyToID("screenLinesNum");
        static readonly int time_ = Shader.PropertyToID("time_");
        static readonly int jitterHAmount = Shader.PropertyToID("jitterHAmount");
        static readonly int speed = Shader.PropertyToID("speed");
        static readonly int jitterHFreq = Shader.PropertyToID("jitterHFreq");
        static readonly int jitterHRate = Shader.PropertyToID("jitterHRate");
        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
        static readonly int _Mask = Shader.PropertyToID("_Mask");

        static readonly int TempTargetId = Shader.PropertyToID("Glitch5");
        LimitlessGlitch5 Glitch5;
        Material Glitch5Material;
        RenderTargetIdentifier currentTarget;

        private float _time;
        private float tempVFR;
        public float t;

        public Glitch5Pass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("LimitlessGlitch/Glitch5");
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            Glitch5Material = CoreUtils.CreateEngineMaterial(shader);

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
            if (Glitch5Material == null)
            {
                Debug.LogError("Material not created.");
                return;
            }
            var stack = VolumeManager.instance.stack;
            Glitch5 = stack.GetComponent<LimitlessGlitch5>();
            if (!renderingData.cameraData.postProcessEnabled && Glitch5.GlobalPostProcessingSettings.value) return;

            if (Glitch5 == null) { return; }
            if (!Glitch5.IsActive()) { return; }

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

            if (Glitch5.interval.value == IntervalMode.Random)
            {
                t -= Time.deltaTime;
                if (t <= 0)
                {
                    tempVFR = UnityEngine.Random.Range(Glitch5.minMax.value.x, Glitch5.minMax.value.y);
                    t = tempVFR;
                }
            }
            else
            {
                tempVFR = Glitch5.frequency.value;
            }

            if (Glitch5.unscaledTime.value) { _time = Time.unscaledTime; }
            else _time = Time.time;

            Glitch5Material.SetFloat(screenLinesNum, Glitch5.stretchResolution.value);
            Glitch5Material.SetFloat(time_, _time);
            if (Glitch5.interval.value == IntervalMode.Infinite) Glitch5Material.EnableKeyword("CUSTOM_INTERVAL"); else Glitch5Material.DisableKeyword("CUSTOM_INTERVAL");
            if (Glitch5.shiftMode.value == AxisMode.Horizontal) Glitch5Material.EnableKeyword("SHIFT_H"); else Glitch5Material.DisableKeyword("SHIFT_H");

            Glitch5Material.SetFloat(jitterHAmount, Glitch5.amount.value);
            Glitch5Material.SetFloat(speed, Glitch5.speed.value);
            Glitch5Material.SetFloat(jitterHFreq, tempVFR);
            Glitch5Material.SetFloat(jitterHRate, 1-Glitch5.rate.value);

            if (Glitch5.mask.value != null)
            {
                Glitch5Material.SetTexture(_Mask, Glitch5.mask.value);
                Glitch5Material.SetFloat(_FadeMultiplier, 1);
            }
            else
            {
                Glitch5Material.SetFloat(_FadeMultiplier, 0);
            }

            cmd.SetGlobalTexture(MainTexId, source);

            cmd.GetTemporaryRT(destination, Screen.width, Screen.height, 0, FilterMode.Point, RenderTextureFormat.Default);


            cmd.Blit(source, destination);
            cmd.Blit(destination, source, Glitch5Material, Glitch5.offsetAxis.value == AxisMode.Horizontal ? 0 : 1);
        }
        
    }

}


