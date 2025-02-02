using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.VFX;
public class GraveProjectile : MonoBehaviour
{
    private float gravityScale = 1f; // Настраиваемая скорость падения
    private Rigidbody rb;
    private bool hasCollided = false; // Флаг для общего столкновения
    private bool hasLanded = false; // Флаг для проверки, было ли приземление
    public GameObject landingEffect; // Префаб для эффекта приземления
    public Transform effectParent; // Пустой объект внутри модели для размещения эффекта
    public Animator GRAnim;

    private float bounceHeight = 2f; // Начальная высота отскока
    private float bounceMultiplier = 1.5f; // Множитель высоты отскока при каждом столкновении
    private float maxBounceHeight = 10f; // Максимальная высота отскока
    private float bounceDuration = 0.5f; // Длительность отскока
    private float rotationSpeed = 1080f; // Скорость вращения в градусах за секунду

    public float collisionTimeout = 3f; // Время до автоматического засчитывания столкновения
    private float collisionTimer = 0f; // Таймер для отслеживания времени

    [Header("Robo Arms Settings")]
    public float roboArmsSpawnInterval = 1f; // Интервал между появлением роборуки (в секундах)
    public float roboArmsLifetime = 2f; // Время существования роборуки (в секундах)

    public GameObject roboArmsPrefab; // Префаб для роборук
    public Transform roboArmsHolder; // Пустой объект для размещения роборук (RoboArmsHolder)
    public float clawsActiveTime;
    public float trailActiveTime = 2f;
    public int armSlashDamage = 50;
    public LayerMask targetLayer; // Слой, на котором находятся цели
    public Vector3 armSlashSphereCenter; // Центр области удара (локальные координаты)
    public float armSlashSphereSize;   // Размер области удара
    private bool isTrailActive;
    private float savedYRotation;
    private Mesh[] cachedMeshes;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;

    [Header("MashRelated")]
    public float meshRefrashRate = 0.1f;
    public Transform positionToSpawn;
    public float meshDestroyDelay = 0.5f;

    [Header("Shader Related")]
    public Material trailMat;
    public string shaderVarRef;
    public float shaderVarRate = 0.1f;
    public float shaderVarRefreshRate = 0.05f;


    [Header("Effect Around Grave")]
    public GameObject[] surroundingEffectPrefabs; // Массив префабов эффектов
    public float effectRadius = 5f; // Радиус, в котором будут появляться эффекты
    public float effectFrequency = 1f; // Частота появления эффекта (в секундах)

