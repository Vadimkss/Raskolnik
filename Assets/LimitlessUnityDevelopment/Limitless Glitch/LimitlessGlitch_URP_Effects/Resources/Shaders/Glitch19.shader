Shader "LimitlessGlitch/Glitch19" 
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
    HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        sampler2D _NoiseTex;

        struct Attributes
        {
            float4 positionOS       : POSITION;
            float2 uv               : TEXCOORD0;
        };

        struct Varyings
        {
            float2 uv        : TEXCOORD0;
            float4 vertex : SV_POSITION;
            UNITY_VERTEX_OUTPUT_STEREO
        };
        Varyings vert(Attributes input)
        {
            Varyings output = (Varyings)0;
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
            output.vertex = vertexInput.positionCS;
            output.uv = input.uv;

            return output;
        }
        float AMPLITUDE = 0.8;
        float SPEED = 0.7;
        float ShiftSeed = 8;//8-16
        int stop;
        float fade;
        float randAmount;

        float4 rgbShift(in float2 p, in float4 shift) {
            shift *= 2.0 * shift.w - 1.0;
            float2 rs = float2(shift.x, -shift.y);
            float2 gs = float2(shift.y, -shift.z);
            float2 bs = float2(shift.z, -shift.x);

            float4 r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, p + rs);
            float4 g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, p + gs);
            float4 b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, p + bs);

            return float4(r.x, g.y, b.z, (r.a+g.a+b.a)/3);
        }

        float4 noise(in float2 p) {
            return tex2D(_NoiseTex, p);
        }
        float rand(float n)
        {
            return frac(sin(n) * 43758.5453123);
        }
        float rand(float2 n)
        {
            return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
        }
        float4 float4pow(in float4 v, in float p) {
            return float4(pow(abs(v.x), p), pow(abs(v.y), p), pow(abs(v.z), p), v.w);
        }

        float4 Frag(Varyings input) : SV_Target
        {
            float2 uv = input.uv;
            if (stop == 1)return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            float4 shift = float4pow(noise(float2(SPEED * _Time.x,2.0 * SPEED * _Time.x / 25.0)),ShiftSeed) * float4(AMPLITUDE,AMPLITUDE,AMPLITUDE,1.0);

            float time = floor(_Time.x * 228);
            float temp_rand = clamp(rand(float2(time, time)), 0.001, 1);

            float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            if (temp_rand > randAmount) {
                return lerp(col, rgbShift(uv, shift), fade);
            }
            else {
                return col;
            }

        }

    ENDHLSL

    SubShader
    {
            Tags{ "RenderType" = "Opaque" }
                LOD 200
        Pass
        {
            HLSLPROGRAM

                #pragma vertex vert
                #pragma fragment Frag

            ENDHLSL
        }
    }
        FallBack "Diffuse"
}