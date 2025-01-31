Shader "Custom/HeartPulseShaderWithComicEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PulseColor ("Pulse Color", Color) = (1,0,0,1)  // Красный цвет для пульсации
        _BlendFactor ("Blend Factor", Range(0, 1)) = 0.0  // Коэффициент смешивания
        _MaxBlendFactor ("Max Blend Factor", Range(0, 1)) = 1.0  // Максимальный бленд фактор
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)  // Цвет контура
        _OutlineThickness ("Outline Thickness", Range(0, 0.1)) = 0.02  // Толщина контура
        _PosterizeLevels ("Posterize Levels", Range(1, 10)) = 3  // Уровни постеризации (для эффекта комиксов)
    }
    SubShader
    {
        Tags { "RenderType"="UI" }
        Blend SrcAlpha OneMinusSrcAlpha
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
            float4 _PulseColor;
            float _BlendFactor;
            float _MaxBlendFactor;
            float4 _OutlineColor;
            float _OutlineThickness;
            int _PosterizeLevels;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Получаем цвет из текстуры
                float4 baseColor = tex2D(_MainTex, i.uv);

                // Расчет расстояния от центра (0.5, 0.5)
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);

                // Интерполяция цвета от стандартного к цвету пульсации в зависимости от расстояния
                float blendAmount = smoothstep(0.0, 1.0, 1.0 - dist);
                float4 lerpedColor = lerp(baseColor, _PulseColor, _BlendFactor * _MaxBlendFactor * blendAmount);

                // Постеризация цвета для эффекта комиксов
                lerpedColor.rgb = floor(lerpedColor.rgb * _PosterizeLevels) / _PosterizeLevels;

                // Вычисление нормали для контуров
                float2 dxy = fwidth(i.uv);
                float2 edgeDetection = step(dxy, i.uv) * step(dxy, 1.0 - i.uv);
                float edgeFactor = 1.0 - smoothstep(0.0, _OutlineThickness, min(edgeDetection.x, edgeDetection.y));

                // Добавление контура
                float4 outline = _OutlineColor * edgeFactor;

                // Возвращаем итоговый цвет с учетом контура
                return float4(lerpedColor.rgb * (1.0 - outline.a) + outline.rgb * outline.a, baseColor.a);
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}