    private bool isEffectSpawning = false; // Флаг для того, чтобы не запускать несколько раз корутину
    private Health enemyHealth;
    private GraveWeapon grWeapon;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("GraveProjectile требует компонент Rigidbody!");
        }

        if (effectParent == null)
        {
            Debug.LogError("Не назначен пустой объект для эффекта (effectParent)!");
        }

        isTrailActive = false;

        if (!isTrailActive)
        {
            StartCoroutine(ActiveTrail(trailActiveTime));
        }

        rb.useGravity = false;
        savedYRotation = transform.eulerAngles.y;

        if (effectParent == null)
        {
            Debug.LogWarning("EffectParent не назначен. Эффект приземления будет использовать позицию объекта.");
        }
        else if (effectParent.position.y > transform.position.y)
        {
            Debug.LogWarning("EffectParent расположен выше объекта. Проверьте его позицию.");
        }

        GRAnim.SetTrigger("Opened");

        VisualEffect[] vfxComponents = GetComponentsInChildren<VisualEffect>();

        foreach (var vfx in vfxComponents)
        {
            vfx.Stop(); // Останавливаем эффект, если он начал проигрываться
        }
    }
    private void Update()
    {
        // Проверяем, если объект ещё не столкнулся, увеличиваем таймер
        if (!hasCollided && !hasLanded)
        {
            collisionTimer += Time.deltaTime;

            if (collisionTimer >= collisionTimeout)
            {
                // Засчитываем автоматическое столкновение
                hasCollided = true;
                SimulateCollision();
            }
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.freezeRotation = true;

            if (!hasLanded) // Прекращаем вращение после приземления
            {
                // Применяем пользовательскую гравитацию
                Vector3 gravity = gravityScale * Physics.gravity * Time.fixedDeltaTime;
                rb.velocity += gravity;

                // Вращаем объект вокруг своей оси во время полёта
                transform.Rotate(Vector3.right * rotationSpeed * Time.fixedDeltaTime, Space.Self);
            }
        }



    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground") && !hasLanded)
        {
            hasLanded = true;
            HandleLanding();


            // Останавливаем вращение
            rb.velocity = Vector3.zero; // Останавливаем движение
                                        //  rb.angularVelocity = Vector3.zero; // Полностью убираем вращение
                                        //  transform.rotation = Quaternion.Euler(0, 90, 90); // Фиксируем финальную ориентацию
            rb.useGravity = true;

            // Вызываем дополнительный метод для приземления
            OnGraveLanded();
        }
        else if (!hasCollided)
        {
            hasCollided = true;
            HandleCollision(collision);
        }
    }

    private void OnGraveLanded()
    {
        // Логика при приземлении гроба
        Debug.Log("Гроб приземлился!");

        DOTween.Play("Grave_Shaking");

        // Создание эффекта при приземлении
        if (landingEffect != null)
        {
            CreateLandingEffect();
        }

        // Спавн роборуки
        if (roboArmsHolder != null && roboArmsPrefab != null)
        {
            StartCoroutine(SpawnRoboArmsRoutine(clawsActiveTime - 1f));
            Debug.Log("Роборуки созданы в RoboArmsHolder");
        }

        // Спавн эффекта когтей
        if (!isEffectSpawning)
        {
            StartCoroutine(SpawnClawEffect(clawsActiveTime));
            isEffectSpawning = true;
        }

        // Устанавливаем триггер анимации
        GRAnim.SetTrigger("Opened");

        // Запускаем блокировку через секунду
        StartCoroutine(BlockMovementAfterDelay(1f));

        DealDamage(50f, 10, 5, armSlashSphereSize/2);
    }

    private IEnumerator BlockMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero; // Останавливаем скорость
            rb.angularVelocity = Vector3.zero; // Останавливаем вращение
            rb.constraints = RigidbodyConstraints.FreezeAll; // Замораживаем все движения
        }

        Debug.Log("Гроб заблокирован спустя " + delay + " секунд.");
    }

    private IEnumerator SpawnClawEffect(float duration) // Спавн эффектов когтей с ограничением по времени
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Выбираем случайный эффект из массива
            GameObject randomEffectPrefab = surroundingEffectPrefabs[Random.Range(0, surroundingEffectPrefabs.Length)];

            // Находим все компоненты VisualEffect на объекте
            VisualEffect[] vfxComponents = GetComponentsInChildren<VisualEffect>();

            if (vfxComponents.Length > 0)
            {
                // Выбираем случайный эффект среди существующих
                VisualEffect randomEffect = vfxComponents[Random.Range(0, vfxComponents.Length)];

                // Воспроизводим выбранный эффект
                randomEffect.Play();
            }
            else
            {
                Debug.LogWarning("Не найдено VFX компонентов для воспроизведения эффекта.");
            }

            // Ждем заданную частоту появления эффекта
            yield return new WaitForSeconds(effectFrequency);

            // Увеличиваем прошедшее время
            elapsedTime += effectFrequency;
        }
    }

    private IEnumerator SpawnRoboArmsRoutine(float duration) // Спавн роборуки с триггером
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Создаем роборуку
            if (roboArmsHolder != null && roboArmsPrefab != null)
            {
                GameObject roboArms = Instantiate(roboArmsPrefab, roboArmsHolder.position, Quaternion.identity, roboArmsHolder);
                Debug.Log("Роборуки созданы в RoboArmsHolder");
                DealDamage(armSlashDamage, -10f, 0f, armSlashSphereSize);

                // Удаляем роборуку через заданное время
                Destroy(roboArms, roboArmsLifetime);
            }
            else
            {
                Debug.LogWarning("roboArmsHolder или roboArmsPrefab не назначены!");
            }

            // Ждем заданный интервал перед следующим появлением
            yield return new WaitForSeconds(roboArmsSpawnInterval);

            // Увеличиваем прошедшее время
            elapsedTime += roboArmsSpawnInterval;
        }

        // После завершения работы корутины устанавливаем триггер
        if (GRAnim != null && !string.IsNullOrEmpty("Closed"))
        {
            GRAnim.SetTrigger("Closed");
          
        }


    }

    public void DealDamage(float damage, float knockBackPower, float verticalKnockbackPower, float radius)
    {
        Vector3 sphereCenter = transform.TransformPoint(armSlashSphereCenter);

        // Получение всех объектов в области удара
        Collider[] hits = Physics.OverlapSphere(sphereCenter, radius, targetLayer);

        foreach (Collider hit in hits)
        {
            Debug.Log("Удар по объекту: " + hit.name);

            // Проверка на наличие компонента здоровья
            Health enemyHealth = hit.GetComponent<Health>();
            if (enemyHealth != null)
            {
                // Наносим урон
                enemyHealth.TakeDamage(damage, DamageHandler.AttackType.Grave);

               

                // Отталкиваем врага
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 knockbackDirection = (hit.transform.position - transform.position).normalized;

                    Vector3 knockbackForce = new Vector3(
                        knockbackDirection.x * knockBackPower,
                        verticalKnockbackPower,
                        knockbackDirection.z * knockBackPower
                    );

                    rb.AddForce(knockbackForce, ForceMode.Impulse);
                }
            }
        }
    }


    private void CacheMeshes()
    {
        if (skinnedMeshRenderers == null)
            skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        cachedMeshes = new Mesh[skinnedMeshRenderers.Length];
        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            cachedMeshes[i] = new Mesh();
            skinnedMeshRenderers[i].BakeMesh(cachedMeshes[i]);
        }
    }

    IEnumerator ActiveTrail(float timeActive)
    {
        CacheMeshes();

        while (timeActive > 0)
        {
            timeActive -= meshRefrashRate;

            for (int i = 0; i < cachedMeshes.Length; i++)
            {
                GameObject gObj = new GameObject();
                gObj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                MeshFilter mf = gObj.AddComponent<MeshFilter>();

                mf.mesh = cachedMeshes[i];
                mr.material = trailMat;

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                Destroy(gObj, meshDestroyDelay);
            }

            yield return new WaitForSeconds(meshRefrashRate);
        }

        isTrailActive = false;
    }

    IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshRate)
    {
        float valueToAnimate = mat.GetFloat(shaderVarRef);

        while (valueToAnimate > goal)
        {
            valueToAnimate += rate;
            mat.SetFloat(shaderVarRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }

    private void SimulateCollision()
    {
        Debug.Log("Гроб автоматически столкнулся через таймаут!");

        // Эмулируем столкновение, создавая фейковый объект Collision
        Collision fakeCollision = new Collision();
        HandleCollision(fakeCollision);
    }

    private void HandleCollision(Collision collision)
    {
        // Останавливаем движение
        if (rb != null)
        {
            rb.velocity = Vector3.zero;    // Останавливаем скорость
            rb.angularVelocity = Vector3.zero; // Останавливаем вращение
        }

        Debug.Log($"Гроб столкнулся с {(collision.collider != null ? collision.collider.name : "фиктивным объектом")}");

        // Проверяем, если объект имеет тег "Enemy"
        if (collision.collider != null && collision.collider.CompareTag("Enemy"))
        {
            Debug.Log("Гроб попал во врага!");
            // Логика нанесения урона врагу
        }

        // Анимация отскока
        AnimateBounce();

        // Увеличиваем высоту отскока после каждого столкновения
        IncreaseBounceHeight();
    }

    private void HandleLanding()
    {
        // Создаём эффект приземления
        CreateLandingEffect();
    }

    private void CreateLandingEffect()
    {
        if (landingEffect != null)
        {
            // Используем позицию effectParent, если он указан, иначе transform.position
            Vector3 spawnPosition = effectParent != null ? effectParent.position : transform.position;

            // Корректируем позицию эффекта так, чтобы он был на уровне земли
            RaycastHit hit;
            if (Physics.Raycast(spawnPosition + Vector3.up, Vector3.down, out hit, Mathf.Infinity))
            {
                spawnPosition = hit.point; // Ставим эффект на точку соприкосновения с землей
            }
            else
            {
                Debug.LogWarning("Raycast не нашел землю под объектом! Используется текущая позиция.");
            }

            // Создаём эффект в скорректированной позиции
            GameObject effect = Instantiate(landingEffect, spawnPosition, Quaternion.identity);

            // Устанавливаем начальный размер эффекта
            effect.transform.localScale = Vector3.zero;

            // Анимация увеличения эффекта до заданного размера
            effect.transform.DOScale(Vector3.one * 0.5f, 0.5f).SetEase(Ease.OutQuad);

            // Уничтожаем эффект через 5 секунд
            Destroy(effect, 5f);
        }
        else
        {
            Debug.LogError("Префаб для эффекта приземления не назначен!");
        }
    }

    private void AnimateBounce()
    {
        // Сохраняем текущий глобальный поворот в начале анимации
        Quaternion initialRotation = transform.rotation;

        // Создаём последовательность для анимации
        Sequence bounceSequence = DOTween.Sequence();

        // Анимация подъёма с новым типом Ease
        bounceSequence.Append(transform.DOLocalMoveY(transform.position.y + bounceHeight, bounceDuration / 2)
                           .SetEase(Ease.OutBack)); // Плавный подъем с "взмахом"

        // Добавляем вращение вокруг оси X
        bounceSequence.Join(transform.DOLocalRotate(new Vector3(360, 90, 90), bounceDuration / 2, RotateMode.FastBeyond360)
                           .SetEase(Ease.OutBack)); // Вращение синхронно с подъемом

        // Плавное и тяжелое падение
        bounceSequence.Append(transform.DOLocalMoveY(0, bounceDuration / 2)
                           .SetEase(Ease.InQuart)); // Ускоренное падение с эффектом тяжести

        // Вертикальное выравнивание после завершения анимации
        bounceSequence.AppendCallback(() =>
        {
            // Выравниваем объект вертикально и возвращаем сохранённый поворот
            transform.rotation = Quaternion.Euler(0, initialRotation.eulerAngles.y, 90);
        });



        // Запускаем анимацию
        bounceSequence.Play();
    }
    private void IncreaseBounceHeight()
    {
        // Увеличиваем высоту отскока
        bounceHeight = Mathf.Min(bounceHeight * bounceMultiplier, maxBounceHeight);
    }

    public void SetGravityScale(float scale)
    {
        gravityScale = scale;
    }


    private void OnDrawGizmosSelected()
    {
        // Отображаем область удара в редакторе
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(armSlashSphereCenter, armSlashSphereSize);
    }

}