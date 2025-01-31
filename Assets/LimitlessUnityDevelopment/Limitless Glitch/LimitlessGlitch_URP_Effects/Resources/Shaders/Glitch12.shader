Shader "LimitlessGlitch/Glitch12"
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
	float amount;
	float linesAmount;
	float speed;
	float fade;
	float randAmount;
	float _FadeMultiplier;

	#define q(x,p) (floor((x)/(p))*(p))
	#define TILE_SIZE 16.0

	float wow;
	float Amount = 1.0;
	int stop;
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
		return (a < 0) ? -c : c;
	}
	float4 rgb2hsv(float4 c)
	{
		float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
		float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
		float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

		float d = q.x - min(q.w, q.y);
		float e = 1.0e-10;
		return float4(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x, 1);
	}

	float4 hsv2rgb(float4 c)
	{
		float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
		float4 p = abs(frac(c + K) * 6.0 - K);
		return c.z * lerp(K, clamp(p - K, 0.0, 1.0), c.y);
	}

	float4 posterize(float4 color, float steps)
	{
		return floor(color * steps) / steps;
	}

	float quantize(float n, float steps)
	{
		return floor(n * steps) / steps;
	}

	float4 downsample(float2 uv, float pixelSize)
	{
		return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - mod(uv, float2(pixelSize, pixelSize)));
	}

	float rand(float n)
	{
		return frac(sin(n) * 43758.5453123);
	}
	float rand(float2 n)
	{
		return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
	}
	float noise(float p)
	{
		float fl = floor(p);
		float fc = frac(p);
		return lerp(rand(fl), rand(fl + 1.0), fc);
	}
	float noise(float2 p)
	{
		float2 ip = floor(p);
		float2 u = frac(p);
		u = u * u * (3.0 - 2.0 * u);
		float res = lerp(
			lerp(rand(ip), rand(ip + float2(1.0, 0.0)), u.x),
			lerp(rand(ip + float2(0.0, 1.0)), rand(ip + float2(1.0, 1.0)), u.x), u.y);
		return res * res;
	}

	float4 edge(float2 uv, float sampleSize)
	{
		float dx = sampleSize / _ScreenParams.x;
		float dy = sampleSize / _ScreenParams.y;
		return (
			lerp(downsample( uv - float2(dx, 0.0), sampleSize), downsample(uv + float2(dx, 0.0), sampleSize), mod(uv.x, dx) / dx) +
			lerp(downsample( uv - float2(0.0, dy), sampleSize), downsample( uv + float2(0.0, dy), sampleSize), mod(uv.y, dy) / dy)
			) / 2.0 - SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
	}

	float4 distort(float2 uv, float edgeSize)
	{
		float2 pixel = float2(1.0, 1.0) / _ScreenParams.xy;
		float4 field = rgb2hsv(edge(uv, edgeSize));
		float2 distort = pixel * sin((field.rb) * PI * 2.0);
		float shiftx = noise(float2(quantize(uv.y + 31.5, _ScreenParams.y / speed) * _Time.y, frac(_Time.y) * 300.0));
		float shifty = noise(float2(quantize(uv.x + 11.5, _ScreenParams.x / speed) * _Time.y, frac(_Time.y) * 100.0));
		float4 rgb = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + (distort + (pixel - pixel / 2.0) * float2(shiftx, shifty) * (50.0 + 100.0 * Amount)) * Amount);
		float4 hsv = rgb2hsv(rgb);
		hsv.y = mod(hsv.y + shifty * pow(Amount, 5.0) * 0.25, 1.0);
		return posterize(hsv2rgb(hsv), floor(lerp(256.0, pow(1.0 - hsv.z - 0.5, 2.0) * 64.0 * shiftx + 4.0, 1.0 - pow(1.0 - Amount, 5.0))));
	}

	float4 Frag(Varyings i) : SV_Target
	{
		float2 uv = i.uv;
		if (stop == 1)return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
		wow = clamp(mod(noise(_Time.y + uv.y), 1.0), 0.0, 1.0) * 2.0 - 1.0;
		Amount = wow*amount;
		if (_FadeMultiplier > 0)
		{
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r);
			Amount *= alpha_Mask;
			fade *= alpha_Mask;
		}
		float time = floor(_Time.x * 228);
		float temp_rand = rand(float2(time, time));
		float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
		if (temp_rand > randAmount) {
			return lerp(col,distort( uv, 8.0),fade);
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