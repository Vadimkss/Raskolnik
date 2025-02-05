using System.Collections;
using UnityEngine;
using TMPro;
using FMOD.Studio;
using NTC.MonoCache;

public class PlayerMovementAdvanced : MonoCache
{
    [Header("Movement")]
    public float moveSpeed;
    public float baseMoveSpeed; // Базовая скорость
    [SerializeField]private float walkSpeed;
    [SerializeField]private float airMovementSpeed;
  
    public float dashSpeed;
    public float dashSpeedChangeFactor;
    public float maxYSpeed;
    public float groundDrag;
    public float wallRunSpeed;
    private float externalSpeedModifier = 1f; // Множитель скорости, изменяемый из других скриптов
    private Coroutine speedModifierCoroutine; // Коррутина для управления временем действия модификации скорости
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
    public float footstepDistanceThreshold = 1f; // Расстояние, через которое проигрывается звук
    public float maxFootstepFrequency = 0.5f; // Максимальная частота воспроизведения звука шагов (в секундах)

    private float footstepDistanceTraveled; // Расстояние, пройденное с последнего воспроизведения звука
    private float footstepTimer; // Таймер для отслеживания частоты воспроизведения

    //Audio
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
    public MovementState state;
    private GameObject mainCamera;
    public enum MovementState
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

    protected override void Run()
    {
        float insideOffset = -0.5f;
        Vector3 raycastStart = transform.position - Vector3.up * insideOffset;

        // ground check
        bool previouslyGrounded = grounded;
        grounded = Physics.Raycast(raycastStart, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        Debug.DrawRay(raycastStart, Vector3.down * (playerHeight * 0.5f + 0.2f), grounded ? Color.green : Color.red);

        MyInput();
        SpeedControl();
        StateHandler();

        // Check for landing after being in air
        if (!previouslyGrounded && grounded && wasInAir)
        {
            // Проигрываем звук приземления
            AudioManager.instance.PlayOneShot(FMODEvents.Instance.Falled, transform.position);

            // Устанавливаем, что персонаж больше не в воздухе
            wasInAir = false;
        }

        // If character is in the air and not grounded, mark them as being in the air
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

        // Обновление звука шагов
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

    private void RotateBodyByCam()
    {
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.y = mainCamera.transform.eulerAngles.y;
        transform.eulerAngles = currentRotation;
    }

    //метод для временного отключения движения игрока
    public void DisableMovement(float duration)
    {
        Debug.Log("DisableMovement called for duration: " + duration);
        if (canMove) // Проверяем, действительно ли нужно отключить движение
        {
            canMove = false; // Отключаем движение
            StartCoroutine(EnableMovementAfterDelay(duration)); // Запускаем корутину для восстановления

            Debug.Log("Стопе");
        }
    }

    // Коррутина для восстановления возможности движения
    private IEnumerator EnableMovementAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration); // Ждем заданное время
        canMove = true; // Восстанавливаем возможность движения
        Debug.Log("Movement enabled after " + duration + " seconds");
    }

