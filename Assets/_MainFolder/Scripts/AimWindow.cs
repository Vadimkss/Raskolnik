using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Не забудьте подключить пространство имен DoTween

public class TargetVisualizer : MonoBehaviour
{
    public Image targetAreaImage; // UI-элемент для отображения области захвата
    public float animationDuration = 0.5f; // Длительность анимации появления

    private void Start()
    {
        // Изначально скрываем область захвата
        targetAreaImage.color = new Color(targetAreaImage.color.r, targetAreaImage.color.g, targetAreaImage.color.b, 0f);
        targetAreaImage.gameObject.SetActive(false);
    }

    public void ShowArea()
    {
        // Включаем область захвата и анимируем её появление
        targetAreaImage.gameObject.SetActive(true);
        targetAreaImage.DOFade(1f, animationDuration).SetEase(Ease.InOutSine);
    }

    public void HideArea()
    {
        // Анимируем исчезновение области захвата
        targetAreaImage.DOFade(0f, animationDuration).SetEase(Ease.InOutSine).OnComplete(() => targetAreaImage.gameObject.SetActive(false));
    }
}
