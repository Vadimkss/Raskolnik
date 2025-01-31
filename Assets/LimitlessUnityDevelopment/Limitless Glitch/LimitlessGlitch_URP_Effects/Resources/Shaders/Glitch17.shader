Shader "LimitlessGlitch/Glitch17"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
	HLSLINCLUDE

	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

	uniform TEXTURE2D(_MainTex);
	uniform SAMPLER(sampler_MainTex);
	uniform TEXTURE2D(_Mask);
	uniform SAMPLER(sampler_Mask);
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
	uniform float Size1;
	uniform float Speed;
	uniform float Strength;
	uniform float Fade;
	uniform float rgb_split;
	uniform float randAmount;
	uniform float someFactor;

	uniform float stop;
	float rand(float2 n)
	{
		return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
	}

	float mod(float x, float y) {
		return   x - y * floor(x / y);
	}
	float hash(float n)
	{
		n = mod(n, 64.0);
		return frac(sin(n) * 43758.5453);
	}

	float noise(float2 p)
	{
		return hash(p.x + p.y * 57.0);
	}

	float2 cell(float2 cell)
	{
		return float2(noise(cell) + cos(cell.y) * 0.3, noise(cell * 0.3) + sin(cell.x) * 0.3);
	}

	float3 voronoiNoize2(float2 t, float pw)
	{
		float2 p = floor(t);
		float3 nn = float3(1e10, 1e10, 1e10);
		float wsum = 0.0;
		float3 cl = float3(0.0, 0.0, 0.0);

		for (int y = -1; y < 2; y += 1)
			for (int x = -1; x < 2; x += 1)
			{
				float2 q = float2(float(x), float(y)) + p;
				float2 q2 = q - floor(q / 8.0) * 8.0;
				float2 c = q + cell(q2);
				float2 r = c - t;
				float cos1 = 0.5 + 0.5 * cos((q2.x + q2.y * 119.0) * 8.0);
				float d = dot(r, r);
				float w = pow(smoothstep(0.0, 1.0, 1.0 - abs(r.x)), pw) * pow(smoothstep(0.0, 1.0, 1.0 - abs(r.y)), pw);
				cl += float3(cos1, cos1, cos1) * w;
				wsum += w;
				nn = lerp(float3(q2, d), nn, step(nn.z, d));
			}
		return pow(cl / wsum, float3(0.5, 0.5, 0.5)) * 2;
		}

	float3 voronoiNoize(float2 t)
	{
		float3 v1 = voronoiNoize2(t * 0.25, 16.0);
		return v1 * (0.0 + 1.0 * voronoiNoize2(t * 0.5 + v1.xy, 2.0))
			+ voronoiNoize2(t * 0.5, 4.0) * 0.5;
	}

	float4 Frag(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float time = _Time.x * Speed;
		float2 uv = input.uv;
		if (stop == 1)return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

		if (_FadeMultiplier > 0)
		{
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r);
			Fade *= alpha_Mask;
		}

		float2 t = uv + (float2(sin(time), cos(time)) - time * 2.0) * 0.4;
		float size = 32.2;
		float2 tt = frac((t) * 0.5) * size;
		float4 col = float4(0.0,0.0,0.0,0.0);
			float2 off = voronoiNoize(tt * Size1 + 90).rg;

			col.r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, clamp(uv + (1 - off  + 0 * rgb_split) * Strength, 0.0, 1.0)).r;//ge
			col.g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, clamp(uv + (1 - off  + 1 * rgb_split) * Strength, 0.0, 1.0)).g;//de
			col.b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, clamp(uv + (1 - off  + 2 * rgb_split) * Strength, 0.0, 1.0)).b;//fe
			col.a = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, clamp(uv + (1 - off  + 3 * rgb_split) * Strength, 0.0, 1.0)).a;//ee

		float timeS = floor(_Time.x * 228);
		float temp_rand = clamp(rand(float2(timeS, timeS)), 0.001, 1);
		float4 col1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
		if (temp_rand > randAmount) {
			return lerp(col1, col, Fade);
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