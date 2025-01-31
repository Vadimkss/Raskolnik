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

    public float minSlamRadius = 3f;
    public float maxSlamRadius = 10f;
    public float minUpwardForce = 500f;
    public float maxUpwardForce = 1500f;
    public float minCrackScale = 0.5f;
    public float maxCrackScale = 3f;
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
    public Animator cameraShake;
  
    public float yOffset = 0.1f;
    public Ease crackScaleEase = Ease.OutBounce;
    public float destroyDelay = 2f;

    public GameObject additionalObjectPrefab;
    public float additionalObjectYOffset = 0.1f;

    private EventInstance slamInstance;

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
            currentFallSpeed += fallAcceleration * Time.fixedDeltaTime;

            // Ограничиваем скорость падения
            currentFallSpeed = Mathf.Max(currentFallSpeed, maxFallSpeed);

            // Обновляем вертикальную скорость Rigidbody
            rb.velocity = new Vector3(rb.velocity.x, currentFallSpeed, rb.velocity.z);
        }
    }

    private void EndSlam()
    {
        isSlamming = false;
        slamEffect.enabled = false;
      

        // Рассчитываем процент от максимальной скорости падения
        float slamFactor = Mathf.Clamp01(currentFallSpeed / maxFallSpeed);

        float currentSlamRadius = Mathf.Lerp(minSlamRadius, maxSlamRadius, slamFactor);
        float currentUpwardForce = Mathf.Lerp(minUpwardForce, maxUpwardForce, slamFactor);
        Vector3 currentCrackScale = Vector3.one * Mathf.Lerp(minCrackScale, maxCrackScale, slamFactor);
        Vector3 currentAdditionalObjectScale = Vector3.one * Mathf.Lerp(minAdditionalObjectScale, maxAdditionalObjectScale, slamFactor);

        ApplySlamEffects(currentSlamRadius, currentUpwardForce, currentCrackScale, currentAdditionalObjectScale);

        currentFallSpeed = 0;

        AudioManager.instance.StopTimeline(slamInstance);
        pm.moveSpeed = pm.baseMoveSpeed;

    }

    private void ApplySlamEffects(float radius, float upwardForce, Vector3 crackScale, Vector3 additionalObjectScale)
    {
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

        pm.ModifySpeed(pm.slamSpeedBoost, pm.slamSpeedBoostDuration);
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
}
