Shader "Custom/ComicShaderWithSteps"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0.001, 0.03)) = 0.01
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha  // Добавляем прозрачность
        ZWrite Off                       // Отключаем запись в буфер глубины для прозрачности

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float4 _OutlineColor;
            float _OutlineThickness;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Основной цвет текстуры с учетом альфа-канала
                float4 mainColor = tex2D(_MainTex, i.uv) * _Color;
                
                // Простое освещение с резкими градациями для эффекта комикса
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float brightness = saturate(dot(i.worldNormal, lightDir));

                // Ступенчатое освещение
                if (brightness > 0.75)
                    brightness = 1.0;  // Очень светлая область
                else if (brightness > 0.5)
                    brightness = 0.7;  // Средняя светлая область
                else if (brightness > 0.25)
                    brightness = 0.4;  // Средняя темная область
                else
                    brightness = 0.1;  // Тёмная область

                // Цвет с резкими градациями освещения
                float4 shadedColor = mainColor * brightness;

                // Контур: основан на угле между направлением камеры и нормалью
                float3 viewDir = normalize(i.worldPos - _WorldSpaceCameraPos);
                float outlineFactor = pow(1.0 - saturate(dot(viewDir, i.worldNormal)), 4.0);
                float4 outlineColor = lerp(shadedColor, _OutlineColor, outlineFactor * _OutlineThickness);

                // Учитываем альфа-канал текстуры
                outlineColor.a = mainColor.a;
                
                // Возвращаем конечный результат
                return outlineColor;
            }
            ENDCG
        }
    }
}
