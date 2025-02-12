using UnityEngine;
using UnityEngine.VFX;
using DamageNumbersPro;
using Movement;


public class KatanasController : MonoBehaviour
{
    public Animator katanasAnimator;
    private CapsuleCollider attackCollider;
    public LayerMask targetLayer; // Слой, на котором находятся цели
    public float attackRadius;
    private Health enemyHealth;
    public float damage;
    public float altdamage;
    public Grappling grapplingScript; // Ссылка на компонент Grappling

    public bool isEnemyInRange; // Булевая переменная для проверки нахождения врага в зоне коллайдера
    public bool canAltAttack;
    public float attackCooldown = 1.0f; // Время между атаками (в секундах)
    private float nextAttackTime = 0f;  // Время, когда можно будет сделать следующую атаку
    private float nextAltAttackTime = 0f;  // Время, когда можно будет сделать следующую атаку

    public GameObject bloodEffectPrefab; // Префаб визуального эффекта крови
    public float bloodEffectDuration = 2.0f; // Продолжительность отображения эффекта

    public float altAttackDuration = 5.0f; // Продолжительность альтернативной атаки после использования крюка-кошки
    private float altAttackEndTime = 0f;   // Время окончания альтернативной атаки

    public float jumpforce;
    public float backstepForce = 5.0f; // Сила отталкивания назад
    public Rigidbody rb; // Ссылка на компонент Rigidbody
    public VisualEffect SlashVFX;
    public RectTransform KatanasPopUpHolder;
    


    public VisualEffect altSlashVFX;
    public VisualEffect altSlashVFX2;
    public PlayerMovementAdvanced pm;
    public DamageNumber katanasPopUp;


    // Start is called before the first frame update
    void Start()
    {
        SlashVFX.Stop();
      

        attackCollider = GetComponent<CapsuleCollider>();
        attackCollider.isTrigger = true;

        if (rb == null)
        {
            Debug.LogError("Rigidbody не найден на объекте.");
        }

        // Поиск компонента Grappling в сцене
        grapplingScript = FindObjectOfType<Grappling>();

        if (grapplingScript == null)
        {
            Debug.LogWarning("Компонент Grappling не найден на сцене.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            if ((Input.GetMouseButtonDown(0) || (grapplingScript != null && pm.activeGrapple && isEnemyInRange)))
            {
                DamageNumber damageNumber = katanasPopUp.SpawnGUI(KatanasPopUpHolder, Vector2.zero);


                Attack();
                isEnemyInRange = false;
                SlashVFX.Play(); // Запускаем эффект
            }
        }

        if (Time.time >= nextAltAttackTime)
        {
            if (Input.GetMouseButtonDown(1) )
            {
                AltAttack();
            }
        }
    }

   

 public void Attack()
{
    Debug.Log("Attack");

    // Устанавливаем время для следующей атаки
    nextAttackTime = Time.time + attackCooldown;

    // Анимация атаки
    if (katanasAnimator != null)
    {
        katanasAnimator.SetTrigger("Attack");
    }
    else
    {
        Debug.LogWarning("katanasAnimator не назначен!");
    }

    // Логика для нанесения урона врагам в зоне
    Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, targetLayer);

    foreach (Collider hit in hits)
    {
        Debug.Log("Hit target: " + hit.gameObject.name);

        // Проверяем, реализует ли цель интерфейс IDamageable
        IDamageable target = hit.GetComponent<IDamageable>();

        if (target != null)
        {
            // Наносим урон через интерфейс
            target.TakeDamage(damage, DamageHandler.AttackType.Katanas);

            // Спавним эффект крови в месте удара
            SpawnBloodEffect(hit.transform.position);
        }
        else
        {
            Debug.LogWarning("IDamageable не найден у цели: " + hit.gameObject.name);
        }

        // Запуск визуального эффекта удара
        if (SlashVFX != null)
        {
            SlashVFX.Play();
            Debug.Log("VFX запущен в позиции: " + SlashVFX.transform.position);
        }
        else
        {
            Debug.LogWarning("SlashVFX не назначен!");
        }
    }
}

    public void AltAttack()
    {
        pm.DisableMovement(altAttackDuration);

        // Используем Physics.OverlapSphere для поиска целей
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, targetLayer);

        Debug.Log("Альтернативная атака");

        // Запускаем анимацию для альтернативной атаки
        katanasAnimator.SetTrigger("AltAttack");

        // Логика для нанесения урона врагам в зоне
        foreach (Collider hit in hits)
        {
            Debug.Log("Hit target (AltAttack): " + hit.gameObject.name);

            enemyHealth = hit.GetComponent<Health>();

            if (enemyHealth != null)
            {
                // Наносим урон врагу
                enemyHealth.TakeDamage(altdamage, DamageHandler.AttackType.Katanas);

                // Спавним эффект крови в месте удара
                SpawnBloodEffect(hit.transform.position);
            }
            else
            {
                Debug.LogWarning("Health не найден у цели (AltAttack): " + hit.gameObject.name);
            }

            // Включаем визуальный эффект удара.
            if (altSlashVFX != null && altSlashVFX2 != null)
            {
                altSlashVFX.Play(); // Запускаем эффект
                altSlashVFX2.Play(); // Запускаем эффект
                Debug.Log("VFX запущен в позиции: " + SlashVFX.transform.position);
            }
            else
            {
                Debug.LogWarning("SlashVFX не назначен!");
            }

            grapplingScript.StopGrapple();
        }

        // Направление отталкивания назад, игнорируя вертикальную составляющую
        Vector3 backstepDirection = new Vector3(-Camera.main.transform.forward.x, 0, -Camera.main.transform.forward.z).normalized;

        // Применяем силу для отталкивания назад
        rb.AddForce(backstepDirection * backstepForce, ForceMode.Impulse);

        // Устанавливаем время для следующей атаки
        nextAttackTime = Time.time + attackCooldown;

        canAltAttack = false; // Завершаем возможность альтернативной атаки
    }

   

    // Спавн эффекта крови
    private void SpawnBloodEffect(Vector3 position)
    {
        // Спавним префаб крови в указанной позиции
        GameObject bloodEffect = Instantiate(bloodEffectPrefab, position, Quaternion.identity);

        // Удаляем эффект через заданное время
        Destroy(bloodEffect, bloodEffectDuration);
    }

    // Враг входит в область действия коллайдера
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) != 0) // Проверяем, принадлежит ли объект целевому слою
        {
            isEnemyInRange = true;
            Debug.Log("Враг вошел в зону атаки: " + other.gameObject.name);
        }
    }

    // Враг выходит из области действия коллайдера
    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) != 0) // Проверяем, принадлежит ли объект целевому слою
        {
            isEnemyInRange = false;
            Debug.Log("Враг вышел из зоны атаки: " + other.gameObject.name);
        }
    }

    // Метод, вызываемый при использовании крюка-кошки
    public void OnGrapplingUsed()
    {
        // Начало альтернативной атаки
        altAttackEndTime = Time.time + altAttackDuration;
        canAltAttack = true;
        Debug.Log("Альтернативная атака активирована на " + altAttackDuration + " секунд.");
    }

  
}
