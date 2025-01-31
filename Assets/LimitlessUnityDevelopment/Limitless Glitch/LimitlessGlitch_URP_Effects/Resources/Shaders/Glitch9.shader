Shader "LimitlessGlitch/Glitch9"
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
	#define q(x,p) (floor((x)/(p))*(p))
	float _FadeMultiplier;
	float fade;
	float2 size;
	float randAmount;
	float amount;
	float light;
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

	float rand(float2 st)
	{
		return frac(sin(dot(st.xy,
			float2(12.9898, 78.233))) *
			43758.5453123);
	}

	float fractal_noise(float2 uv_in, float time) {
		float2 uv = uv_in;
		float split_size_x = 20.0*size.x;
		float split_size_y = 100.0*size.y;
		float x = floor(uv.x * split_size_x);
		float y = floor(uv.y * split_size_y);
		return rand(float2(x, y) + float2(cos(time), cos(time)));
	}

	float4 Frag(Varyings i) : SV_Target
	{
		float2 uv = i.uv;
		float4 col;
		float time = floor(_Time.x * 228);
		float temp_rand = clamp(rand(float2(time, time)), 0.001, 1);
		float4 image = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
		float4 image1 = image;
		float4 image2 = image;
		float4 image3 = image;

		if (_FadeMultiplier > 0)
		{
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r);
			fade *= alpha_Mask;
		}
		if (temp_rand > randAmount) {
			float noise = fractal_noise(i.uv, time)*amount;

			image1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0.1 * noise, 0)) - float4(0.1,0.1,0.1,0.1);
			image2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) - float4(0.1,0.1,0.1,0.1);
			image3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - float2(0.1 * noise, 0)) - float4(0.1,0.1,0.1,0.1);
		}
		if (light == 1)
			col = float4((image1.x + image.x) / 2, (image2.y + image.y) / 2, (image3.z + image.z) / 2,  (image1.a + image2.a + image3.a) / 3);
		else
			col = float4(image1.x, image2.y, image3.z,(image1.a + image2.a + image3.a) / 3);

		return lerp(image, col, fade);
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