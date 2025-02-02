using UnityEngine;
using DG.Tweening;
using System.Collections;
using FMOD.Studio;

public class RevolverWeapon : MonoBehaviour
{
    private ComboManager comboManager;
  

    public Transform firePoint; // Точка выстрела (дульный срез оружия)
    public TrailRenderer tracerPrefab; // Префаб с TrailRenderer для трейсера
    public float damageAmount = 25f; // Урон револьвера
    public float maxRange = 100f; // Максимальная дальность выстрела
    public LayerMask hitLayers; // Слои, по которым можно стрелять
    public GameObject revolverPrefab;
 
    public Transform hydeThrowPoint;
    public Transform player;

    public Animator revolverAnim;


    private InputManager im;
    
    private void Start()
    {
        im = FindObjectOfType<InputManager>();

        im.revolverActive = true;

        comboManager = FindObjectOfType<ComboManager>();
        if (tracerPrefab == null)
        {
            Debug.LogError("Префаб с TrailRenderer для трейсера не назначен!");
        }

       
    }

  

   
    void OnEnable()
    {

        im = FindObjectOfType<InputManager>();

     

        if (im.revolverActive)
        {
            revolverAnim.SetBool("RevolverActive", true);
        }

       else { revolverAnim.SetBool("RevolverActive", false); revolverAnim.SetTrigger("Throwed"); }

    }

    public void RevolverShot()
    {
        comboManager.RegisterAttack("Revolver"); // Регистрируем выстрел
      
    }

    public void ChargedRevolverShot()
    {
        comboManager.RegisterAttack("CHRevolver"); // Регистрируем выстрел

      

    }

    public void Shot1()
    {
        revolverAnim.SetTrigger("Shoot1");
        PerformRaycast(0.5f);
        AudioManager.instance.PlayOneShot(FMODEvents.Instance.JakylShooting, this.transform.position);
    }

    public void Shot2()
    {
        revolverAnim.SetTrigger("Shoot2");
        PerformRaycast(0.5f);
        AudioManager.instance.PlayOneShot(FMODEvents.Instance.JakylShooting, this.transform.position);
    }

    public void Shot3()
    {
        revolverAnim.SetTrigger("Shoot3");
        PerformRaycast(0.5f);
        AudioManager.instance.PlayOneShot(FMODEvents.Instance.JakylShooting, this.transform.position);
    }

    public void ThrowingRevolver()
    {
        if (!im.revolverActive) return; // Если револьвер уже неактивен, ничего не делаем

        im.revolverActive = false; // Делаем револьвер неактивным
        Quaternion customRotation = Quaternion.Euler(0, 0, 90); 
        Instantiate(revolverPrefab, hydeThrowPoint.position, customRotation);
        revolverAnim.SetBool("RevolverActive", false);
        revolverAnim.SetTrigger("Throwed");
    }

  

    private void PerformRaycast(float pushPower)
    {
        if (firePoint == null)
        {
            Debug.LogWarning("Точка выстрела не назначена!");
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Камера не найдена!");
            return;
        }

        Ray cameraRay = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        Vector3 hitPoint;

        if (Physics.Raycast(cameraRay.origin, cameraRay.direction, out hit, maxRange, hitLayers))
        {
            Debug.Log($"Попадание в объект: {hit.collider.name}");
            hitPoint = hit.point;

            // Создаём эффект попадания с задержкой
            StartCoroutine(ProcessHitWithDelay(hit, cameraRay.direction, pushPower));
        }
        else
        {
            hitPoint = cameraRay.origin + cameraRay.direction * maxRange;
        }

        // Запускаем визуализацию трейсера
        ShowTracer(hitPoint);
    }

    private IEnumerator ProcessHitWithDelay(RaycastHit hit, Vector3 fireDirection, float pushPower)
    {
        yield return new WaitForSeconds(0.1f); // Задержка перед обработкой попадания

        // Наносим урон
        Health health = hit.collider.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damageAmount, DamageHandler.AttackType.Other);
        }

        // Применяем силу
        Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(fireDirection * damageAmount * pushPower, ForceMode.Impulse);
        }

        // Создаем эффект попадания
        CreateHitEffect(hit.point, hit.normal);
    }



    private void ShowTracer(Vector3 hitPoint)
    {
        if (tracerPrefab == null || firePoint == null)
            return;

        TrailRenderer tracer = Instantiate(tracerPrefab, firePoint.position, Quaternion.identity);
        StartCoroutine(MoveTracer(tracer, firePoint.position, hitPoint));
    }

    private IEnumerator MoveTracer(TrailRenderer tracer, Vector3 startPoint, Vector3 endPoint)
    {
        float duration = tracer.time; // Время отображения трейла
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            tracer.transform.position = Vector3.Lerp(startPoint, endPoint, elapsedTime / duration);
            yield return null;
        }

        Destroy(tracer.gameObject);
    }

    private void CreateHitEffect(Vector3 hitPosition, Vector3 hitNormal)
    {
        Debug.DrawRay(hitPosition, hitNormal, Color.red, 2f); // Временная визуализация попадания
    }
}