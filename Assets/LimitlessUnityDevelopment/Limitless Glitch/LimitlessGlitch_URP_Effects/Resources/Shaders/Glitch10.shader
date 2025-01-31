Shader "LimitlessGlitch/Glitch10"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		HLSLINCLUDE

	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	TEXTURE2D(_MainTex);
	SAMPLER(sampler_MainTex);
	TEXTURE2D(_Mask);
	SAMPLER(sampler_Mask);
	float width;
	float fade;
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

	float3 hash3(uint n)
	{
		n = (n << 13U) ^ n;
		n = n * (n * n * 15731U + 789221U) + 1376312589U;
		uint3 k = n * uint3(n, n * 16807U, n * 48271U);
		return float3(k & uint3(0x7fffffffU, 0x7fffffffU, 0x7fffffffU)) / float(0x7fffffff);
	}
	float rand(float2 st)
	{
		return frac(sin(dot(st.xy,
			float2(12.9898, 78.233))) *
			43758.5453123);
	}


	float4 Frag(Varyings i) : SV_Target
	{
		float2 uv = i.uv;
		uint2 p = uint2(uv);
		float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
		float4 col2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + 0.002);
		float4 diff = abs(col - col2);
		float4 fragColor;
		if (_FadeMultiplier > 0)
		{
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r);
			width /= alpha_Mask;
		}
		if (diff.r > width) {
			fragColor = float4(rand(0.5 + uv * _Time.x), rand(-0.2 + uv * _Time.x), rand(0.8 + uv * _Time.x),1);
		}
		else {
			fragColor = col;
		}
		return lerp(col,fragColor,fade);
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