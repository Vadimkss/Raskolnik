Shader "LimitlessGlitch/Glitch11"
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
	SAMPLER(sampler_Mask);	float amount;
	float linesAmount;
	float speed;
	float noiseA;
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

	float mod(float x, float y) {
		return   x - y * floor(x / y);
	}
	float2 mod(float2 a, float2 b)
	{
		float2 c = frac(abs(a / b)) * abs(b);
		return (a < 0) ? -c : c;   /* if ( a < 0 ) c = 0-c */
	}
	float rand(float2 co) {
		return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
	}

	float sample_noise(float2 fragCoord)
	{
		float2 uv = mod(fragCoord + float2(0, 100. * _Time.x), _ScreenParams.xy);
		float value = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).r;
		return pow(abs(value), 7.);
	}


	float4 Frag(Varyings i) : SV_Target
	{

		float2 uv = i.uv;
		if (_FadeMultiplier > 0)
		{
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r);
			amount *= alpha_Mask;
		}
		float2 wobbl;
		if(noiseA>0)
		wobbl = float2(amount * rand(float2(_Time.x, i.uv.y)), 0.);
		else
		wobbl = float2(amount, 0.);


		float t_val = tan(0.25 * _Time.x * 100 * speed + uv.y * PI * linesAmount);
		float2 tan_off = float2(wobbl.x * min(0., t_val), 0.);

		float4 color1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + wobbl + tan_off);
		float4 color = color1;

		float s_val = ((sin(2. * PI * uv.y + _Time.x * 20.) + sin(2. * PI * uv.y)) / 2.)
		* .015 * sin(_Time.x);
		color += s_val;

		float ival = _ScreenParams.y / 10.;
		float r = rand(float2(_Time.x, i.uv.y));
		float on = floor(float(uint(i.uv.y + (_Time.x * r * 100.)) % uint(ival + 1.)) / ival);
		float wh = sample_noise(i.uv) * on;
		color = float4(min(1., color.r + wh),
			min(1., color.g + wh),
			min(1., color.b + wh), min(1., color.a + wh));

		float vig = 1. - sin(PI * uv.x) * sin(PI * uv.y);

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