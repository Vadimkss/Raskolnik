using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;
using System.Collections;
using UnityEngine.AI;
using NTC.MonoCache;
using FMOD.Studio;
using RenownedGames.AITree;

namespace Movement
{
    /// <summary>
    /// Handles the slam ability for the player, allowing them to perform ground-pound attacks
    /// that affect nearby enemies and create visual effects.
    /// </summary>
    public class Slam : MonoCache
    {
        #region Variables
        [Header("Input Settings")]
        [SerializeField] private KeyCode slamKey = KeyCode.LeftControl;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode dashKey = KeyCode.E;
       
        [Header("Basic Slam Settings")]
        [SerializeField] private float slamForce = 500f;
        [SerializeField] private float maxSlamDuration = 5f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float fallAcceleration = -9.81f;
        [SerializeField] private float maxFallSpeed = -50f;

        [Header("Slam Radius Settings")]
        [SerializeField] private float minSlamRadius = 3f;
        [SerializeField] private float maxSlamRadius = 10f;

        [Header("Upward Force Settings")]
        [SerializeField] private float minUpwardForce = 500f;
        [SerializeField] private float maxUpwardForce = 1500f;

        [Header("Speed Boost Settings")]
        [SerializeField] private float slamSpeedBoost = 1.5f;
        [SerializeField] private float slamSpeedBoostDuration = 2f;

        [Header("After-Slam Jump Settings")]
        [SerializeField] private float minAfterSlamJump = 0f;
        [SerializeField] private float maxAfterSlamJump = 100f;
       
        [Header("Visual Effects")]
        [SerializeField] private VisualEffect slamEffect;
        [SerializeField] private GameObject additionalObjectPrefab;
        [SerializeField] private float minAdditionalObjectScale = 0.5f;
        [SerializeField] private float maxAdditionalObjectScale = 3f;
        [SerializeField] private float crackScaleDuration = 0.5f;
        [SerializeField] private float disappearDelay = 1f;
        [SerializeField] private float yOffset = 0.1f;
        [SerializeField] private Ease crackScaleEase = Ease.OutBounce;
        [SerializeField] private float destroyDelay = 2f;
      
        private Rigidbody rb;
        private PlayerMovementAdvanced playerMovement;
        private bool isSlamming;
        private float currentFallSpeed;
        private EventInstance slamInstance;
        private float lastSlamRadius;
        private readonly float additionalObjectYOffset = 0.1f;
        #endregion

        #region Monos
        private void Start()
        {
            InitializeComponents();
        }

        #endregion

        #region Updates
        protected override void Run()
        {
            HandleSlamInput();
            HandleCancelInput();
            CheckGroundedState();
        }

        protected override void FixedRun()
        {
            UpdateSlamPhysics();
        }
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            rb = GetComponent<Rigidbody>();
            playerMovement = GetComponent<PlayerMovementAdvanced>();
            slamEffect.enabled = false;
        }
        #endregion

        #region Input Handling
        private void HandleSlamInput()
        {
            if (Input.GetKeyDown(slamKey) && !playerMovement.grounded && !isSlamming)
            {
                StartSlam();
            }
        }

        private void HandleCancelInput()
        {
            if ((Input.GetKeyDown(jumpKey) || Input.GetKeyDown(dashKey)) && isSlamming)
            {
                EndSlam();
                if (Input.GetKeyDown(jumpKey))
                {
                    playerMovement.Jump();
                }
            }
        }

        private void CheckGroundedState()
        {
            if (isSlamming && playerMovement.grounded)
            {
                EndSlam();
            }
        }
        #endregion

        #region Slam Mechanics
        private void StartSlam()
        {
            isSlamming = true;
            rb.velocity = new Vector3(rb.velocity.x, -slamForce, rb.velocity.z);
            slamEffect.enabled = true;
            slamInstance = AudioManager.instance.PlayTimeline(FMODEvents.Instance.SlamFall, transform.position);
        }

