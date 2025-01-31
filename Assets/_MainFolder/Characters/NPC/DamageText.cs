using TMPro; // Убедитесь, что вы используете TextMeshPro
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public float minRiseSpeed = 1f; // Минимальная скорость подъема текста
    public float maxRiseSpeed = 3f; // Максимальная скорость подъема текста
    public float lifetime = 1f; // Время жизни текста
    public float randomSideForce = 0.5f; // Максимальная сила, придаваемая тексту в стороны
    public TextMeshPro textMeshPro; // Ссылка на компонент TextMeshPro

    private Vector3 force;
    private float riseSpeed;

    void Start()
    {
        if (textMeshPro == null)
        {
            Debug.LogError("TextMeshPro component is missing on this GameObject.");
            return; // Прерываем выполнение, если компонент не найден
        }

        // Устанавливаем случайную скорость подъема
        riseSpeed = Random.Range(minRiseSpeed, maxRiseSpeed);

        // Применяем случайную силу в стороны
        force = new Vector3(Random.Range(-randomSideForce, randomSideForce), riseSpeed, Random.Range(-randomSideForce, randomSideForce));
        Destroy(gameObject, lifetime); // Уничтожаем объект через lifetime

     
    }

    void Update()
    {
        // Поворачиваем текст к камере (игроку)
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 lookDirection = mainCamera.transform.position - transform.position;
            // Установите небольшую поправку, чтобы предотвратить отображение зеркального текста
            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = rotation * Quaternion.Euler(0, 180, 0); // Поворот на 180 градусов по оси Y
        }

        // Применяем силу
        transform.position += force * Time.deltaTime;

        // Плавно уменьшаем альфа-канал с более медленной интерполяцией
        float t = Mathf.Clamp01(Time.time / lifetime); // Нормализуем значение времени от 0 до 1
        float alpha = Mathf.Lerp(1f, 1f, t * t); // Изменяем скорость исчезновения (t * t замедляет исчезновение)

        Color textColor = textMeshPro.color;
        textColor.a = alpha;
        textMeshPro.color = textColor;
    }

    public void Initialize(string damageAmount)
    {
        if (textMeshPro == null)
        {
            Debug.LogError("TextMeshPro component not found!");
            return; // Прерываем выполнение, если компонент не найден
        }
        textMeshPro.text = damageAmount; // Устанавливаем текст урона
    }
}
