Shader "LimitlessGlitch/Glitch16"
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
	float randAmount;
	float fade;
	int stop;
	float maxiters;
	float speed;
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

	float4 Frag(Varyings i) : SV_Target
	{
		 float2 uv = i.uv;
		 if (stop == 1)return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
		 if (_FadeMultiplier > 0)
		 {
			 float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r);
			 fade *= alpha_Mask;
		 }
		 float scaledT = _Time.x * 5.0;
		 float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
		 int iters = int(scaledT - float(maxiters) * floor(scaledT / float(maxiters)));
		 for (int i = 0; i < maxiters; i++)
		 {
		     color *= 0.8 + (clamp((float(i < iters)) + (1.0 / float(iters)) * float4(float2(_ScreenParams.x * ddx(color.rg)), float2(_ScreenParams.y * ddy(color.ba)))
		         , 0.0, 1.0) * 0.25);
		 }
		 float timeS = floor(_Time.x * 228* speed);
		 float temp_rand = clamp(rand(float2(timeS, timeS)),0.001,1);
		 float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
		 if (temp_rand > randAmount) {
			 return lerp(col, color, fade);
		 }
		 else {
			 return col;

		 }

		 return color;

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