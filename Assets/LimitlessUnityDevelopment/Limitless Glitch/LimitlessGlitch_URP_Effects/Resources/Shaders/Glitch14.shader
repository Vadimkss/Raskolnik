Shader "LimitlessGlitch/Glitch14"
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

	float amount;
	float speed;
	float randAmount;
	int stop;
	float Rspeed;
	float fade;
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

	float rand(float2 n)
	{
		return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
	}

	#define SIN01(a) (sin(a)*0.5 + 0.5)

	float3 rgb2hsv(float3 c)
	{
		float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
		float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
		float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

		float d = q.x - min(q.w, q.y);
		float e = 1.0e-10;
		return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
	}

	float4 Frag(Varyings i) : SV_Target
	{
		float2 uv = i.uv;
		if (stop == 1)return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
		if (_FadeMultiplier > 0)
		{
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r);
			amount *= alpha_Mask;
		}
		float3 hsv = rgb2hsv(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb);
		float angle = hsv.x + atan2(uv.y, uv.x) + _Time.x * speed;
		float2x2 RotationMatrix = float2x2(cos(angle), -sin(angle), sin(angle), cos(angle));
		float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + mul (RotationMatrix , float2(log(max(SIN01(_Time.x * 0.7) - 0.2, 0.) * amount + 1.), 0)) * hsv.y);

		float timeS = floor(_Time.x * 228 * Rspeed);
		float temp_rand = clamp(rand(float2(timeS, timeS)), 0.001, 1);
		float4 col1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
		if (temp_rand > randAmount) {
			return lerp(col1, col, fade);
		}
		else {
			return col1;
		}

	}


	ENDHLSL

		SubShader
	{
		Cull Off ZWrite On ZTest Always Lighting On

		Pass
		{
			HLSLPROGRAM

				#pragma vertex Vert
				#pragma fragment Frag

			ENDHLSL
		}
	}
}