Shader "Custom/HolographicLineSphere"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 0.5)
        _LineWidth ("Line Width", Range(0.01, 1)) = 0.05
        _Transparency ("Transparency", Range(0, 1)) = 0.5
        _LineColor ("Line Color", Color) = (1, 1, 1, 1)
        _LineTexture ("Line Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" }
        Pass
        {
            Name "HolographicLines"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Less

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            float _LineWidth;
            float _Transparency;
            fixed4 _LineColor;
            sampler2D _LineTexture;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(v.normal);

                // Generate UV coordinates based on spherical coordinates
                float3 pos = v.vertex.xyz;
                float2 uv = 0.5 + atan2(pos.z, pos.x) / (2 * 3.14159) * float2(1, -1);
                o.uv = uv;

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Sample the line texture to create lines on the sphere
                half4 lineColor = tex2D(_LineTexture, i.uv) * _LineColor;
                lineColor.a *= _Transparency;

                // Combine the line color with the base color
                half4 finalColor = lerp(_Color, lineColor, lineColor.a);
                return finalColor;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
