Shader "LimitlessGlitch/Glitch21" 
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
        float range = 0.05;
        float noiseQuality = 250.0;
        float noiseIntensity = 0.0088;
        float offsetIntensity = 0.02;
        float colorOffsetIntensity = 1.3;
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

        float verticalBar(float pos, float uvY, float offset)
        {
            float edge0 = (pos - range);
            float edge1 = (pos + range);

            float x = smoothstep(edge0, pos, uvY) * offset;
            x -= smoothstep(pos, edge1, uvY) * offset;
            return x;
        }
        float rand(float2 co)
        {
            return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
        }
        float4 Frag(Varyings i) : SV_Target
        {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 uv = i.uv;

        for (float i = 0.0; i < 0.71; i += 0.1313)
        {
            float d = fmod(_Time.y * i, 1.7);
            float o = sin(1.0 - tan(_Time.y * 0.24 * i));
            o *= offsetIntensity;
            uv.x += verticalBar(d, uv.y, o);
        }

        float uvY = uv.y;
        uvY *= noiseQuality;
        uvY = float(int(uvY)) * (1.0 / noiseQuality);
        float noise = rand(float2(_Time.y * 0.00001, uvY));
        uv.x += noise * noiseIntensity;

        float2 offsetR = float2(0.006 * sin(_Time.y), 0.0) * colorOffsetIntensity;
        float2 offsetG = float2(0.0073 * (cos(_Time.y * 0.97)), 0.0) * colorOffsetIntensity;

        float r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offsetR).r;
        float g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offsetG).g;
        float b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).b;

        float4 tex = float4(r, g, b, 1.0);
        return tex;
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