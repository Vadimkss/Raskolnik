using DG.Tweening;
using UnityEngine;

public class GraveWeapon : MonoBehaviour
{
    private ComboManager comboManager;
    public Animator GraveAnimator;
    public Animator RevolverAnimator;
    public Transform cam;
    public GameObject gravePrefab;       // Префаб гроба
    public Transform throwPoint;        // Точка, из которой будет появляться гроб
    public float throwForce = 15f;      // Сила броска
    public float upwardForce = 5f;      // Дополнительная вертикальная сила
    public Vector3 customRotation;     // Настраиваемый поворот (углы в градусах)
    public float fixedSpeed = 10f;     // Постоянная скорость полёта
    public float gravityScale = 1f;    // Настраиваемая скорость падения
    public bool graveActive = true;
    private Health enemyHealth;
    public LayerMask targetLayer; // Слой, на котором находятся цели

    public Vector3 punchBoxCenter; // Центр области удара (локальные координаты)
    public Vector3 punchBoxSize;   // Размер области удара
    public int punchDamage = 50;        // Урон от удара (можно настроить через инспектор)
    public float damageDelay = 0.3f;    // Задержка перед нанесением урона (можно настроить через инспектор)
    private bool hasDealtDamage = false;  // Флаг для предотвращения повторного нанесения урона




    private void Start()
    {
        comboManager = FindObjectOfType<ComboManager>();
        GraveAnimator.SetBool("GraveActive", true);
    }

    void Awake()
    {
       
    }

    private void Update()
    {
        if (graveActive)
        {
            GraveAnimator.SetBool("GraveActive", true);
        }
    }

    public void GraveAttack()
    {
        comboManager.RegisterAttack("Grave");  // Регистрируем удар гробом
        Debug.Log("ГробРег");
    }

    public void ChargedGraveAttack()
    {
        comboManager.RegisterAttack("CHGrave");  // Регистрируем удар гробом
        Debug.Log("ГробРег");
    }

    public void ThrowGrave()
    {
        Debug.Log("Бросок гроба!");

        GraveAnimator.SetBool("GraveActive", false);

        // Создаём гроб
        GameObject thrownGrave = Instantiate(gravePrefab, throwPoint.position, throwPoint.rotation);

        // Применяем пользовательский поворот
        thrownGrave.transform.rotation = throwPoint.rotation * Quaternion.Euler(customRotation);

        // Рассчитываем направление броска
        Vector3 throwDirection = (throwPoint.forward + throwPoint.up * upwardForce).normalized;

        // Получаем компонент Rigidbody
        Rigidbody rb = thrownGrave.GetComponentInChildren<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Префаб гроба не имеет компонента Rigidbody!");
            return;
        }

        // Устанавливаем фиксированную скорость полёта
        rb.velocity = throwDirection * fixedSpeed;

        // Настраиваем пользовательскую "гравитацию"
        GraveProjectile graveProjectile = thrownGrave.GetComponent<GraveProjectile>();
        if (graveProjectile != null)
        {
            graveProjectile.SetGravityScale(gravityScale);
        }

