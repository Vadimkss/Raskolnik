Shader "Custom/CyberHeartPulseShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PulseColor ("Pulse Color", Color) = (1,0,0,1)  // ÷вет дл€ пульсации
        _GlowColor ("Glow Color", Color) = (0,1,1,1)  // ÷вет свечени€ (неоновый цвет)
        _BlendFactor ("Blend Factor", Range(0, 1)) = 0.0  //  оэффициент смешивани€
        _RippleStrength ("Ripple Strength", Range(0, 1)) = 0.1  // —ила эффекта р€би
        _TimeScale ("Time Scale", Range(0, 5)) = 1.0  // —корость пульсации
        _GlowIntensity ("Glow Intensity", Range(0, 1)) = 0.5  // »нтенсивность свечени€
        _ScanSpeed ("Scan Speed", Range(0, 5)) = 2.0  // —корость сканера
        _GlitchStrength ("Glitch Strength", Range(0, 0.5)) = 0.1  // —ила глича
    }
    SubShader
    {
        Tags { "RenderType"="UI" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _PulseColor;
            float4 _GlowColor;
            float _BlendFactor;
            float _RippleStrength;
            float _TimeScale;
            float _GlowIntensity;
            float _ScanSpeed;
            float _GlitchStrength;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);

                // ƒобавл€ем эффект р€би
                float ripple = sin((dist - _TimeScale * _Time.y) * 10.0) * _RippleStrength;

                // Ёффект сканера (лазера)
                float scanLine = sin(i.uv.y * 10.0 + _ScanSpeed * _Time.y) * 0.5 + 0.5;

                // √лич-эффект (небольшие случайные искажени€)
                float glitch = frac(sin(dot(i.uv * _TimeScale, float2(12.9898, 78.233))) * 43758.5453);
                glitch = step(_GlitchStrength, glitch);

                // »нтерпол€ци€ цвета с учетом р€би и глича
                float blendAmount = smoothstep(0.0, 1.0, 1.0 - dist + ripple) * glitch;
                float4 baseColor = tex2D(_MainTex, i.uv);
                float4 lerpedColor = lerp(baseColor, _PulseColor, _BlendFactor * blendAmount);

                // ƒобавл€ем свечение к кра€м
                float glow = smoothstep(0.4, 0.6, dist) * _GlowIntensity;
                float4 glowColor = lerp(lerpedColor, _GlowColor, glow);

                // Ёффект пульсации интенсивности
                float intensity = 0.75 + 0.25 * sin(_TimeScale * _Time.y * 5.0);
                glowColor.rgb *= intensity * scanLine;

                return float4(glowColor.rgb, baseColor.a);
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}
