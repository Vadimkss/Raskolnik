using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.UI;
using NTC.MonoCache;

namespace Movement
{
    /// <summary>
    /// Handles the dash ability for the player, allowing quick directional movement bursts
    /// with stamina management and visual effects.
    /// </summary>
    public class Dashing : MonoCache
    {
        #region Variables
        [Header("Component References")]
        [SerializeField] private Transform orientation;
        [SerializeField] private Transform playerCam;
        [SerializeField] private PlayerCam cam;
        [SerializeField] private VisualEffect dashEffect;
        [SerializeField] private Image staminaBar;

        private Rigidbody rb;
        private PlayerMovementAdvanced playerMovement;
        private Grappling grappling;
     
        [Header("Dash Settings")]
        [SerializeField] private float dashForce = 20f;
        [SerializeField] private float dashUpwardForce = 2f;
        [SerializeField] private float maxDashYSpeed = 10f;
        [SerializeField] private float dashDuration = 0.25f;
        [SerializeField] private float dashSpeedBoost = 1.5f;
        [SerializeField] private float dashSpeedBoostDuration = 1f;
        [SerializeField] private float dashFov = 90f;
        [SerializeField] private float normalFov = 70f;
      
        [Header("Dash Configuration")]
        [SerializeField] private bool useCameraForward = true;
        [SerializeField] private bool allowAllDirections = true;
        [SerializeField] private bool disableGravity = false;
        [SerializeField] private bool resetVelocity = true;
        [SerializeField] private KeyCode dashKey = KeyCode.E;
      
        [Header("Cooldown")]
        [SerializeField] private float dashCooldown = 1f;
        private float dashCooldownTimer;
        private bool dashReset = true;
       
        [Header("Stamina Settings")]
        [SerializeField] private float currentStamina;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaReduction = 5f;
        [SerializeField] private float staminaCost = 100f;
        [SerializeField] private float airborneStaminaMultiplier = 0.5f;
        [SerializeField] private float lowStaminaThreshold = 60f;
        [SerializeField] private float lowStaminaRecoveryMultiplier = 1.5f;
        #endregion

        private Vector3 delayedForceToApply;

        #region Monos
        private void Start()
        {
            InitializeComponents();
        }
        #endregion

        #region Updates
        protected override void Run()
        {
            HandleDashInput();
        }

        protected override void FixedRun()
        {
            UpdateCooldown();
            UpdateStamina();
            UpdateUI();
        }
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            grappling = GetComponent<Grappling>();
            rb = GetComponent<Rigidbody>();
            playerMovement = GetComponent<PlayerMovementAdvanced>();
            dashEffect.enabled = false;
            currentStamina = maxStamina;
        }
        #endregion

        #region Input Handling
        private void HandleDashInput()
        {
            if (Input.GetKeyDown(dashKey) && HasEnoughStamina())
            {
                InitiateDash();
            }
        }

        private bool HasEnoughStamina()
        {
            return currentStamina >= staminaCost;
        }
        #endregion

        #region Dash Mechanics
        private void InitiateDash()
        {
            if (dashCooldownTimer > 0) return;

            SetupDashParameters();
            ApplyDashForce();
            ApplyDashEffects();
            ConsumeStamina();
        }

        private void SetupDashParameters()
        {
            dashCooldownTimer = dashCooldown;
            playerMovement.dashing = true;
            playerMovement.maxYSpeed = maxDashYSpeed;
            playerMovement.desiredMoveSpeed = playerMovement.baseMoveSpeed;
            playerMovement.groundDrag = 0f;
        }

        private void ApplyDashForce()
        {
            Transform forwardTransform = useCameraForward ? playerCam : orientation;
            Vector3 direction = CalculateDashDirection(forwardTransform);
            Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardForce;

            if (disableGravity)
            {
                rb.useGravity = false;
            }

            delayedForceToApply = forceToApply;
            Invoke(nameof(ApplyDelayedForce), 0.025f);
            Invoke(nameof(ResetDashState), dashDuration);
        }

        private Vector3 CalculateDashDirection(Transform forwardTransform)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");
            Vector3 direction = forwardTransform.forward * verticalInput + forwardTransform.right * horizontalInput;

            // Remove downward movement
            if (direction.y < 0)
            {
                direction.y = 0;
            }

            // Use forward direction if no input
            if (verticalInput == 0 && horizontalInput == 0)
            {
                direction = forwardTransform.forward;
            }

            // Remove vertical component when looking down
            if (Vector3.Dot(forwardTransform.up, Vector3.down) > 0)
            {
                direction.y = 0;
            }

            return direction.normalized;
        }

        private void ApplyDelayedForce()
        {
            if (resetVelocity)
            {
                rb.velocity = Vector3.zero;
            }

            rb.AddForce(delayedForceToApply, ForceMode.Impulse);
        }
        #endregion

        #region Effects
        private void ApplyDashEffects()
        {
            cam.DoFov(dashFov);
            dashEffect.enabled = true;
            DisableGrappleIfActive();
            PlayDashSound();
        }

        private void DisableGrappleIfActive()
        {
            if (playerMovement.activeGrapple)
            {
                playerMovement.activeGrapple = false;
            }
        }

        private void PlayDashSound()
        {
            AudioManager.instance.PlayOneShot(FMODEvents.Instance.DashSFX, transform.position);
        }
        #endregion

        #region State Management
        private void ResetDashState()
        {
            ResetMovementParameters();
            ResetVisualEffects();
            EnablePlayerControl();
        }

        private void ResetMovementParameters()
        {
            rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
            playerMovement.groundDrag = 5f;
            playerMovement.dashing = false;
            playerMovement.maxYSpeed = 0;

            if (disableGravity)
            {
                rb.useGravity = true;
            }
        }

        private void ResetVisualEffects()
        {
            cam.DoFov(normalFov);
            dashEffect.enabled = false;
            StartCoroutine(DeactivateEffectWithDelay());
        }

        private void EnablePlayerControl()
        {
            dashReset = true;
            playerMovement.canMove = true;
        }
        #endregion

        #region Stamina Management
        private void UpdateStamina()
        {
            if (dashReset)
            {
                RegenerateStamina();
            }
        }

        private void RegenerateStamina()
        {
            if (currentStamina >= maxStamina) return;

            float recoveryMultiplier = CalculateStaminaRecoveryMultiplier();
            currentStamina += staminaReduction * recoveryMultiplier;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }

        private float CalculateStaminaRecoveryMultiplier()
        {
            float multiplier = 1f;

            if (!playerMovement.grounded)
            {
                multiplier *= airborneStaminaMultiplier;
            }

            if (currentStamina < lowStaminaThreshold)
            {
                multiplier *= lowStaminaRecoveryMultiplier;
            }

            return multiplier;
        }

        private void ConsumeStamina()
        {
            currentStamina -= staminaCost;
            dashReset = false;
        }
        #endregion

        #region UI Updates
        private void UpdateUI()
        {
            staminaBar.fillAmount = currentStamina / maxStamina;
        }

        private void UpdateCooldown()
        {
            if (dashCooldownTimer > 0)
            {
                dashCooldownTimer -= Time.deltaTime;
            }
        }
        #endregion

        #region Coroutines
        private IEnumerator DeactivateEffectWithDelay()
        {
            yield return new WaitForSeconds(1f);
            dashEffect.enabled = false;
        }
        #endregion
    }
}