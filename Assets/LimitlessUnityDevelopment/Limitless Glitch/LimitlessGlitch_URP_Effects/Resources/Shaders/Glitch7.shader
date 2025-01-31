Shader "LimitlessGlitch/Glitch7" 
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
		uniform float _TimeX;
		uniform float Offset;
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


		uniform float Fade;
		float random(float2 seed)
		{
			return frac(sin(dot(seed * floor(_TimeX * 30.0), float2(127.1,311.7))) * 43758.5453123);
		}

		float random(float seed)
		{
			return random(float2(seed, 1.0));
		}

        float4 Frag(Varyings i) : SV_Target
        {
			float2 uv = i.uv.xy;

			float2 blockS = floor(uv * float2(24., 9.));
			float2 blockL = floor(uv * float2(8., 4.));
			float lineNoise = pow(random(blockS), 8.0) *Offset* pow(random(blockL), 3.0) - pow(random(7.2341), 17.0) * 2.;
			if (_FadeMultiplier > 0)
			{
				float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r);
				lineNoise *= alpha_Mask;
			}

			float4 col1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
			float4 col2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(lineNoise * 0.05 * random(5.0), 0));
			float4 col3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - float2(lineNoise * 0.05 * random(31.0), 0));

			float4 result = float4(float3(col1.x, col2.y, col3.z), col1.a+col2.a+col3.a);
			result = lerp(col1,result, Fade);

			return result;
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