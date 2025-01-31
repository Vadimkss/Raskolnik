using UnityEngine;

public class Teleporting : MonoBehaviour
{
    public Transform teleportTarget; // Ссылка на точку назначения для телепортации

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Проверка на столкновение с объектом игрока
        {
            other.transform.position = teleportTarget.position; // Перемещаем игрока в точку назначения
        }
    }
}
