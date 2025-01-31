using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swayAmount = 0.05f;  // Амплитуда колебаний
    public float maxSwayAmount = 0.1f; // Максимальная амплитуда колебаний
    public float swaySpeed = 3f;      // Скорость колебаний

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    [Header("Tilt Settings")]
    public float tiltAmount = 5f;     // Угол наклона оружия

    void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        // Получаем смещение мыши
        float mouseX = Input.GetAxis("Mouse X") * swayAmount;
        float mouseY = Input.GetAxis("Mouse Y") * swayAmount;

        // Ограничиваем смещение
        mouseX = Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount);
        mouseY = Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount);

        // Вычисляем новое положение оружия
        Vector3 finalPosition = new Vector3(mouseX, mouseY, 0f);
        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition + finalPosition, Time.deltaTime * swaySpeed);

        // Вычисляем новый наклон оружия
        Quaternion xTilt = Quaternion.AngleAxis(-mouseX * tiltAmount, Vector3.up);
        Quaternion yTilt = Quaternion.AngleAxis(mouseY * tiltAmount, Vector3.right);
        Quaternion finalRotation = initialRotation * xTilt * yTilt;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation, Time.deltaTime * swaySpeed);
    }
}
