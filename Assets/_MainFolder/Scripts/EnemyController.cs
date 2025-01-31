using UnityEngine;
using RenownedGames.AITree;
using DG.Tweening;

public class EnemyController : MonoBehaviour
{
   

    private GameObject player;
    private Transform playerTransform;
    private bool canMelee;
    private Animator animator;
    private Vector3 randomRotation;


    [SerializeField] private float chaseRange = 5f; // Радиус сферы
    
    [SerializeField] private float meleeRange = 5f; // Радиус сферы

    [SerializeField] private BehaviourTree behaviourTree;
  
    [SerializeField] private Blackboard blackboard;

    [SerializeField] public float RS_StepLenght;
    
    [SerializeField] public float RS_StepDuration;

    [SerializeField] public Ease animationEase;

    [SerializeField] private LayerMask playerLayer; // Слой, на котором находится игрок
    void Start()
    {
        if (animator != null)
        {
            animator = GetComponent<Animator>();
        }

        else animator = GetComponentInChildren<Animator>();

        // Получаем дерево поведения и Blackboard
        this.behaviourTree = this.GetComponent<BehaviourRunner>().GetBehaviourTree();
        this.blackboard = this.behaviourTree.GetBlackboard();

        // Ищем игрока по тегу и получаем его Transform
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;

            // Записываем Transform игрока в Blackboard
            if (blackboard.TryGetKey("Player", out TransformKey playerT))
            {
                playerT.SetValue(playerTransform);
            }
        }
        else
        {
            Debug.LogError("Игрок с тегом 'Player' не найден!");
        }
    }

    void Update()
    {
        ChaseCheck();
        MeleeCheck(); // Добавляем проверку на ближний бой
    }

    public void ChaseCheck()
    {
        Collider[] meleeHits = Physics.OverlapSphere(transform.position, meleeRange, playerLayer);
        Collider[] chaseHits = Physics.OverlapSphere(transform.position, chaseRange, playerLayer);

        if (meleeHits.Length > 0) // Приоритет ближнего боя
        {
            OutChase();
            InMeleeRange();
        
        }
        else if (chaseHits.Length > 0) // Если игрок в зоне преследования
        {
            OutMeleeRange();
            InChase();
         
        }
        else // Игрок вне зоны
        {
            OutMeleeRange();
            OutChase();
        }
    }

    public void InChase()
    {
        // Устанавливаем значение ключа в Blackboard
        if (blackboard.TryGetKey("IsInShaseRange", out BoolKey chasekey))
        {
            chasekey.SetValue(true);
        }

      
        if (blackboard.TryGetKey("PlayerPosition", out Vector3Key playerpos))
        {
            playerpos.SetValue(playerTransform.position);
        }

       
    }

    public void OutChase()
    {
        // Сбрасываем значение ключа в Blackboard
        if (blackboard.TryGetKey("IsInShaseRange", out BoolKey chasekey))
        {
            chasekey.SetValue(false);
        }
    }


    public void MeleeCheck()
    {
        Collider[] meleeHits = Physics.OverlapSphere(transform.position, meleeRange, playerLayer);

        if (meleeHits.Length > 0) // Если игрок найден
        {
            OutChase();
            InMeleeRange(); // Может ударить 


        }
        else
        {
            OutMeleeRange(); // Не может ударить
        }

    }

    public void InMeleeRange()
    {
        // Устанавливаем значение ключа в Blackboard
        if (blackboard.TryGetKey("IsInMeleeRange", out BoolKey meleekey))
        {
            meleekey.SetValue(true);
            canMelee = true;
        }

      

        Debug.Log("В области атаки");

    }

    public void OutMeleeRange()
    {
        // Сбрасываем значение ключа в Blackboard
        if (blackboard.TryGetKey("IsInMeleeRange", out BoolKey meleekey))
        {
            meleekey.SetValue(false);
            canMelee = false;
        }

     
    }

   
    public void RS_Rotate()
    {
      

        if (blackboard.TryGetKey("CanRotate", out BoolKey canRotateKey))
        {
            canRotateKey.SetValue(true);
        }

    }


    public void GetRanPosRotation()
    {
        Vector3 targetPoint = Vector3.zero; // Начальное значение цели
        Vector3 selfPosition = transform.position; // Текущая позиция объекта

        // Получаем случайную позицию
        if (blackboard.TryGetKey("RandomPosition", out Vector3Key randomPosition))
        {
            targetPoint = randomPosition.GetValue();
        }

        // Вычисляем направление к случайной позиции
        Vector3 directionOfRanPos = (targetPoint - selfPosition).normalized;

        // Преобразуем направление в вращение
        Quaternion targetRotation = Quaternion.LookRotation(directionOfRanPos);

        // Корректируем вращение, если модель повёрнута боком
        Quaternion correctedRotation = targetRotation * Quaternion.Euler(0, 90, 0); // Поворот на 90 градусов вокруг оси Y (пример)

        // Применяем вычисленное вращение к blackboard
        if (blackboard.TryGetKey("RotationOfRanPos", out Vector3Key rotationOfRanPos))
        {
            rotationOfRanPos.SetValue(correctedRotation.eulerAngles);
        }

        // Опционально: Если нужно, чтобы объект сразу поворачивался к цели
        transform.rotation = correctedRotation;
    }

    
    public void RS_StepForward()
    {
        // Поворачиваем вектор движения на -90 градусов относительно текущей ориентации объекта
        Vector3 adjustedDirection = Quaternion.Euler(0, -90, 0) * transform.forward;
        Vector3 targetPosition = transform.position + adjustedDirection * RS_StepLenght;

        transform.DOMove(targetPosition, RS_StepDuration)
          .SetEase(animationEase)
            .OnComplete(() => Debug.Log("Шаг завершён"));
    }

    public void ResetTrigger(string triggerName)
    {
        animator.ResetTrigger(triggerName);

    }


 
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
