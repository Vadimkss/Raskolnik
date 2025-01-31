Shader "Custom/FuturisticLine"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (0,1,0,1)
        _GradientSpeed ("Gradient Speed", Range(0.1, 5.0)) = 1.0
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 1.0
        _Tiling ("Tiling", Range(1, 10)) = 2.0
        _PulseSpeed ("Pulse Speed", Range(0.1, 5.0)) = 1.0
        _PulseStrength ("Pulse Strength", Range(0.1, 1.0)) = 0.5
        _LineTexture ("Line Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float pulseFactor : TEXCOORD1;
            };

            sampler2D _LineTexture;
            float4 _MainColor;
            float4 _EmissionColor;
            float _GradientSpeed;
            float _GlowIntensity;
            float _Tiling;
            float _PulseSpeed;
            float _PulseStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _Tiling;

                // Пульсация на основе времени для динамики линии
                float time = _Time.y * _PulseSpeed;
                o.pulseFactor = (1.0 + sin(time)) * _PulseStrength;

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Движение градиента вдоль линии
                float time = _Time.y * _GradientSpeed;
                float gradient = abs(sin(i.uv.x + time));

                // Основной цвет с градиентом
                half4 baseColor = tex2D(_LineTexture, i.uv) * _MainColor;
                baseColor.rgb *= gradient;

                // Добавляем свечение (glow)
                half4 glow = _EmissionColor * _GlowIntensity * gradient;

                // Применение пульсации для оживления линии
                baseColor.rgb *= i.pulseFactor;
                glow.rgb *= i.pulseFactor;

                return baseColor + glow;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
