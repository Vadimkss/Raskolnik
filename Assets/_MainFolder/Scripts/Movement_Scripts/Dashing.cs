using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.UI;
using NTC.MonoCache;

public class Dashing : MonoCache
{
    public ChromaticAberrationController chromaticAberrationController;

    [Header("References")]
    public Transform orientation;
    public Transform playerCam;
    private Rigidbody rb;
    private PlayerMovementAdvanced pm;
    private Grappling gr;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float maxDashYSpeed;
    public float dashDuration;
    private bool dashReset = true;
    public AudioSource dashAudioEffect;
    public float baseSpeed;
    public float speedBoostAmount;
    public float boostTime;

    [Header("CameraEffects")]
    public PlayerCam cam;
    public float dashFov;
    CameraShaker cameraShaker;

    [Header("Settings")]
    public bool useCameraForward = true;
    public bool allowAllDirections = true;
    public bool disableGravity = false;
    public bool resetVel = true;

    [Header("Cooldown")]
    public float dashCd;
    private float dashCdTimer;

    [Header("Input")]
    public KeyCode dashKey = KeyCode.E;

    public VisualEffect dashEffect;

    [Header("Stamina")]
    public float currentStamina;
    public float maxStamina;
    public float staminaReduction;

    public Image staminaBar;

   

    private Vector3 delayedForceToApply;

    void Start()
    {
        gr = GetComponent<Grappling>();
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
        dashEffect.enabled = false;
        currentStamina = maxStamina;
    }

    protected override void Run()
    {
        if (Input.GetKeyDown(dashKey) && currentStamina >= 100f)
        {
            Dash();
        }
    }

    protected override void FixedRun()
    {
        if (dashCdTimer > 0)
            dashCdTimer -= Time.deltaTime;

        if (dashReset)
        {
            StaminaReduction();
        }

        

        // Update stamina bar
        float fillAmount = currentStamina / maxStamina;
        staminaBar.fillAmount = fillAmount;
    }

    private void Dash()
    {
        // Отключаем дополнительное ускорение, удаляем увеличение скорости при рывке
        pm.groundDrag = 0f;
        rb.AddForce(Vector3.down * 0.1f, ForceMode.Acceleration);

        // Проверяем кулдаун
        if (dashCdTimer > 0) return;

        dashCdTimer = dashCd;

        pm.dashing = true;
        pm.maxYSpeed = maxDashYSpeed;

        // Сохраняем текущую базовую скорость
        baseSpeed = pm.moveSpeed;

        // Убираем ускорение от рывка (больше не изменяем moveSpeed)
        pm.desiredMoveSpeed = baseSpeed;

        cam.DoFov(dashFov);

        // Определяем направление рывка на основе ввода WASD
        Transform forwardT = useCameraForward ? playerCam : orientation;
        Vector3 direction = GetDirection(forwardT); // Получаем направление из ввода

        // Применяем силу рывка в выбранном направлении
        Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardForce;

        if (disableGravity) rb.useGravity = false;

        delayedForceToApply = forceToApply;
        Invoke(nameof(DelayedDashForce), 0.025f); // Применяем силу через небольшой промежуток
        Invoke(nameof(ResetDash), dashDuration); // Сбрасываем параметры после окончания рывка

        dashEffect.enabled = true;
        currentStamina -= 100f;
        dashReset = false;
        dashAudioEffect.Play();

        if (pm.activeGrapple)
        {
            pm.activeGrapple = false;
        }

        AudioManager.instance.PlayOneShot(FMODEvents.Instance.DashSFX, this.transform.position);
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D
        float verticalInput = Input.GetAxisRaw("Vertical"); // W/S

        // Направление рывка будет зависеть от ввода на WASD
        Vector3 direction = forwardT.forward * verticalInput + forwardT.right * horizontalInput;

        // Устанавливаем Y-компоненту в 0, чтобы избежать движения вверх/вниз
        if (direction.y < 0)
        {
            direction.y = 0;
        }

        // Если нет ввода, используем переднее направление камеры
        if (verticalInput == 0 && horizontalInput == 0)
        {
            direction = forwardT.forward;
        }

        // Если игрок смотрит вниз, исключаем вертикальную составляющую
        if (Vector3.Dot(forwardT.up, Vector3.down) > 0)
        {
            direction.y = 0; // Игнорируем вертикальную составляющую
        }

        return direction.normalized;
    }


    private void DelayedDashForce()
    {
        if (resetVel)
            rb.velocity = Vector3.zero;

        rb.AddForce(delayedForceToApply, ForceMode.Impulse);

    }

    private void ResetDash()
    {
        rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration); 
        pm.groundDrag = 5f;
        pm.dashing = false;
        pm.maxYSpeed = 0;

        cam.DoFov(70f);

        if (disableGravity)
            rb.useGravity = true;

        // Плавно уменьшаем скорость до базовой
   

        dashEffect.enabled = false;
        StartCoroutine(DeactivateEffectCoroutine());
        dashReset = true;
        pm.canMove = true;
      
    }

 

    private void StaminaReduction()
    {
        if (currentStamina < maxStamina && pm.grounded)
            currentStamina += staminaReduction;
        else if (currentStamina < maxStamina && !pm.grounded)
            currentStamina += 0.5f * staminaReduction;
        if (currentStamina < 60)
            currentStamina += staminaReduction * 1.5f;
    }

    private IEnumerator DeactivateEffectCoroutine()
    {
        yield return new WaitForSeconds(1f); // Delay of one second
        dashEffect.enabled = false;
    }

   
}