    private void MyInput()
    {
        if (!canMove || gr.grappling) return; // Если движение отключено, выходим из метода

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKeyDown(jumpKey) && readyToJump && jumpsRemaining > 0 && !sliding)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

    }

    public float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;

    private void StateHandler()
    {
        // Mode - Freeze
        if (freeze)
        {
            state = MovementState.freeze;
        }
        // Mode - Wallrunning
        else if (wallRunning && !sliding)
        {
            state = MovementState.wallrunning;

        }
        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;

        }
        // Mode - Dashing
        else if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
        }
      
        // Mode - Sprinting
     
        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        // Mode - Air
        else
        {
            state = MovementState.air;
            desiredMoveSpeed = desiredMoveSpeed < airMovementSpeed ? walkSpeed : airMovementSpeed;
        }

        // Если новая скорость отличается от предыдущей
        if (desiredMoveSpeed != lastDesiredMoveSpeed)
        {
            StopAllCoroutines();

            if (keepMomentum)
            {
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                moveSpeed = desiredMoveSpeed * externalSpeedModifier;
            }
        }


        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }




    

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - walkSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference) * externalSpeedModifier;
            time += Time.deltaTime;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed * externalSpeedModifier;
    }

    private void MovePlayer()
    {
        // Если игрок в состоянии подката или других исключений
        if (activeGrapple || state == MovementState.dashing || !canMove) return;

        // При подкате не изменяем направление, только применяем уже существующее движение
        if (sliding)
        {

            rb.AddForce(moveDirection.normalized * sl.slideSpeed * 10f, ForceMode.Force);
            return;
        }

        // Обновляем направление движения
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Если игрок на склоне
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            // Стабилизация на склоне
            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        // Если игрок на земле
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // Если игрок в воздухе
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // Управление гравитацией
        rb.useGravity = !OnSlope() && UseGravity && !sliding;
    }

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

        // Плавное уменьшение скорости, если она больше базовой
       
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

        // Рассчитываем направление к цели
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Рассчитываем расстояние до точки зацепа
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // Устанавливаем скорость в зависимости от расстояния до точки зацепа
        Vector3 velocity = direction * speed;

        // Применяем силу к Rigidbody, чтобы двигаться к точке
        rb.velocity = velocity;

        cam.DoFov(grappleFov);

        // Останавливаем игрока через определенное время или при достижении цели
        Invoke(nameof(StopAtTarget), distanceToTarget / speed);
    }

    // Останавливаем движение, когда игрок достиг точки
    private void StopAtTarget()
    {
        rb.velocity = Vector3.zero; // Останавливаем движение
        ResetRestrictions(); // Сбрасываем ограничения

        // Выполняем дополнительные действия, если нужно
        gr.StopGrapple(); // Останавливаем крюк
    }



    private Vector3 velocityToSet;

    private bool enableMovementOnNextTouch;

    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;

        cam.DoFov(grappleFov);
    }

    public void ResetRestrictions()
    {
        activeGrapple = false;
        cam.DoFov(85f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();
            GetComponent<Grappling>().StopGrapple();
        }
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
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

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    public void ModifySpeed(float modifier, float duration)
    {
       
        speedModifierCoroutine = StartCoroutine(ApplySpeedModifier(modifier, duration));
    }

    // Коррутина для применения модификации скорости
    private IEnumerator ApplySpeedModifier(float modifier, float duration)
    {
        externalSpeedModifier = modifier;
        UpdateMoveSpeed(); // Обновляем скорость сразу после изменения модификатора

      

        // Постепенный сброс к стандартной скорости
        float elapsedTime = 0f;
        float initialModifier = externalSpeedModifier;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            externalSpeedModifier = Mathf.Lerp(initialModifier, 1f, elapsedTime / duration);
            UpdateMoveSpeed(); // Обновляем скорость на каждом шаге
            yield return null;
        }

        externalSpeedModifier = 1f; // Сброс к стандартной скорости
        UpdateMoveSpeed(); // Обновляем скорость после сброса
        speedModifierCoroutine = null;
    }

   
   


    private void UpdateMoveSpeed()
    {
        moveSpeed = desiredMoveSpeed + externalSpeedModifier; // Обновляем moveSpeed с учетом модификатора
    }
    private void UpdateMoveSpeedText()
    {
        if (moveSpeedText != null)
        {
            // Текущая скорость движения персонажа (с учетом всех модификаторов)
            float currentSpeed = rb.velocity.magnitude;
            moveSpeedText.text = "Speed: " + currentSpeed.ToString("F2") + " m/s"; // Форматирование до двух знаков после запятой
        }
    }
    private void UpdateFootstepAudio()
    {
        float speed = rb.velocity.magnitude;

        // Если персонаж движется, на земле и не скользит
        if (speed > 0.1f && grounded && !sliding)
        {
            footstepTimer += Time.deltaTime;

            // Регулируем частоту шагов в зависимости от скорости
            if (footstepTimer >= maxFootstepFrequency / (speed * 2 / walkSpeed))
            {
                footstepTimer = 0f;
                AudioManager.instance.PlayOneShot(FMODEvents.Instance.FootSteps, this.transform.position);
            }
        }
        else
        {
            footstepTimer = 0f; // Сброс таймера, если персонаж не движется
        }
    }
}
