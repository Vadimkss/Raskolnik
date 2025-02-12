using System.Collections;
using UnityEngine;
using TMPro;
using FMOD.Studio;
using NTC.MonoCache;

namespace Movement
{

    public class PlayerMovementAdvanced : MonoCache
    {
        #region Variables

        [Header("Movement")]
        public float moveSpeed;
        public float baseMoveSpeed;
        [SerializeField] private float walkSpeed;
        [SerializeField] private float airMovementSpeed;
        public float speedIncreaseMultiplier;
        public float slopeIncreaseMultiplier;
        public float dashSpeed;
        public float dashSpeedChangeFactor;
        public float maxYSpeed;
        public float groundDrag;
        public float wallRunSpeed;
        private float externalSpeedModifier = 1f;
        private Coroutine speedModifierCoroutine;
        public bool canMove = true;


        [Header("Jumping")]
        public float jumpForce;
        public float jumpCooldown;
        public float airMultiplier;
        public bool readyToJump;
        public int maxJumpCount = 2;
        public int jumpsRemaining = 0;


        [Header("Keybinds")]
        public KeyCode jumpKey = KeyCode.Space;


        [Header("Ground Check")]
        public float playerHeight;
        public LayerMask whatIsGround;
        public bool grounded;
        public bool UseGravity;

        [Header("Slope Handling")]
        public float maxSlopeAngle;
        private RaycastHit slopeHit;
        private bool exitingSlope;

        [Header("Camera Effects")]
        public PlayerCam cam;
        public float grappleFov = 95f;

        [Header("Footsteps Audio")]
        public float footstepDistanceThreshold = 1f;
        public float maxFootstepFrequency = 0.5f;
        private float footstepDistanceTraveled;
        private float footstepTimer;


        private EventInstance FootSteps;

        [Header("UI")]
        public TextMeshProUGUI moveSpeedText;

        public Transform orientation;

        private float horizontalInput;
        private float verticalInput;
        private Vector3 moveDirection;
        private Rigidbody rb;
        private bool wasInAir = false;
        private Grappling gr;
        private Sliding sl;
        private MovementState state;
        private GameObject mainCamera;
        private enum MovementState
        {
            freeze,
            walking,
            sprinting,
            wallrunning,
            crouching,
            dashing,
            sliding,
            air
        }

        public bool freeze;
        public bool dashing;
        public bool sliding;
        public bool activeGrapple;
        public bool wallRunning;

        #endregion

        #region Monos

        private void Start()
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            sl = GetComponent<Sliding>();
            gr = GetComponent<Grappling>();
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            UseGravity = true;
            readyToJump = true;
            jumpsRemaining = maxJumpCount;
            FootSteps = AudioManager.instance.CreateInstance(FMODEvents.Instance.FootSteps);
            footstepDistanceTraveled = 0f;
            footstepTimer = 0f;
        }

        #endregion

        #region Updates

        protected override void Run()
        {
            float insideOffset = -0.5f;
            Vector3 raycastStart = transform.position - Vector3.up * insideOffset;
            bool previouslyGrounded = grounded;
            grounded = Physics.Raycast(raycastStart, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
            Debug.DrawRay(raycastStart, Vector3.down * (playerHeight * 0.5f + 0.2f), grounded ? Color.green : Color.red);

            MyInput();
            SpeedControl();
            StateHandler();

            if (!previouslyGrounded && grounded && wasInAir)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.Instance.Falled, transform.position);
                wasInAir = false;
            }

            if (!grounded && !wasInAir)
            {
                wasInAir = true;
            }

