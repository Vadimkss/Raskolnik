using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;

public class Sliding : MonoBehaviour
{
    [Header("REFERENCES")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovementAdvanced pm;

    public bool sliding;
    public Animator LLegAnimator;
    public Animator RLegAnimator;

    [Header("Sliding Mechanics")]
    public float slideSpeed;
    public float initialSlideSpeed = 20f; // Начальная скорость подката
    public float minFriction = 0.95f; // Минимальное трение на ровной поверхности
    public float maxFriction = 1.05f; // Максимальное трение на спуске
    public float upwardFriction = 0.9f; // Трение на подъеме
    public float stopSpeedThreshold = 5f; // Порог остановки по скорости
    public float currentSlideSpeed;
    [SerializeField] float groundPullForce;
    public float maxSlideSpeed = 40f; // Максимальная скорость скольжения
    public float downhillAcceleration = 5f; // Ускорение при движении вниз

    [Header("Dynamic Friction")]
    public float initialFriction = 1.0f; // Начальное трение
    public float frictionDecayRate = 0.1f; // Скорость снижения трения
   

    private Vector3 slideDirection;
    private float currentFriction;

    [Header("Camera & Effects")]
    public Transform cameraTransform;
    public float dashFov = 100f;
    public VisualEffect dashEffect;

    private float originalFov;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;

    [Header("Scaling Effect")]
    public float slideScale = 0.8f; // Размер во время подката
    public float scaleDuration = 0.2f; // Длительность анимации масштабирования

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();

        originalFov = cameraTransform.GetComponent<Camera>().fieldOfView;
        currentFriction = initialFriction;
    }

    private void Update()
    {
        if (Input.GetKeyDown(slideKey) && !sliding && !pm.wallRunning) 
        {
            StartSlide();
        }

        if (sliding)
        {
            // Прерывание подката прыжком
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StopSlide();
            }

            // Прерывание подката при отпускании клавиши
            if (Input.GetKeyUp(slideKey))
            {
                StopSlide();
            }
        }
    }

    private void FixedUpdate()
    {
        if (sliding)
        {
            SlidingMovement();
            ApplyGroundPull(); // Применяем дополнительное притяжение к земле
        }
    }

    private void ApplyGroundPull()
    {
       
     

        rb.AddForce(Vector3.down * groundPullForce, ForceMode.Acceleration);
    }

    private void StartSlide()
    {
        groundPullForce = 100;

        sliding = true;
        pm.sliding = true;
        pm.desiredMoveSpeed = slideSpeed;

        rb.useGravity = sliding;

        // Анимация
        LLegAnimator.SetTrigger("Sliding");
        RLegAnimator.SetTrigger("Sliding");

        // Направление подката фиксируется на момент начала
        slideDirection = pm.grounded ? new Vector3(orientation.forward.x, 0, orientation.forward.z).normalized
                                     : new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;

        // Добавление начальной скорости подката
        Vector3 slideVelocity = slideDirection * initialSlideSpeed;
        rb.velocity = new Vector3(slideVelocity.x, rb.velocity.y, slideVelocity.z);

        currentFriction = initialFriction; // Сброс трения к начальному значению

        // Эффекты
        dashEffect?.Play();
        cameraTransform.GetComponent<Camera>().DOFieldOfView(dashFov, 0.2f);

        // Уменьшение объекта игрока
        playerObj.DOScale(slideScale, scaleDuration);
    }

    private void SlidingMovement()
    {
        if (!pm.grounded) // Если персонаж в воздухе, не изменяем скорость
            return;

        float slopeAngle = GetSlopeAngle(); // Получаем угол наклона
        Vector3 surfaceNormal = GetSurfaceNormal(); // Получаем нормаль поверхности

        // Движение только в фиксированном направлении
        Vector3 slopeDirection = Vector3.ProjectOnPlane(slideDirection, surfaceNormal).normalized;

        if (slopeAngle < 0f && Mathf.Abs(slopeAngle) < 45f) // Спуск
        {
            groundPullForce = 100;

            // Базовое ускорение на основе крутизны склона
            float slopeAcceleration = Mathf.Lerp(0f, downhillAcceleration, Mathf.Abs(slopeAngle) / 45f);

            // Применяем ускорение в направлении склона
            Vector3 accelerationForce = slopeDirection * slopeAcceleration * 2;

         
           

            Debug.Log("вниз, текущая скорость: " + currentSlideSpeed);
        }

        else if (slopeAngle > 0f) // Подъем
            {
            groundPullForce =  0;
            currentFriction = Mathf.Max(currentFriction - frictionDecayRate * Time.fixedDeltaTime * upwardFriction, minFriction);
            rb.velocity *= currentFriction;


            if (rb.velocity.magnitude < stopSpeedThreshold)
            {
                StopSlide();
            }

            Debug.Log("вверх");
        }
        else // Ровная поверхность
        {
            groundPullForce = 100;
            currentFriction = Mathf.Max(currentFriction - frictionDecayRate * Time.fixedDeltaTime, minFriction);
            rb.velocity *= currentFriction;

            Debug.Log("ровно");
        }

        currentSlideSpeed = rb.velocity.magnitude;
        pm.desiredMoveSpeed = currentSlideSpeed;
        UpdateCameraFOV();
        if (currentSlideSpeed < stopSpeedThreshold)
        {
            StopSlide();
        }
    }

    void UpdateCameraFOV()
    {
        float speedRatio = currentSlideSpeed / maxSlideSpeed;
        float targetFOV = Mathf.Lerp(originalFov, dashFov, speedRatio);
        cameraTransform.GetComponent<Camera>().fieldOfView =
            Mathf.Lerp(cameraTransform.GetComponent<Camera>().fieldOfView, targetFOV, Time.deltaTime * 5f);
    }

    private Vector3 GetSurfaceNormal()
    {
        float rayDistance = 2f;
        Vector3[] rayOffsets = new Vector3[] {
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
        Vector3[] rayOffsets = new Vector3[] {
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

                // Проецируем направление скольжения на плоскость склона
                Vector3 projectedDirection = Vector3.ProjectOnPlane(slideDirection, surfaceNormal).normalized;

                // Определяем знак угла на основе направления движения
                float dot = Vector3.Dot(projectedDirection, Vector3.up);
                float angle = Vector3.Angle(Vector3.up, surfaceNormal);

                // Если движемся вверх по склону, делаем угол отрицательным
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

    private void StopSlide()
    {
        // Явно сбрасываем скорость, чтобы предотвратить сохранение скорости подката
        rb.velocity = new Vector3(0, rb.velocity.y, 0);  // Сбрасываем горизонтальную скорость, оставляя вертикальную
     
        currentSlideSpeed = 0;
        currentFriction = 0;
        sliding = false;
        pm.sliding = false;

        // Снимаем анимацию подката
        LLegAnimator.SetTrigger("StopSliding");
        RLegAnimator.SetTrigger("StopSliding");

        // Эффекты
        dashEffect?.Stop();
        cameraTransform.GetComponent<Camera>().DOFieldOfView(originalFov, 0.2f);

        // Возвращение объекта игрока к исходному размеру
        playerObj.DOScale(2f, scaleDuration);

        pm.moveSpeed = pm.baseMoveSpeed;
    }
}
