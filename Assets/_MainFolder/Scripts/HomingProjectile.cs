using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    public float speed = 10f; // —корость полета
    public GameObject target; // ÷ель, на которую направл€етс€ снар€д
    public float rotationSpeed = 200f; // —корость поворота снар€да к цели
    public SphereCollider targetHeadCollider; //  оллайдер головы цели

    void Update()
    {
        // ≈сли цели или коллайдера головы нет, уничтожаем снар€д
        if (target == null || targetHeadCollider == null)
        {
            Destroy(gameObject);
            return;
        }

        // ѕолучаем позицию головы (или другой части тела, к которой нужно наводитьс€)
        Vector3 targetPosition = targetHeadCollider.transform.position;

        // ќтладочный вывод
        Debug.DrawLine(transform.position, targetPosition, Color.red); // Ћини€ между снар€дом и целью
        Debug.Log($"Target Position: {targetPosition}");
        Debug.Log($"Projectile Position: {transform.position}");

        // Ќаправл€ем снар€д в сторону головы
        Vector3 direction = (targetPosition - transform.position).normalized;

        // —оздаем кватернион дл€ поворота в сторону цели
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // ѕоворачиваем снар€д в сторону цели с заданной скоростью
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

        // ƒвигаем снар€д вперед в направлении его forward
        // ѕримен€ем вектор направлени€
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ≈сли снар€д попал в цель, уничтожаем его
        if (other.gameObject == target)
        {
            // «десь можно добавить логику уничтожени€ цели или нанесени€ урона
            Destroy(gameObject); // ”ничтожаем снар€д при столкновении с целью
        }
    }
}