            UpdateMoveSpeedText();
        }

        protected override void FixedRun()
        {
            MovePlayer();
            RotateBodyByCam();

            rb.drag = grounded && !activeGrapple ? groundDrag : 0;
            if (grounded)
            {
                jumpsRemaining = maxJumpCount;

                if (grounded && !sliding)
                {
                    jumpsRemaining = maxJumpCount;

                    UpdateFootstepAudio();
                }
            }
        }

        #endregion

        #region TehnicionMethods

        private void RotateBodyByCam()
        {
            Vector3 currentRotation = transform.eulerAngles;
            currentRotation.y = mainCamera.transform.eulerAngles.y;
            transform.eulerAngles = currentRotation;
        }

        private void MyInput()
        {
            if (!canMove || activeGrapple) return;

            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");

            if (Input.GetKeyDown(jumpKey) && readyToJump && jumpsRemaining > 0 && !sliding)
            {
                readyToJump = false;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }

        }

        private void OnCollisionEnter(Collision collision)
        {
            if (enableMovementOnNextTouch)
            {
                enableMovementOnNextTouch = false;
                gr.ResetRestrictions();
                GetComponent<Grappling>().StopGrapple();
            }
        }

        #endregion

        #region StateHandler

        public float desiredMoveSpeed;
        private float lastDesiredMoveSpeed;
        private MovementState lastState;
        private bool keepMomentum;

        private void StateHandler()
        {

            if (wallRunning && !sliding)
            {
                state = MovementState.sliding;

            }

            else if (sliding)
            {
                state = MovementState.sliding;

                if (OnSlope() && rb.velocity.y < 0.1f)
                    desiredMoveSpeed = sl.currentSlideSpeed;
            }

            else if (dashing)
            {
                state = MovementState.dashing;
                desiredMoveSpeed = dashSpeed;
            }

            else if (grounded)
            {
                state = MovementState.walking;
                desiredMoveSpeed = walkSpeed;
            }

            else
            {
                state = MovementState.air;
                desiredMoveSpeed = desiredMoveSpeed < airMovementSpeed ? walkSpeed : airMovementSpeed;
            }

            if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 10f && moveSpeed != 0)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                moveSpeed = desiredMoveSpeed;
            }

            lastDesiredMoveSpeed = desiredMoveSpeed;
        }

        private IEnumerator SmoothlyLerpMoveSpeed()
        {
            float time = 0;
            float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
            float startValue = moveSpeed;

            while (time < difference)
            {
                moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

                if (OnSlope())
                {
                    float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                    float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                    time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
                }
                else
                    time += Time.deltaTime * speedIncreaseMultiplier;

                yield return null;
            }

            moveSpeed = desiredMoveSpeed;
        }

        #endregion

        #region MovePlayer

        private void MovePlayer()
        {

            if (activeGrapple || state == MovementState.dashing || !canMove) return;


            if (sliding)
            {

                rb.AddForce(moveDirection.normalized * sl.slideSpeed * 10f, ForceMode.Force);
                return;
            }


            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;


            if (OnSlope() && !exitingSlope)
            {
                rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);


                if (rb.velocity.y > 0)
                    rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }

            else if (grounded)
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
            }

            else
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
            }

            rb.useGravity = !OnSlope() && UseGravity && !sliding;
        }


        public void DisableMovement(float duration)
        {
            Debug.Log("DisableMovement called for duration: " + duration);
            if (canMove)
            {
                canMove = false;
                StartCoroutine(EnableMovementAfterDelay(duration));

                Debug.Log("Стопе");
            }
        }

        private IEnumerator EnableMovementAfterDelay(float duration)
        {
            yield return new WaitForSeconds(duration);
            canMove = true;
            Debug.Log("Movement enabled after " + duration + " seconds");
        }

        #endregion

        #region Speed_&_Jumps

        private void SpeedControl()
        {
            if (activeGrapple) return;

            if (OnSlope() && !exitingSlope)
            {
                if (rb.velocity.magnitude > moveSpeed) rb.velocity = rb.velocity.normalized * moveSpeed;
            }
            else
            {
                Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

                if (flatVel.magnitude > moveSpeed)
                {
                    Vector3 limitedVel = flatVel.normalized * moveSpeed;
                    rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
                }
            }

            if (maxYSpeed != 0 && rb.velocity.y > maxYSpeed)
                rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);


        }

        public void Jump()
        {
            exitingSlope = true;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            jumpsRemaining -= 1;

            if (activeGrapple)
            {
                activeGrapple = false;
            }

            if (grounded)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.Instance.Jump, this.transform.position);

            }

            else
            {
                AudioManager.instance.PlayOneShot(FMODEvents.Instance.jump2, this.transform.position);
            }
        }

        private void ResetJump()
        {
            readyToJump = true;
            exitingSlope = false;
        }



        public void JumpToPosition(Vector3 targetPosition, float speed)
        {
            activeGrapple = true;
            Vector3 direction = (targetPosition - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            Vector3 velocity = direction * speed;
            rb.velocity = velocity;
            cam.DoFov(grappleFov);
            Invoke(nameof(StopAtTarget), distanceToTarget / speed);
        }

        public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
        {
            float gravity = Physics.gravity.y;
            float displacementY = endPoint.y - startPoint.y;
            Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);
            Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
            Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

            return velocityXZ + velocityY;
        }

        private void StopAtTarget()
        {
            rb.velocity = Vector3.zero;
            gr.ResetRestrictions();
            gr.StopGrapple();
        }



        private Vector3 velocityToSet;

        private bool enableMovementOnNextTouch;

        private void SetVelocity()
        {
            enableMovementOnNextTouch = true;
            rb.velocity = velocityToSet;

            cam.DoFov(grappleFov);
        }

        #endregion

        #region Slope

        public bool OnSlope()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f) && !sliding)
            {
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                return angle < maxSlopeAngle && angle != 0;
            }

            return false;
        }

        public Vector3 GetSlopeMoveDirection(Vector3 direction)
        {
            return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
        }

        #endregion

        #region SpeedModify

        public void ModifySpeed(float modifier, float duration)
        {
            speedModifierCoroutine = StartCoroutine(ApplySpeedModifier(modifier, duration));
        }


        private IEnumerator ApplySpeedModifier(float modifier, float duration)
        {
            externalSpeedModifier = modifier;
            UpdateMoveSpeed();

            float elapsedTime = 0f;
            float initialModifier = externalSpeedModifier;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                externalSpeedModifier = Mathf.Lerp(initialModifier, 1f, elapsedTime / duration);
                UpdateMoveSpeed();
                yield return null;
            }
            externalSpeedModifier = 1f;
            UpdateMoveSpeed();
            speedModifierCoroutine = null;
        }

        private void UpdateMoveSpeed()
        {
            moveSpeed = desiredMoveSpeed + externalSpeedModifier;
        }
        private void UpdateMoveSpeedText()
        {
            if (moveSpeedText != null)
            {
                float currentSpeed = rb.velocity.magnitude;
                moveSpeedText.text = "Speed: " + currentSpeed.ToString("F2") + " m/s";
            }
        }
        private void UpdateFootstepAudio()
        {
            float speed = rb.velocity.magnitude;

            if (speed > 0.1f && grounded && !sliding)
            {
                footstepTimer += Time.deltaTime;

                if (footstepTimer >= maxFootstepFrequency / (speed * 2 / walkSpeed))
                {
                    footstepTimer = 0f;
                    AudioManager.instance.PlayOneShot(FMODEvents.Instance.FootSteps, this.transform.position);
                }
            }
            else
            {
                footstepTimer = 0f;
            }
        }

        #endregion
    }
}