        graveActive = false;

    }


    private void OnGraveHit(GameObject grave, Collider other)
    {
      

        // Останавливаем движение гроба
        Rigidbody rb = grave.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;   // Останавливаем скорость
            rb.angularVelocity = Vector3.zero; // Останавливаем вращение
        }

        // Дополнительные действия при попадании
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Гроб попал во врага!");
            // Здесь можно вызвать метод нанесения урона врагу
        }

        // Гроб остаётся лежать на земле
    }

    private void AnimateBounce()
    {
        // Начальная высота отскока
        float bounceHeight = 2f;
        // Длительность отскока
        float bounceDuration = 0.5f;

        // Множитель высоты отскока с каждым прыжком
        float bounceHeightMultiplier = 1.5f;

        // Коэффициент ослабления силы отскока с каждым прыжком
        float bounceDecay = 0.7f;

        // Создаём последовательность
        Sequence bounceSequence = DOTween.Sequence();

        // Анимация подъёма с вращением (вдоль оси Y)
        bounceSequence.Append(transform.DOLocalMoveY(transform.position.y + bounceHeight, bounceDuration / 2)
                           .SetEase(Ease.OutQuad)); // Плавное поднятие

        bounceSequence.Join(transform.DOLocalRotate(new Vector3(0, 90, 80), bounceDuration / 2, RotateMode.FastBeyond360)
                           .SetEase(Ease.Linear)); // Вращение вдоль оси Y во время подъёма

        // Плавное падение
        bounceSequence.Append(transform.DOLocalMoveY(0, bounceDuration / 2)
                           .SetEase(Ease.InQuad)); // Плавное падение

        // После падения фиксируем ориентацию по оси X и Z
        bounceSequence.AppendCallback(() =>
        {
            // Применяем корректную ориентацию
            transform.localRotation = Quaternion.Euler(0, 90, 80);

            // Увеличиваем высоту отскока для следующего прыжка
            bounceHeight *= bounceHeightMultiplier;

            // Понижаем силу отскока с каждым новым падением
            bounceDuration *= bounceDecay;

            // Создаём эффект отталкивания объектов с Rigidbody
            Collider[] colliders = Physics.OverlapSphere(transform.position, 2f); // Радиус вокруг гроба для поиска объектов
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Enemy") || collider.GetComponent<Rigidbody>())
                {
                    Rigidbody rb = collider.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        // Придаём импульс объектам с Rigidbody
                        rb.AddForce(Vector3.up * 5f, ForceMode.Impulse); // Подаём импульс вверх
                    }
                }
            }

            // Создание визуального эффекта при приземлении
        });

        // Запускаем следующую анимацию с обновлёнными значениями
        bounceSequence.Play();
    }

    public void GrPunch1()
    {
        Debug.Log("Анимация первого удара гробом");
        ExecutePunch("Punch1", new Vector3(-5, 15, -5), 0.4f);
        // Устанавливаем урон и задержку для первого удара
        punchDamage = 50; // Настроить урон
        damageDelay = 0.3f; // Настроить задержку
    }

    public void GrPunch2()
    {
        Debug.Log("Анимация второго удара гробом");
        ExecutePunch("Punch2", new Vector3(-5, -15, 5), 0.6f);
        RevolverAnimator.SetTrigger("GoBack");
        // Устанавливаем урон и задержку для второго удара
        punchDamage = 70; // Настроить урон
        damageDelay = 0.4f; // Настроить задержку
    }

    public void GrPunch3()
    {
        Debug.Log("Анимация третьего удара гробом");
        ExecutePunch("Punch3", new Vector3(-10, 0, 0), 0.5f);
        // Устанавливаем урон и задержку для третьего удара
        punchDamage = 100; // Настроить урон
        damageDelay = 0.5f; // Настроить задержку
    }

    private void ExecutePunch(string animationTrigger, Vector3 cameraRotation, float duration)
    {
        // Вызов анимации
        GraveAnimator.SetTrigger(animationTrigger);

        // Создаем последовательность для камеры
        Sequence sequence = DOTween.Sequence();

        // Плавный переход камеры
        sequence.Append(cam.DOLocalRotate(cameraRotation, duration / 2).SetEase(Ease.Linear));
        sequence.Append(cam.DOLocalRotate(Vector3.zero, duration).SetEase(Ease.InOutBack));
        DealDamageDelayed();
       
    }


    public void DealDamageDelayed()
    {
        // Проверка на нанесение урона
        if (!hasDealtDamage)
        {
            hasDealtDamage = true;  // Устанавливаем флаг, чтобы предотвратить повторный вызов
            Invoke("DealDamage", damageDelay);  // Делаем задержку перед нанесением урона
        }
    }

    private void DealDamage()
    {
        // Преобразование центра из локальных в мировые координаты
        Vector3 boxCenter = transform.TransformPoint(punchBoxCenter);

        // Получение всех объектов в области удара
        Collider[] hits = Physics.OverlapBox(boxCenter, punchBoxSize / 2, transform.rotation, targetLayer);

        foreach (Collider hit in hits)
        {
            Debug.Log("Удар по объекту: " + hit.name);

            // Проверка на наличие компонента здоровья
            Health enemyHealth = hit.GetComponent<Health>();
            if (enemyHealth != null)
            {
                // Наносим урон
                enemyHealth.TakeDamage(punchDamage, DamageHandler.AttackType.Grave);

                

                // Проверяем смерть врага
                if (enemyHealth.health <= 0)
                {
                    Debug.Log("Враг уничтожен: " + hit.name);
                }

                // Отталкиваем врага с учетом направления удара
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Направление отталкивания от центра удара (от объекта игрока)
                    Vector3 knockbackDirection = (hit.transform.position - transform.position).normalized;

                    // Применяем силу отталкивания
                    rb.AddForce(knockbackDirection * 5f, ForceMode.Impulse); // Можно настроить силу отталкивания
                }
            }
            else
            {
                Debug.LogWarning("У цели нет компонента Health: " + hit.name);
            }
        }

        // После того как урон был нанесён, сбрасываем флаг
        hasDealtDamage = false;
    }



    private void OnDrawGizmosSelected()
    {
        // Отображаем область удара в редакторе
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(punchBoxCenter, punchBoxSize);
    }

    public void StartComboAnimation()
    {
        GraveAnimator.SetTrigger("ComboAttack");
    }
}
