Shader "LimitlessGlitch/Glitch13"
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

	float random2d(float2 n) {
		return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
	}

	float randomRange(in float2 seed, in float min, in float max) {
		return min + random2d(seed) * (max - min);
	}

	float insideRange(float v, float bottom, float top) {
		return step(bottom, v) - step(top, v);
	}
	float rand(float2 n)
	{
		return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
	}
	float val1 = 0.2; 
	float val2 = 0.2; 
	float val3 = 0.2;
	float SPEED = 0.6; 
	float randAmount;
	float fade;
	int stop;

	float4 Frag(Varyings i) : SV_Target
	{
		float time = floor(_Time.x * SPEED * 60.0);
		float2 uv = i.uv;
		if (stop==1)return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

		float4 outCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
		if (_FadeMultiplier > 0)
		{
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r);
			fade *= alpha_Mask;
		}
		float maxOffset = val1 / 2.0;
		for (float i = 0.0; i < 10.0 * val2; i += 1.0) {
			float sliceY = random2d(float2(time, 2345.0 + float(i)));
			float sliceH = random2d(float2(time, 9035.0 + float(i))) * 0.25;
			float hOffset = randomRange(float2(time, 9625.0 + float(i)), -maxOffset, maxOffset);
			float2 uvOff = uv;
			uvOff.x += hOffset;
			if (insideRange(uv.y, sliceY, frac(sliceY + sliceH)) == 1.0) {
				outCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvOff);
			}
		}

		float maxColOffset = val3 / 6.0;
		float rnd = random2d(float2(time, 9545.0));
		float2 colOffset = float2(randomRange(float2(time, 9545.0), -maxColOffset, maxColOffset),
		randomRange(float2(time, 7205.0), -maxColOffset, maxColOffset));
		if (rnd < 0.33) {
			outCol.r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + colOffset).r;
		}
		else if (rnd < 0.66) {
			outCol.g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + colOffset).g;
		}
		else {
			outCol.b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + colOffset).b;
		}
		float timeS = floor(_Time.x * 228);
		float temp_rand = clamp(rand(float2(timeS, timeS)), 0.001, 1);
		float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
		if (temp_rand > randAmount) {
			return lerp(col, outCol, fade);
		}
		else {
			return col;
		}
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