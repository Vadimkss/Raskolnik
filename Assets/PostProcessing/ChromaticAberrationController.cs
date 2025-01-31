using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticAberrationController : MonoBehaviour
{
    public Volume postProcessingVolume;
    private ChromaticAberration chromaticAberration;
    private float targetIntensity = 0f; // Целевая интенсивность эффекта
    private float transitionDuration = 1f; // Длительность перехода
    private float transitionTimer = 0f; // Таймер для перехода

    private void Start()
    {
        // Получаем компонент хроматической аберрации из VolumeProfile
        if (postProcessingVolume != null && postProcessingVolume.profile != null)
        {
            if (postProcessingVolume.profile.TryGet<ChromaticAberration>(out chromaticAberration))
            {
                // Начальная интенсивность
                chromaticAberration.intensity.value = targetIntensity;
            }
            else
            {
                Debug.LogWarning("ChromaticAberration component not found in the VolumeProfile!");
            }
        }
        else
        {
            Debug.LogWarning("PostProcessingVolume or its profile is not assigned!");
        }
    }

    private void Update()
    {
        // Плавное изменение интенсивности эффекта
        if (chromaticAberration != null && transitionTimer < transitionDuration)
        {
            transitionTimer += Time.deltaTime;
            float currentIntensity = Mathf.Lerp(chromaticAberration.intensity.value, targetIntensity, transitionTimer / transitionDuration);
            chromaticAberration.intensity.value = currentIntensity;
        }
    }

    // Метод для запуска плавного перехода интенсивности от 0 до 0.5
    public void StartIncreaseIntensityTransition()
    {
        if (chromaticAberration != null)
        {
            targetIntensity = 0.5f;
            transitionTimer = 0f;
        }
        else
        {
            Debug.LogWarning("ChromaticAberration component is not initialized!");
        }
    }

    // Метод для запуска плавного перехода интенсивности от 0.5 до 0
    public void StartDecreaseIntensityTransition()
    {
        if (chromaticAberration != null)
        {
            targetIntensity = 0f;
            transitionTimer = 0f;
        }
        else
        {
            Debug.LogWarning("ChromaticAberration component is not initialized!");
        }
    }
}
