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
    public float initialSlideSpeed = 20f; // Начальная скорость подката
    public float minFriction = 0.95f; // Минимальное трение на ровной поверхности
    public float maxFriction = 1.05f; // Максимальное трение на спуске
    public float upwardFriction = 0.9f; // Трение на подъеме
    public float stopSpeedThreshold = 5f; // Порог остановки по скорости
    public float currentSlideSpeed;

    [Header("Dynamic Friction")]
    public float initialFriction = 1.0f; // Начальное трение
    public float frictionDecayRate = 0.1f; // Скорость снижения трения
    public float upwardFrictionMultiplier = 2.0f; // Множитель ускоренного снижения трения на подъеме

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
        float groundPullForce = 100f; // Минимальное значение притяжения
        groundPullForce = Mathf.Max(groundPullForce, 300f); // Устанавливаем минимальное значение

        rb.AddForce(Vector3.down * groundPullForce, ForceMode.Acceleration);
    }

    private void StartSlide()
    {
        sliding = true;
        pm.sliding = true;
        pm.desiredMoveSpeed = pm.slideSpeed;

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

        if (slopeAngle > 0f && slopeAngle < 45f) // Спуск
        {
            // Ускорение на спуске
            float acceleration = Mathf.Lerp(0f, maxFriction, slopeAngle / 45f);
            rb.AddForce(slopeDirection * acceleration, ForceMode.Acceleration);
        }
        else if (slopeAngle >= 45f) // Подъем
        {
            Vector3 uphillDirection = Vector3.ProjectOnPlane(slideDirection, surfaceNormal).normalized;

            // Увеличиваем трение на подъеме
            float uphillResistance = upwardFriction * upwardFrictionMultiplier;
            rb.AddForce(-uphillDirection * uphillResistance, ForceMode.Acceleration);

            // Принудительное снижение скорости
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, frictionDecayRate * Time.fixedDeltaTime);

            if (rb.velocity.magnitude < stopSpeedThreshold)
            {
                StopSlide();
            }
        }
        else // Ровная поверхность
        {
            currentFriction = Mathf.Max(currentFriction - frictionDecayRate * Time.fixedDeltaTime, minFriction);
            rb.velocity *= currentFriction;
        }

        currentSlideSpeed = rb.velocity.magnitude;
        pm.desiredMoveSpeed = currentSlideSpeed;

        if (currentSlideSpeed < stopSpeedThreshold)
        {
            StopSlide();
        }
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
                totalAngle += Vector3.Angle(Vector3.up, surfaceNormal);
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
