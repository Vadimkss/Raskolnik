using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HeartBeatController : MonoBehaviour
{
    public RectTransform heartRectTransform;  // Ссылка на RectTransform сердца
    public Image heartImage;  // Ссылка на компонент Image для сердца
    public Material heartMaterial;  // Материал с шейдером для управления цветом и сканированием

    public float beatDuration = 0.5f;  // Длительность одного биения
    public float beatPause = 1.0f;  // Время между биениями

    public float maxHeightScale = 1.1f;  // Максимальное увеличение по высоте
    public float maxWidthScale = 1.05f;  // Максимальное увеличение по ширине
    public float minHeightScale = 0.9f;  // Минимальный размер по высоте
    public float minWidthScale = 0.9f;  // Минимальный размер по ширине

    public Color normalColor = Color.white;  // Цвет сердца в покое
    public Color pulseColor = Color.red;  // Цвет пульсации
    public Color scanLineColor = Color.cyan;  // Цвет сканирующего эффекта
    public float scanLineWidth = 0.1f;  // Толщина сканирующего эффекта
    public float maxBlendFactor = 1.0f;  // Максимальный бленд фактор
    public float rippleAmount = 0.1f;  // Интенсивность ряби

    void Start()
    {
        // Начальное состояние
        heartRectTransform.localScale = new Vector3(minWidthScale, minHeightScale, 1f);
        heartMaterial.SetColor("_PulseColor", pulseColor);
        heartMaterial.SetFloat("_BlendFactor", 0f);
        heartMaterial.SetColor("_ScanColor", scanLineColor);
        heartMaterial.SetFloat("_ScanWidth", scanLineWidth);
        heartMaterial.SetFloat("_MaxBlendFactor", maxBlendFactor);
        heartMaterial.SetFloat("_RippleAmount", rippleAmount);

        // Анимация биения сердца
        Sequence heartBeatSequence = DOTween.Sequence()
            // Увеличение по высоте
            .Append(heartRectTransform.DOScaleY(maxHeightScale, beatDuration / 2).SetEase(Ease.InOutQuad))
            // Увеличение по ширине с одновременным уменьшением высоты
            .Join(heartRectTransform.DOScaleX(maxWidthScale, beatDuration / 2).SetEase(Ease.InOutQuad))
            .Join(DOTween.To(() => heartMaterial.GetFloat("_BlendFactor"),
                             x => heartMaterial.SetFloat("_BlendFactor", x),
                             1f, beatDuration / 2))
            // Возвращение к минимальным размерам
            .Append(heartRectTransform.DOScale(new Vector3(minWidthScale, minHeightScale, 1f), beatDuration / 2).SetEase(Ease.InOutQuad))
            .Join(DOTween.To(() => heartMaterial.GetFloat("_BlendFactor"),
                             x => heartMaterial.SetFloat("_BlendFactor", x),
                             0f, beatDuration / 2))
            // Время между биениями
            .AppendInterval(beatPause)
            .SetLoops(-1, LoopType.Restart);  // Бесконечный цикл анимации

        heartBeatSequence.Play();
    }

    // Функции для настройки параметров во время игры
    public void SetBeatPause(float newPause)
    {
        beatPause = newPause;
    }

    public void SetScanLineColor(Color newColor)
    {
        scanLineColor = newColor;
        heartMaterial.SetColor("_ScanColor", scanLineColor);
    }

    public void SetScanLineWidth(float newWidth)
    {
        scanLineWidth = newWidth;
        heartMaterial.SetFloat("_ScanWidth", scanLineWidth);
    }

    public void SetMaxBlendFactor(float newMaxBlendFactor)
    {
        maxBlendFactor = newMaxBlendFactor;
        heartMaterial.SetFloat("_MaxBlendFactor", maxBlendFactor);
    }

    public void SetRippleAmount(float newRippleAmount)
    {
        rippleAmount = newRippleAmount;
        heartMaterial.SetFloat("_RippleAmount", rippleAmount);
    }

    public void SetMinHeightScale(float newMinHeightScale)
    {
        minHeightScale = newMinHeightScale;
        heartRectTransform.localScale = new Vector3(heartRectTransform.localScale.x, minHeightScale, 1f);
    }

    public void SetMinWidthScale(float newMinWidthScale)
    {
        minWidthScale = newMinWidthScale;
        heartRectTransform.localScale = new Vector3(minWidthScale, heartRectTransform.localScale.y, 1f);
    }
}
