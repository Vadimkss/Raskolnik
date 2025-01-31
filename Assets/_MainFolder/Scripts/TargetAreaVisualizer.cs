using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Не забудьте подключить пространство имен DoTween

public class TargetAreaVisualizer : MonoBehaviour
{
    public Image aimWindowImage; // UI-элемент для отображения области
    public float aimWindowSize = 200f; // Размер "окна" для прицеливания

    RectTransform rectTransform;

    void Start()
    {
        rectTransform = aimWindowImage.GetComponent<RectTransform>();
    }

    void Update()
    {
        // Обновляем размер и позицию "окна" в зависимости от размера области захвата
        if (rectTransform != null)
        {
            // Позиция "окна" должна быть по центру экрана
            rectTransform.anchoredPosition = Vector2.zero;

            // Размеры "окна"
            rectTransform.sizeDelta = new Vector2(aimWindowSize, aimWindowSize);
        }
    }
}