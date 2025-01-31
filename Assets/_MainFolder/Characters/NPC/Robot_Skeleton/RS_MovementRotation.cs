using UnityEngine;

public class RS_MovementRotation : MonoBehaviour
{
    private Vector3 previousPosition; // Сохраняем предыдущую позицию
    [SerializeField] private float rotationSpeed = 5f; // Скорость поворота для плавности

    void Start()
    {
        // Запоминаем начальную позицию
        previousPosition = transform.position;
    }

    void Update()
    {
        // Получаем вектор движения
        Vector3 movementDirection = transform.position - previousPosition;

        // Проверяем, что объект действительно движется
        if (movementDirection.magnitude > 0.01f)
        {
            // Создаем поворот в направлении движения
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

            // Плавно поворачиваем объект
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Сохраняем текущую позицию для следующего кадра
        previousPosition = transform.position;
    }
}