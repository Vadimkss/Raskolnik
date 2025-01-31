Shader "LimitlessGlitch/Glitch20" 
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
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        float _FadeMultiplier;
        float _Intensity;

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
        uniform float Speed;
        uniform float AMPLITUDE;
        uniform float Fade;
        uniform float randAmount;
        uniform int stop;


        float2 uvp(float2 uv) {
            return clamp(uv, 0.0, 1.0);
        }

        float rand(float2 co) {
            return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
        }

        float4 Frag(Varyings input) : SV_Target
        {

            float4 col = float4(0.0,0.0,0.0,0.0);
            float amp;

            amp = AMPLITUDE;

            float2 uv = input.uv;
            if (stop == 1)return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            if (_FadeMultiplier > 0)
            {
                float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r);
                Fade *= alpha_Mask;
            }
            [unroll]for (int i = 0; i < 4; i++) {
                uv += float2(sin(_Time.x + float(i) + amp), cos(_Time.x + float(i) + amp)) * amp * 0.2;
                float4 texOrig = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvp(uv));

                uv.x += (rand(float2(uv.y + float(i), _Time.x)) * 2.0 - 1.0) * amp * 0.8 * (texOrig[i] + 0.2);
                uv.y += (rand(float2(uv.x, _Time.x + float(i))) * 2.0 - 1.0) * amp * 0.1 * (texOrig[i] + 0.2);

                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvp(uv));

                tex += (rand(uv + _Time.x + float(i)) * 2.0 - 1.0) * amp * 0.1;
                tex += (rand(uv + _Time.x + tex[i] + float(i)) * 2.0 - 1.0) * amp * 0.2;
                tex += lerp(1.0, rand(uv + tex[i] + float(i) * 253.6 + _Time.x) * tex.r * 5.0, amp);
                tex += abs(tex[i] - texOrig[i]);

                tex *= rand(uv) * amp + 1.0;

                tex = frac(tex);

                col[i] = tex[i];
            }
            
            float4 col1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
            float time = floor(_Time.x * Speed);
            float temp_rand = clamp(rand(float2(time, time)), 0.001, 1);

            if (temp_rand > randAmount) {
                return lerp(col1 , float4(col.rgb,col1.a/col.a), Fade* _Intensity);
            }
            else {
                return col1;
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