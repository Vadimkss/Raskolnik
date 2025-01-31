Shader "LimitlessGlitch/Glitch18" 
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
        uniform float _FadeMultiplier;

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
        uniform float fade;
        uniform float randAmount;
        uniform int stop;
        uniform float m_offset;

        float mod(float x, float y) {
            return   x - y * floor(x / y);
        }
        float rand(float n)
        {
            return frac(sin(n) * 43758.5453123);
        }
        float rand(float2 n)
        {
            return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
        }
        float4 Frag(Varyings input) : SV_Target
        {
                        float2 uv = input.uv;
                        float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                        if (stop == 1)return color;
                        if (_FadeMultiplier > 0)
                        {
                            float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r);
                            fade *= alpha_Mask;
                        }
                        float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                        float2 offset = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).xz;
                        float2 uvOff = (uv + offset* m_offset);

                        float4 color2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvOff);

                        float time = floor(_Time.x * Speed);
                        float temp_rand = clamp(rand(time), 0.001, 1);

                        if (temp_rand > randAmount) {
                            return lerp(color, col * float4(mod(color.x / color2.x, 2), mod(color.y / color2.y, 2), mod(color.z / color2.z, 2), mod(color.a / color2.a, 2)), fade);
                        }
                        else {
                            return color;
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