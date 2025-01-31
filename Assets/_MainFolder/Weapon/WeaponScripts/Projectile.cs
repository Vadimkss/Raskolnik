using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))] // Автоматически добавляем Rigidbody
public class Projectile : MonoBehaviour
{
    // Основные параметры проджектайла
    private float damage;
    private float speed;
    private DamageHandler.AttackType attackType;
    private Vector3 direction;

    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private float visualScale = 0.2f; // Размер снаряда
    [SerializeField] private WeaponConfigSO config;
    [SerializeField] private float currentDamage;

    // Компоненты
    private Rigidbody rb;

    // Флаги состояния
    private bool isInitialized;

    // Дополнительные настройки
    [Tooltip("Время жизни проджектайла в секундах")]
    public float lifeTime = 5f;

    [Tooltip("Использовать физическую симуляцию")]
    public bool usePhysics = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Если нет визуальных компонентов, создаем базовую сферу
        if (meshRenderer == null && trailRenderer == null)
        {
            GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visualObject.transform.SetParent(transform);
            visualObject.transform.localPosition = Vector3.zero;
            visualObject.transform.localScale = Vector3.one * visualScale;

            // Удаляем коллайдер с визуального объекта, так как коллизии обрабатываются на основном объекте
            Destroy(visualObject.GetComponent<Collider>());

            meshRenderer = visualObject.GetComponent<MeshRenderer>();

            // Опционально добавляем трейл
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
            trailRenderer.time = 0.2f; // Время жизни следа
            trailRenderer.startWidth = 0.1f;
            trailRenderer.endWidth = 0.0f;
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        // Настраиваем физику
        if (rb != null)
        {
            rb.useGravity = false; // Отключаем гравитацию для прямолинейного движения
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Улучшенное определение столкновений
            rb.interpolation = RigidbodyInterpolation.Interpolate; // Сглаживание движения
        }
    }

    public void Initialize(float damage, float speed, DamageHandler.AttackType attackType, WeaponConfigSO inConfig)
    {
        config = inConfig;
        startTime = Time.time;
        Debug.Log($"Projectile initialized at time: {startTime}");

        this.damage = damage;
        this.speed = speed;
        this.attackType = attackType;
        this.direction = transform.forward;
        isInitialized = true;


        if (usePhysics && rb != null)
        {
            rb.velocity = direction * speed;
        }

        // Добавляем отладку для таймера уничтожения
        Debug.Log($"Setting lifetime timer: {lifeTime} seconds");
        Destroy(gameObject, lifeTime);
        if(config.damageDecreaseByDistance)  StartCoroutine(CalculateDamageByDistance());
        else currentDamage = config.damage;
    }

    private void Update()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Projectile not initialized!");
            return;
        }

        if (!usePhysics)
        {
            transform.position += direction * (speed * Time.deltaTime);
            Debug.Log($"Moving projectile. Position: {transform.position}");
        }
        else if (rb != null && rb.velocity.magnitude != speed)
        {
            rb.velocity = direction * speed;
        }
    }

    private IEnumerator CalculateDamageByDistance()
    {
        float elapsedTime = 0f;
        float maxTime = config.range / speed; // время до максимальной дальности

        while (elapsedTime < maxTime)
        {
            elapsedTime += Time.deltaTime;
            currentDamage = damage * (1 - elapsedTime / maxTime);
            currentDamage = Mathf.Max(currentDamage, 0); // Урон не может быть ниже 0
            yield return null; // ждем следующий кадр
        }

    }

    private void OnDestroy()
    {
        Debug.Log($"Projectile destroyed at position: {transform.position}. Time alive: {Time.time - startTime}");
    }

    private float startTime;

   

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Projectile collided with: {collision.gameObject.name} at position: {collision.contacts[0].point}");

        if (collision.gameObject.TryGetComponent<IDamageable>(out var target))
        {
            Debug.Log($"Hit damageable object: {collision.gameObject.name}");
            target.TakeDamage(currentDamage, attackType);
            SpawnHitEffect(collision.contacts[0].point);
        }

        Debug.Log("Destroying projectile due to collision");
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Projectile triggered with: {other.gameObject.name} at position: {transform.position}");

        if (other.TryGetComponent<IDamageable>(out var target))
        {
            Debug.Log($"Hit damageable object (trigger): {other.gameObject.name}");
            target.TakeDamage(currentDamage, attackType);
            SpawnHitEffect(transform.position);
        }

        Debug.Log("Destroying projectile due to trigger");
        Destroy(gameObject);
    }

    private void SpawnHitEffect(Vector3 hitPoint)
    {
        // Здесь можно добавить создание визуальных эффектов при попадании
        // Например, партиклы или декали
    }

    // Визуализация траектории в редакторе
    private void OnDrawGizmos()
    {
        if (isInitialized)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }
    }
}