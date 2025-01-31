using UnityEngine;

public class GroundCracks : MonoBehaviour
{
    public GameObject cracksObject; // Ссылка на объект с трещинами

    public void OnCollisionEnter(Collision collision)
    {
        // Проверяем, столкнулся ли персонаж с землей
        if (collision.gameObject.CompareTag("Floor"))
        {
            // Активируем объект трещин
            cracksObject.SetActive(true);
        }
    }
}
