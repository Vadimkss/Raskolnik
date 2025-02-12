using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;

namespace Movement
{
    public class Sliding : MonoBehaviour
    {
        #region Variables

        [Header("REFERENCES")]
        public Transform orientation;
        public Transform playerObj;
        private Rigidbody rb;
        private PlayerMovementAdvanced pm;

        [Header("Sliding Mechanics")]
        [SerializeField] private float stopSpeedThreshold = 5f;
        [SerializeField] float groundPullForce;
        [SerializeField] private float maxSlideSpeed = 40f;
        public float currentSlideSpeed;
        public float slideSpeed;

        [Header("Dynamic Friction")]
        [SerializeField] private float initialFriction = 1.0f;
        [SerializeField] private float frictionDecayRate = 0.1f;
        [SerializeField] private float minFriction = 0.95f;
        [SerializeField] private float maxFriction = 1.05f;
        [SerializeField] private float upwardFriction = 0.9f;

        [Header("Downhill Acceleration")]
        [SerializeField] private float downhillSpeedMultiplier = 1.2f;
        [SerializeField] private float accelerationInterval = 0.5f;
        [SerializeField] private float downhillAcceleration = 5f;
        private float lastAccelerationTime;
        private bool isSlindingDownhill;

        private Vector3 slideDirection;
        private float currentFriction;

        [Header("Camera & Effects")]
        public float dashFov = 100f;
        public Transform cameraTransform;
        public VisualEffect dashEffect;
        private float originalFov;

        [Header("Input")]
        public KeyCode slideKey = KeyCode.LeftControl;

        [Header("Scaling Effect")]
        [SerializeField] private float slideScale = 0.8f;
        [SerializeField] private float scaleDuration = 0.2f;

        #endregion

        #region Monos

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            pm = GetComponent<PlayerMovementAdvanced>();
            originalFov = cameraTransform.GetComponent<Camera>().fieldOfView;
            currentFriction = initialFriction;
        }
        #endregion

        #region Updates

        private void Update()
        {
            if (Input.GetKeyDown(slideKey) && !pm.sliding && !pm.wallRunning)
            {
                StartSlide();
            }

            if (pm.sliding)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StopSlide();
                }

