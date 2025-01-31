Shader "Custom/FuturisticUI"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (.5, .5, .5, 1)
        _Speed ("Speed", Float) = 1.0
        _Intensity ("Intensity", Float) = 0.1
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _Speed;
            float _Intensity;
            
            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _Speed;
                float offset = sin(time) * _Intensity;
                float2 uv = i.uv + float2(offset, offset);
                half4 col = tex2D(_MainTex, uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}