        private void UpdateSlamPhysics()
        {
            if (!isSlamming) return;

            float speedMultiplier = playerMovement.moveSpeed / 1.2f;
            currentFallSpeed += fallAcceleration * speedMultiplier * Time.fixedDeltaTime;
            currentFallSpeed = Mathf.Max(currentFallSpeed, maxFallSpeed * playerMovement.moveSpeed * 2f);
            rb.velocity = new Vector3(rb.velocity.x, currentFallSpeed, rb.velocity.z);
        }

        private void EndSlam()
        {
            ResetMovementState();
            float slamFactor = CalculateSlamFactor();
            ApplySlamEffects(slamFactor);
            AudioManager.instance.StopTimeline(slamInstance);
        }

        private void ResetMovementState()
        {
            playerMovement.moveSpeed = playerMovement.baseMoveSpeed;
            isSlamming = false;
            slamEffect.enabled = false;
        }

        private float CalculateSlamFactor()
        {
            float speedFactor = currentFallSpeed / (maxFallSpeed * playerMovement.moveSpeed / 10f);
            return Mathf.Clamp01(speedFactor);
        }
        #endregion

        #region Effect Application
        private void ApplySlamEffects(float slamFactor)
        {
            float currentAfterSlamJump = Mathf.Lerp(minAfterSlamJump, maxAfterSlamJump, slamFactor / 1.5f);
            float currentSlamRadius = Mathf.Lerp(minSlamRadius, maxSlamRadius, slamFactor);
            float currentUpwardForce = Mathf.Lerp(minUpwardForce, maxUpwardForce, slamFactor);
            Vector3 objectScale = Vector3.one * Mathf.Lerp(minAdditionalObjectScale, maxAdditionalObjectScale, slamFactor);

            ApplyAreaEffect(currentSlamRadius, currentUpwardForce);
            ApplyPlayerEffects(slamFactor, currentAfterSlamJump);
            SpawnVisualEffects(objectScale);
        }

        private void ApplyAreaEffect(float radius, float upwardForce)
        {
            lastSlamRadius = radius;
            var affectedColliders = Physics.OverlapSphere(transform.position, radius, enemyLayer);

            foreach (var collider in affectedColliders)
            {
                if (TryGetEnemyComponents(collider, out Rigidbody enemyRb, out NavMeshAgent agent, out BehaviourRunner runner))
                {
                    ApplyEnemyEffects(enemyRb, agent, runner, upwardForce);
                }
            }
        }

        private bool TryGetEnemyComponents(Collider collider, out Rigidbody rb, out NavMeshAgent agent, out BehaviourRunner runner)
        {
            rb = collider.GetComponent<Rigidbody>();
            agent = collider.GetComponent<NavMeshAgent>();
            runner = collider.GetComponent<BehaviourRunner>();
            return rb != null;
        }

        private void ApplyEnemyEffects(Rigidbody enemyRb, NavMeshAgent agent, BehaviourRunner runner, float upwardForce)
        {
            if (agent != null)
            {
                agent.enabled = false;
                runner.enabled = false;
                StartCoroutine(EnableNavMeshAgent(agent, runner, 1f));
            }

            enemyRb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
        }

        private void ApplyPlayerEffects(float slamFactor, float jumpForce)
        {
            currentFallSpeed = 0;

            if (slamFactor > 0.2f)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);

                if (slamFactor > 0.4f)
                {
                    playerMovement.ModifySpeed(slamSpeedBoost, slamSpeedBoostDuration);
                }
            }
        }

        private void SpawnVisualEffects(Vector3 objectScale)
        {
            if (additionalObjectPrefab != null)
            {
                Vector3 spawnPosition = transform.position + Vector3.up * additionalObjectYOffset;
                GameObject visualEffect = Instantiate(additionalObjectPrefab, spawnPosition, Quaternion.identity);
                visualEffect.transform.localScale = objectScale;
                Destroy(visualEffect, destroyDelay);
            }
        }
        #endregion

        #region Coroutines
        private IEnumerator EnableNavMeshAgent(NavMeshAgent agent, BehaviourRunner runner, float delay)
        {
            yield return new WaitForSeconds(delay);
            agent.enabled = true;
            runner.enabled = true;
        }
        #endregion

        #region Gizmos
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, lastSlamRadius);
        }
        #endregion
    }
}