                if (Input.GetKeyUp(slideKey))
                {
                    StopSlide();
                }
            }
        }
      

        private void FixedUpdate()
        {
            if (pm.sliding)
            {
                SlidingMovement();
                ApplyGroundPull();
            }
        }

        void UpdateCameraFOV()
        {
            float speedRatio = currentSlideSpeed / maxSlideSpeed;
            float targetFOV = Mathf.Lerp(originalFov, dashFov, speedRatio);
            cameraTransform.GetComponent<Camera>().fieldOfView =
            Mathf.Lerp(cameraTransform.GetComponent<Camera>().fieldOfView, targetFOV, Time.deltaTime * 5f);
        }

        #endregion

        #region SlidingMethods

        private float lastcurrentFriction;

        private void StartSlide()
        {
            pm.sliding = true;
            rb.useGravity = pm.sliding;
            lastAccelerationTime = Time.time;
            isSlindingDownhill = false;
            slideDirection = pm.grounded ? new Vector3(orientation.forward.x, 0, orientation.forward.z).normalized
                                         : new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
            Vector3 slideVelocity = slideDirection * pm.moveSpeed;
            rb.velocity = new Vector3(slideVelocity.x, rb.velocity.y, slideVelocity.z);
            currentFriction = initialFriction;
            dashEffect?.Play();
            cameraTransform.GetComponent<Camera>().DOFieldOfView(dashFov, 0.2f);
            playerObj.DOScale(slideScale, scaleDuration);
        }

        private void SlidingMovement()
        {
            if (!pm.grounded) return;

            float slopeAngle = GetSlopeAngle();
            Vector3 surfaceNormal = GetSurfaceNormal();
            Vector3 slopeDirection = Vector3.ProjectOnPlane(slideDirection, surfaceNormal).normalized;

            if (slopeAngle < 0f && Mathf.Abs(slopeAngle) < 45f) // Спуск
            {
                isSlindingDownhill = true;
                groundPullForce = 200;


                if (Time.time - lastAccelerationTime >= accelerationInterval)
                {
                    currentSlideSpeed *= downhillSpeedMultiplier;
                    Vector3 normalizedVelocity = rb.velocity.normalized;
                    rb.velocity = normalizedVelocity * currentSlideSpeed;
                    lastAccelerationTime = Time.time;
                }

                float slopeAcceleration = Mathf.Lerp(0f, downhillAcceleration, Mathf.Abs(slopeAngle) / 45f);
                Vector3 accelerationForce = slopeDirection * slopeAcceleration * 2;
                rb.AddForce(accelerationForce, ForceMode.Acceleration);

                Debug.Log("Вниз " + currentFriction);
            }

            else
            {
                isSlindingDownhill = false;

                if (slopeAngle > 0f) // Подъем
                {
                    groundPullForce = 0;
                    currentFriction = Mathf.Max(currentFriction - frictionDecayRate * Time.fixedDeltaTime * upwardFriction, minFriction);
                    rb.velocity *= currentFriction;

                    if (rb.velocity.magnitude < stopSpeedThreshold)
                    {
                        StopSlide();
                    }

                    Debug.Log("Вверх " + currentFriction);
                }
                else // Ровная поверхность
                {
                    if (!pm.grounded)
                    {
                        groundPullForce = 0;
                    }

                    currentFriction = Mathf.Max(currentFriction - frictionDecayRate * Time.fixedDeltaTime * upwardFriction, minFriction);
                    rb.velocity *= currentFriction;
                    Debug.Log("ровно " + currentFriction);

                }
            }

            currentSlideSpeed = rb.velocity.magnitude;
            pm.desiredMoveSpeed = currentSlideSpeed;
            UpdateCameraFOV();
        }

        private void StopSlide()
        {
            if (currentSlideSpeed < pm.baseMoveSpeed)
            {
                pm.moveSpeed = pm.baseMoveSpeed;
            }

            currentSlideSpeed = 0;
            currentFriction = 0;
            pm.sliding = false;
            pm.sliding = false;
            dashEffect?.Stop();
            cameraTransform.GetComponent<Camera>().DOFieldOfView(originalFov, 0.2f);
            playerObj.DOScale(2f, scaleDuration);

        }

        private void ApplyGroundPull()
        {
            if (pm.grounded) rb.AddForce(Vector3.down * groundPullForce, ForceMode.Acceleration);
        }

        #endregion

        #region SlopeSlideCalculation

        private Vector3 GetSurfaceNormal()
        {
            float rayDistance = 2f;
            Vector3[] rayOffsets = new Vector3[]
            {
                Vector3.zero,
                Vector3.forward * 0.5f,
                Vector3.back * 0.5f,
                Vector3.left * 0.5f,
                Vector3.right * 0.5f
            };

            foreach (var offset in rayOffsets)
            {
                Vector3 rayOrigin = transform.position + offset;
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance))
                {
                    return hit.normal;
                }
            }

            return Vector3.up;
        }

        private float GetSlopeAngle()
        {
            float rayDistance = 2f;
            Vector3[] rayOffsets = new Vector3[]
            {
            Vector3.zero,
            Vector3.forward * 0.5f,
            Vector3.back * 0.5f,
            Vector3.left * 0.5f,
            Vector3.right * 0.5f
            };

            float totalAngle = 0f;
            int rayCount = 0;

            foreach (var offset in rayOffsets)
            {
                Vector3 rayOrigin = transform.position + offset;
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance))
                {
                    Vector3 surfaceNormal = hit.normal;
                    Vector3 projectedDirection = Vector3.ProjectOnPlane(slideDirection, surfaceNormal).normalized;
                    float dot = Vector3.Dot(projectedDirection, Vector3.up);
                    float angle = Vector3.Angle(Vector3.up, surfaceNormal);

                    if (dot < 0)
                        angle = -angle;

                    totalAngle += angle;
                    rayCount++;
                }
            }

            if (rayCount > 0)
            {
                return totalAngle / rayCount;
            }

            return 0f;
        }

        #endregion

    }
}
