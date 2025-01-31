Shader "LimitlessGlitch/Glitch4" 
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
		TEXTURE2D(_Mask);
		SAMPLER(sampler_Mask);		
		uniform float _FadeMultiplier;
		uniform float _GlitchInterval;
		uniform float _GlitchRate;
        uniform float _RGBSplit;
        uniform float _Speed;
        uniform float _Amount;
		float2 _Res;
		#define NOISE_SIMPLEX_1_DIV_289 0.00346020761245674740484429065744f

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

		float2 mod289(float2 x) {
			return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
		}
		float3 mod289(float3 x) {
			return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
		}
		float3 permute(float3 x) {
			return mod289(
				x * x * 34.0 + x
			);
		}
		float snoise(float2 v)
		{
			const float4 C = float4(
				0.211324865405187, // (3.0-sqrt(3.0))/6.0
				0.366025403784439, // 0.5*(sqrt(3.0)-1.0)
				-0.577350269189626, // -1.0 + 2.0 * C.x
				0.024390243902439  // 1.0 / 41.0
				);

			// First corner
			float2 i = floor(v + dot(v, C.yy));
			float2 x0 = v - i + dot(i, C.xx);

			// Other corners
			// float2 i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
			// Lex-DRL: afaik, step() in GPU is faster than if(), so:
			// step(x, y) = x <= y

			// Actually, a simple conditional without branching is faster than that madness :)
			int2 i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;

			// Permutations
			i = mod289(i); // Avoid truncation effects in permutation
			float3 p = permute(
				permute(
					i.y + float3(0.0, i1.y, 1.0)
				) + i.x + float3(0.0, i1.x, 1.0)
			);

			float3 m = max(
				0.5 - float3(
					dot(x0, x0),
					dot(x12.xy, x12.xy),
					dot(x12.zw, x12.zw)
					),
				0.0
			);
			m = m * m;
			m = m * m;

			// Gradients: 41 points uniformly over a line, mapped onto a diamond.
			// The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)

			float3 x = 2.0 * frac(p * C.www) - 1.0;
			float3 h = abs(x) - 0.5;
			float3 ox = floor(x + 0.5);
			float3 a0 = x - ox;

			// Normalise gradients implicitly by scaling m
			// Approximation of: m *= inversesqrt( a0*a0 + h*h );
			m *= 1.79284291400159 - 0.85373472095314 * (a0 * a0 + h * h);

			// Compute final noise value at P
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot(m, g);
		}
		float random(float2 c) 
        {
			return frac(sin(dot(c.xy, float2(12.9898, 78.233))) * 43758.5453);
		}
		float mod(float x, float y)
		{
			return x - y * floor(x / y);
		}


        float4 Frag(Varyings i) : SV_Target
        {
			half strength = 0.;
				float2 shake = float2(0., 0.);
				strength = smoothstep(_GlitchInterval * _GlitchRate, _GlitchInterval, _GlitchInterval - mod(_Time.y, _GlitchInterval));
				shake = float2(strength , strength )* float2(random(float2(_Time.xy)) * 2.0 - 1.0, random(float2(_Time.y * 2.0, _Time.y * 2.0)) * 2.0 - 1.0) / float2(_Res.x, _Res.y);
				if (_FadeMultiplier > 0)
				{
					float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.uv).r);
					strength *= alpha_Mask;
					shake *= alpha_Mask;
				}

				float y = i.uv.y * _Res.y;
				float rgbWave = 0.;
				
					rgbWave = (
						snoise(float2( y * 0.01, _Time.y * _Speed*20)) * ( strength * _Amount*32.0) 
						* snoise(float2( y * 0.02, _Time.y * _Speed*10)) * (strength * _Amount*4.0) 

						) / _Res.x;
				
				float rgbDiff = (_RGBSplit*50 + (20.0 * strength + 1.0)) / _Res.x;
				rgbDiff = rgbDiff*rgbWave ;
				float rgbUvX = i.uv.x + rgbWave;
				
				float4 g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(rgbUvX, i.uv.y) + shake);
				float4 rb = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(rgbUvX +rgbDiff, i.uv.y) + shake);

				float4 ret = float4(rb.x, g.y, rb.z, rb.a+g.a );

				return ret;
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