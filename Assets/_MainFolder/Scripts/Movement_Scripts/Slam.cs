using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;
using System.Collections;
using UnityEngine.AI;
using NTC.MonoCache;
using FMOD.Studio;
using RenownedGames.AITree;

public class Slam : MonoCache
{
    public KeyCode slamKey = KeyCode.LeftControl;
    public float slamForce = 500f;
    public float baseSlamRadius = 5f;
    public float baseSlamUpwardForce = 1000f;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode dashKey = KeyCode.E;
    public float maxSlamDuration = 5f;
    public LayerMask enemyLayer;

    public float slamSpeedBoost = 1.5f; // Значение ускорения при слэме
    public float slamSpeedBoostDuration = 2f; // Длительность ускорения при слэме

    public float minSlamRadius = 3f;
    public float maxSlamRadius = 10f;
    public float minUpwardForce = 500f;
    public float maxUpwardForce = 1500f;
 
    public float minAdditionalObjectScale = 0.5f;
    public float maxAdditionalObjectScale = 3f;
    public float crackScaleDuration = 0.5f;
    public float disappearDelay = 1f;

    private Rigidbody rb;
    private PlayerMovementAdvanced pm;
    private bool isSlamming = false;
    private float currentFallSpeed;
    public float fallAcceleration = -9.81f;
    public float maxFallSpeed = -50f;

    public VisualEffect slamEffect;
  
  
    public float yOffset = 0.1f;
    public Ease crackScaleEase = Ease.OutBounce;
    public float destroyDelay = 2f;

    public GameObject additionalObjectPrefab;
    private float additionalObjectYOffset = 0.1f;

    private EventInstance slamInstance;
    private float lastSlamRadius;

    private float minAfterSlamJump = 0;
    private float maxAfterSlamJump = 100;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
        slamEffect.enabled = false;
    }

    protected override void Run()
    {
        if (Input.GetKeyDown(slamKey) && !pm.grounded && !isSlamming)
        {
            StartSlam();
        }

        if ((Input.GetKeyDown(jumpKey) || Input.GetKeyDown(dashKey)) && isSlamming)
        {
            EndSlam();
            if (Input.GetKeyDown(jumpKey))
            {
                pm.Jump();
            }
        }

        if (isSlamming && pm.grounded)
        {
            EndSlam();
        }
    }

    private void StartSlam()
    {
        isSlamming = true;
        rb.velocity = new Vector3(rb.velocity.x, -slamForce, rb.velocity.z);
        slamEffect.enabled = true;

        slamInstance = AudioManager.instance.PlayTimeline(FMODEvents.Instance.SlamFall, this.transform.position);

      
    }

    protected override void FixedRun()
    {
        if (isSlamming)
        {
            // Увеличиваем скорость падения с учетом ускорения
            currentFallSpeed += fallAcceleration * pm.moveSpeed * Time.fixedDeltaTime;

            // Ограничиваем скорость падения
            currentFallSpeed = Mathf.Max(currentFallSpeed, maxFallSpeed * pm.moveSpeed * 2f);

            // Обновляем вертикальную скорость Rigidbody
            rb.velocity = new Vector3(rb.velocity.x, currentFallSpeed, rb.velocity.z);
        }
    }


    private void EndSlam()
    {
        pm.moveSpeed = pm.baseMoveSpeed;

        isSlamming = false;
        slamEffect.enabled = false;

        Debug.Log(currentFallSpeed);
      
        // Рассчитываем процент от максимальной скорости падения
        float slamFactor = Mathf.Clamp01(currentFallSpeed / (maxFallSpeed * pm.moveSpeed/10f));
        float currentAfterSlamJump = Mathf.Lerp(minAfterSlamJump, maxAfterSlamJump, slamFactor);
        float currentSlamRadius = Mathf.Lerp(minSlamRadius, maxSlamRadius, slamFactor);
        float currentUpwardForce = Mathf.Lerp(minUpwardForce, maxUpwardForce, slamFactor);
      
        Vector3 currentAdditionalObjectScale = Vector3.one * Mathf.Lerp(minAdditionalObjectScale, maxAdditionalObjectScale, slamFactor);

        ApplySlamEffects(currentSlamRadius, currentUpwardForce, currentAdditionalObjectScale);
    
        currentFallSpeed = 0;

        if (slamFactor > 0.5f)
        {
            rb.velocity = new Vector3(rb.velocity.x, currentAfterSlamJump, rb.velocity.z);
        }

        AudioManager.instance.StopTimeline(slamInstance);

        Debug.Log("Slam Factor: " + slamFactor + ", After Slam Jump: " + currentAfterSlamJump);

    }
   

   

    private void ApplySlamEffects(float radius, float upwardForce, Vector3 additionalObjectScale)
    {

        lastSlamRadius = radius; // Сохраняем последний использованный радиус
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, enemyLayer);

        foreach (Collider collider in colliders)
        {
            Rigidbody enemyRb = collider.GetComponent<Rigidbody>();
            NavMeshAgent agent = collider.GetComponent<NavMeshAgent>();
            BehaviourRunner BRunner = collider.GetComponent<BehaviourRunner>();

            if (enemyRb != null)
            {
                if (agent != null)
                {
                    agent.enabled = false;
                    BRunner.enabled = false;
                }

                enemyRb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);

                if (agent != null && BRunner != null)
                {
                    StartCoroutine(EnableNavMeshAgent(agent, BRunner, 1f));

                  

                }
            }
        }

      

        if (additionalObjectPrefab != null)
        {
            CreateAdditionalObject(additionalObjectScale);
        }

        pm.ModifySpeed(slamSpeedBoost, slamSpeedBoostDuration);
   }

    private void CreateAdditionalObject(Vector3 objectScale)
    {
        Vector3 additionalSpawnPosition = transform.position + Vector3.up * additionalObjectYOffset;
        GameObject additionalObjectInstance = Instantiate(additionalObjectPrefab, additionalSpawnPosition, Quaternion.identity);
        additionalObjectInstance.transform.localScale = objectScale;
        Destroy(additionalObjectInstance, destroyDelay);
    }

    private IEnumerator EnableNavMeshAgent(NavMeshAgent agent, BehaviourRunner Brunner, float delay)
    {
        yield return new WaitForSeconds(delay);
        agent.enabled = true;
        Brunner.enabled = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lastSlamRadius);
    }
}
