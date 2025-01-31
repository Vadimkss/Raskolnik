using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        // Находим главную камеру
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Не удалось найти главную камеру! Убедитесь, что в сцене есть камера с тегом MainCamera.");
        }
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Направляем "лицо" объекта на камеру
            Vector3 direction = mainCamera.transform.position - transform.position;
            direction.y = 0; // Игнорируем вертикальную ось для 2D-эффекта
            transform.rotation = Quaternion.LookRotation(-direction);
        }
    }
}