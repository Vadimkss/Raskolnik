Shader "Custom/StylizedTracer"
{
    Properties
    {
        _BaseMap("Base Texture", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _EdgeColor("Edge Color", Color) = (0, 0, 0, 1)
        _EdgeWidth("Edge Width", Range(0, 1)) = 0.1
        _NoiseScale("Noise Scale", Range(0.1, 10)) = 1
        _Speed("Scroll Speed", Range(0, 10)) = 1
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _BaseMap;
            float4 _BaseMap_ST;
            float4 _BaseColor;
            float4 _EdgeColor;
            float _EdgeWidth;
            float _NoiseScale;
            float _Speed;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }

            float noise(float2 uv)
            {
                return frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;
                uv.x += _Time.y * _Speed;

                float baseTexture = tex2D(_BaseMap, uv).r;

                // Noise overlay for hand-drawn effect
                float n = noise(uv * _NoiseScale);
                float edge = smoothstep(1.0 - _EdgeWidth, 1.0, baseTexture + n);

                half4 baseColor = lerp(_EdgeColor, _BaseColor, edge);
                baseColor.a *= baseTexture;

                return baseColor;
            }
            ENDHLSL
        }
    }
    FallBack "Unlit/Texture"
}