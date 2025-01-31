Shader "LimitlessGlitch/Glitch_2" 
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
        HLSLINCLUDE
         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);
    TEXTURE2D(_Mask);
    SAMPLER(sampler_Mask); 
    TEXTURE2D(_NoiseTex);
    SAMPLER(sampler_NoiseTex);
        sampler2D _TrashTex;
        float _Intensity;
        float _ColorIntensity;
        float _FadeMultiplier;
        	        struct Attributes
        {
            float4 positionOS       : POSITION;
            float2 uv               : TEXCOORD0;
        };

        struct Varyings
        {
            float2 uv        : TEXCOORD0;
            float4 positionCS : SV_POSITION;
            UNITY_VERTEX_OUTPUT_STEREO
        };
        Varyings Vert(Attributes input)
        {
            Varyings output = (Varyings)0;
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
            output.positionCS = vertexInput.positionCS;
            output.uv = input.uv;

            return output;
        }

       float4 Frag(Varyings i) : SV_Target
        {
        float4 glitch = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv);
        float thresh = 1.001 - _Intensity * 1.001;
        float w_d = step(thresh*_ColorIntensity/ SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.uv).r, pow(abs(glitch.z), 2.5));
        float w_f = step(thresh, pow(abs(glitch.w), 2.5)); 
        float w_c = step(thresh, pow(abs(glitch.z), 3.5)); 
        float2 uv = frac(i.uv + glitch.xy * w_d);
        float4 source = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
        float3 color = lerp(source, tex2D(_TrashTex, uv), w_f).rgb;
        float3 neg = saturate(color.grb + (1 - dot(color, 1)) * 0.1);
        color = lerp(color, neg, w_c);

        return float4(color, source.a);
       }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex Vert
                #pragma fragment Frag

            ENDHLSL
        }
    }
}