using UnityEngine;
using FMOD.Studio;
using DG.Tweening;
using NTC.MonoCache;

namespace Movement
{
    /// <summary>
    /// Handles grappling hook mechanics for the player character.
    /// </summary>
    public class Grappling : MonoCache
    {
        #region Component References
        [Header("Component References")]
        [SerializeField] private Camera cam;
        [SerializeField] public Transform gunTip;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Animator grapplingGunAnimator;
        [SerializeField] private KatanasController katanasController;
        #endregion

        #region Grappling Settings
        [Header("Grappling Settings")]
        [SerializeField] private float maxGrappleDistance = 30f;
        [SerializeField] private float grappleDelayTime = 0.2f;
        [SerializeField] private float overshootYAxis = 2f;
        [SerializeField] private float raycastRadius = 0.5f;
        [SerializeField] private float grappleSpeed = 20f;
        [SerializeField] private float grappleSpeedBoost = 1.2f;
        [SerializeField] private float grappleSpeedBoostDuration = 2f;
        [SerializeField] private LayerMask whatIsGrappleable;
        #endregion

        #region Cooldown Settings
        [Header("Cooldown")]
        [SerializeField] private float grapplingCooldown = 1f;
        private float grapplingCooldownTimer;
        #endregion

        #region Input Settings
        [Header("Input")]
        [SerializeField] private KeyCode grappleKey = KeyCode.Mouse1;
        #endregion

        #region Private Fields
        private PlayerMovementAdvanced pm;
        private EventInstance ziplineInstance;
        private Vector3 grapplePoint;
        
       
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            InitializeComponents();
        }

        protected override void Run()
        {
            HandleGrapplingInput();
            UpdateCooldown();
            CheckForGrapplingInterruption();
        }

        protected override void LateRun()
        {
            UpdateGrapplingRope();
        }
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            pm = GetComponent<PlayerMovementAdvanced>();
            ziplineInstance = AudioManager.instance.CreateInstance(FMODEvents.Instance.Zipline);
        }
        #endregion

        #region Grappling Logic
        private void HandleGrapplingInput()
        {
            if (Input.GetKeyDown(grappleKey) && !pm.dashing)
            {
                StartGrapple();
            }
        }

        private void StartGrapple()
        {
            if (grapplingCooldownTimer > 0) return;

            pm.activeGrapple = true;
            grapplingGunAnimator.SetTrigger("GrappleOn");
            
            if (TryFindGrapplePoint(out Vector3 hitPoint))
            {
                grapplePoint = hitPoint;
                Invoke(nameof(ExecuteGrapple), grappleDelayTime);
            }
            else
            {
                HandleFailedGrapple();
            }

            InitializeGrappleVisuals();
            PlayGrapplingSound();
        }

        private bool TryFindGrapplePoint(out Vector3 hitPoint)
        {
            RaycastHit hit;
            bool didHit = Physics.SphereCast(
                cam.transform.position,
                raycastRadius,
                cam.transform.forward,
                out hit,
                maxGrappleDistance,
                whatIsGrappleable
            );

            hitPoint = didHit ? hit.point : Vector3.zero;
            return didHit;
        }

        private void HandleFailedGrapple()
        {
            grapplePoint = cam.transform.position + cam.transform.forward * maxGrappleDistance;
            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        private void InitializeGrappleVisuals()
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(1, grapplePoint);
            pm.jumpsRemaining = pm.maxJumpCount;
        }

        private void ExecuteGrapple()
        {
            pm.freeze = false;

            float highestPointOnArc = CalculateGrappleArc();
            pm.JumpToPosition(grapplePoint, grappleSpeed);

            Invoke(nameof(StopGrapple), 1f);
            AudioManager.instance.StopInstance(ziplineInstance);
        }

        private float CalculateGrappleArc()
        {
            Vector3 lowestPoint = transform.position + Vector3.down;
            float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
            return grapplePointRelativeYPos < 0 ? overshootYAxis : grapplePointRelativeYPos + overshootYAxis;
        }
        #endregion

        #region Utility Methods
        private void UpdateCooldown()
        {
            if (grapplingCooldownTimer > 0)
            {
                grapplingCooldownTimer -= Time.deltaTime;
            }
        }

        private void CheckForGrapplingInterruption()
        {
            if (pm.dashing || pm.wallRunning || pm.sliding)
            {
                StopGrapple();
            }
        }

        private void UpdateGrapplingRope()
        {
            if (pm.activeGrapple)
            {
                lineRenderer.SetPosition(0, gunTip.position);
            }
        }

        private void PlayGrapplingSound()
        {
            AudioManager.instance.PlayOneShot(FMODEvents.Instance.Zipline, transform.position);
        }
        #endregion

        #region Public Methods
        public void StopGrapple()
        {
            pm.freeze = false;
         
            grapplingCooldownTimer = grapplingCooldown;
            lineRenderer.enabled = false;
            grapplingGunAnimator.SetTrigger("GrappleOff");

            AudioManager.instance.StopInstance(ziplineInstance);
            pm.ModifySpeed(grappleSpeedBoost, grappleSpeedBoostDuration);
        }

      

        public Vector3 GetGrapplePoint() => grapplePoint;

        public void ResetRestrictions()
        {
            pm.activeGrapple = false;
            pm.cam.DoFov(85f);
        }
        #endregion
    }
}