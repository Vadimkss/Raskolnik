using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class WeaponBase : MonoBehaviour, IShootable
{
    protected WeaponConfigSO config;
    protected Camera mainCamera;
    protected float nextFireTime;
    protected List<Transform> shootPoints = new List<Transform>();
    protected float currentSpread;
    protected bool isAltFireMode;
    protected int burstShotsRemaining;
    protected bool isShooting;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform weaponParent; // Для отслеживания поворота оружия
    private WeaponManager WeaponManager;
    private Transform shootPoint;
    private Vector3 shootVector;
    private LineRenderer shootLine;  // Добавляем переменную для LineRenderer

    public virtual void SetupWeapon(WeaponConfigSO config)
    {
        this.config = config;
        InitializeShootPoints();
        ResetSpread();
      
    }

    protected virtual void Awake()
    {
        mainCamera = Camera.main;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        weaponParent = transform.parent;
        // Убрали CreateShootLine() отсюда
    }
    public void ShowTracer(Vector3 hitPoint)
    {
        if (config.tracerPrefab == null || shootPoint == null)
            return;

        // Рассчитываем направление выстрела с учетом разброса
        Vector3 shootDirection = GetShootDirection(shootPoint);

        // Корректируем конечную точку с учетом разброса
        Vector3 tracerEndPoint = shootPoint.position + shootDirection * config.range;

        // Создаем и перемещаем трассер
        GameObject tracerObject = Instantiate(config.tracerPrefab, shootPoint.position, Quaternion.identity);
        TrailRenderer tracer = tracerObject.GetComponent<TrailRenderer>();
        if (tracer != null)
        {
            StartCoroutine(MoveTracer(tracer, shootPoint.position, tracerEndPoint));
        }
        else
        {
            Debug.LogError("Prefab does not contain a TrailRenderer component!");
        }
    }

    public IEnumerator MoveTracer(TrailRenderer tracer, Vector3 startPoint, Vector3 endPoint)
    {
        float distance = Vector3.Distance(startPoint, endPoint);
        float duration = distance / config.tracerSpeed; // Вычисляем время на основе расстояния и скорости
        float elapsedTime = 0f;

        // Устанавливаем время жизни трейла
        tracer.time = config.tracerLifetime;

        while (elapsedTime < duration)
        {
            if (tracer == null) // Проверка на случай уничтожения объекта
                yield break;

            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration;

            // Используем плавную интерполяцию для более естественного движения
            float smoothProgress = Mathf.SmoothStep(0f, 1f, normalizedTime);
            tracer.transform.position = Vector3.Lerp(startPoint, endPoint, smoothProgress);

            yield return null;
        }

        // Плавное затухание трейла перед уничтожением
        if (tracer != null)
        {
            float fadeTime = tracer.time;
            yield return new WaitForSeconds(fadeTime);
            Destroy(tracer.gameObject);
        }
    }


private void OnEnable()
    {
        GameObject foundShootPoint = GameObject.FindGameObjectWithTag("ShootPoint");
        if (foundShootPoint != null)
        {
            shootPoint = foundShootPoint.transform;
        }
        else
        {
            Debug.LogError("ShootPoint с тегом не найден!");
        }

       
    }

    public void SetWeaponManager(WeaponManager weaponManager)
    {
        WeaponManager = weaponManager;
    }

    // Метод для вызова события
    protected void FireWeaponEvent()
    {
        if (WeaponManager != null)
        {
            WeaponManager.OnWeaponFire?.Invoke();
        }
        else
        {
            Debug.LogWarning("WeaponManager не установлен для этого оружия!");


        }
    }
  

    protected virtual void InitializeShootPoints()
    {
        // Очищаем существующие точки
        foreach (Transform point in shootPoints)
        {
            if (point != null) Destroy(point.gameObject);
        }
        shootPoints.Clear();

        // Создаем новые точки стрельбы
        for (int i = 0; i < config.shootPointOffsets.Count; i++)
        {
            GameObject spObject = new GameObject($"{config.shootPointBaseName}_{i}");
            Transform point = spObject.transform;
            point.SetParent(transform);
            point.localPosition = config.shootPointOffsets[i];
            shootPoints.Add(point);
        }
    }

    protected float lastBurstShotTime; // Время последнего выстрела в очереди

    public virtual void ToggleAlternativeFire()
    {
        if (config.hasAlternativeFire)
        {
            isAltFireMode = !isAltFireMode;
            ResetSpread();
        }
    }

    protected virtual void Update()
    {
        UpdateSpread();

        if (isShooting && CanShoot()) // Добавили проверку CanShoot() здесь
        {
            switch (config.fireMode)
            {
                case FireMode.Auto:
                    if (Time.time >= nextFireTime)
                    {
                        Shoot();
                        UpdateNextFireTime();
                        FireWeaponEvent();

                    }
                    break;

                case FireMode.Burst:
                    if (burstShotsRemaining > 0)
                    {
                        if (lastBurstShotTime < 0 || Time.time >= nextFireTime)
                        {
                            Shoot();
                            burstShotsRemaining--;
                            lastBurstShotTime = Time.time;
                            UpdateNextFireTime();
                            FireWeaponEvent();
                        }
                    }
                    break;

                case FireMode.Semi:
                    if (Time.time >= nextFireTime)
                    {
                        Shoot();
                        UpdateNextFireTime();
                        isShooting = false; // Останавливаем стрельбу после одного выстрела
                        FireWeaponEvent();
                    }
                    break;
            }

        
        }
    }

    public virtual void StartShooting()
    {
        if (!isShooting) // Добавляем проверку, чтобы избежать повторной инициализации при удержании
        {
            isShooting = true;
            if (config.fireMode == FireMode.Burst)
            {
                burstShotsRemaining = config.burstSize;
                lastBurstShotTime = -1f;
            }
            nextFireTime = Time.time; // Позволяем стрелять сразу при нажатии
        }
    }

    public virtual void StopShooting()
    {
        isShooting = false;
        if (config.fireMode == FireMode.Burst)
        {
            burstShotsRemaining = 0; // Сбрасываем оставшиеся выстрелы в очереди
        }
    }
    public abstract void Shoot();

    protected bool CanShoot()
    {
        float currentFireRate = isAltFireMode ? config.alternativeFireRate : config.fireRate;
        return Time.time >= nextFireTime && currentFireRate > 0;
    }

    protected void UpdateNextFireTime()
    {
        float currentFireRate = isAltFireMode ? config.alternativeFireRate : config.fireRate;
        if (currentFireRate > 0) // Защита от деления на ноль
        {
            nextFireTime = Time.time + 1f / currentFireRate;
        }
    }

  

    protected void ResetSpread()
    {
        SpreadConfig spreadConfig = isAltFireMode ?
            config.alternativeSpreadConfig : config.spreadConfig;
        currentSpread = spreadConfig.baseSpread;
    }

    protected Vector3 GetShootDirection(Transform shootPoint)
    {
        // Получаем луч из центра экрана
        Ray cameraRay = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // Находим точку прицеливания
        RaycastHit hit;
        Vector3 targetPoint;
        float distance;

        if (Physics.Raycast(cameraRay, out hit, config.range, config.targetLayers))
        {
            targetPoint = hit.point;
            distance = Vector3.Distance(shootPoint.position, hit.point);
        }
        else
        {
            // Если луч ни во что не попал, берём точку на определённом расстоянии
            targetPoint = cameraRay.origin + cameraRay.direction * config.range;
            distance = config.range;
        }

        // Базовое направление теперь всегда идёт из точки выстрела в точку прицеливания
        Vector3 baseDirection = (targetPoint - shootPoint.position).normalized;

        // Рассчитываем силу разброса в зависимости от дистанции
        // Очень близкие цели получают минимальный разброс
        float distanceMultiplier = Mathf.Clamp01(distance / 2f); // Начинаем увеличивать разброс после 2 единиц
        float effectiveSpread = currentSpread * distanceMultiplier;

        // Применяем разброс
        if (effectiveSpread > 0)
        {
            // Создаём случайное отклонение
            float spreadAngleX = Random.Range(-effectiveSpread, effectiveSpread);
            float spreadAngleY = Random.Range(-effectiveSpread, effectiveSpread);

            // Создаём локальную систему координат для разброса
            Vector3 right = Vector3.Cross(baseDirection, Vector3.up).normalized;
            Vector3 up = Vector3.Cross(right, baseDirection).normalized;

            // Применяем разброс в локальной системе координат
            Quaternion rotation = Quaternion.AngleAxis(spreadAngleX, up) * Quaternion.AngleAxis(spreadAngleY, right);
            return rotation * baseDirection;
        }

        return baseDirection;
    }

    // Обновляем метод UpdateSpread для более заметного эффекта
    protected virtual void UpdateSpread()
    {
        SpreadConfig spreadConfig = isAltFireMode ?
            config.alternativeSpreadConfig : config.spreadConfig;

        if (isShooting)
        {
            // Увеличиваем разброс при стрельбе
            currentSpread = Mathf.Min(
                currentSpread + spreadConfig.spreadIncrease * Time.deltaTime,
                spreadConfig.maxSpread
            );
        }
        else
        {
            // Уменьшаем разброс когда не стреляем
            currentSpread = Mathf.Max(
                currentSpread - spreadConfig.spreadDecrease * Time.deltaTime,
                spreadConfig.baseSpread
            );
        }

      
    }

    // Добавим метод для визуализации конуса разброса в редакторе
    protected virtual void OnDrawGizmos()
    {
        if (!Application.isPlaying || mainCamera == null) return;

        foreach (Transform point in shootPoints)
        {
            if (point != null)
            {
                // Визуализация точки выстрела
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(point.position, 0.02f);

                // Визуализация конуса разброса
                Vector3 forward = (mainCamera.transform.forward * 5f);
                float spreadRadius = Mathf.Tan(currentSpread * Mathf.Deg2Rad) * 5f;

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(point.position + forward, spreadRadius);

                // Рисуем линии конуса
                Gizmos.DrawLine(point.position, point.position + forward + Vector3.up * spreadRadius);
                Gizmos.DrawLine(point.position, point.position + forward - Vector3.up * spreadRadius);
                Gizmos.DrawLine(point.position, point.position + forward + Vector3.right * spreadRadius);
                Gizmos.DrawLine(point.position, point.position + forward - Vector3.right * spreadRadius);
            }
        }
    }

    public IEnumerator MuzzleFlash(float destroyTimer)
    {
        GameObject muzzleFlash = Instantiate(config.muzzleFlashPrefab, shootPoint.position, shootPoint.rotation, shootPoint.parent );
        yield return new WaitForSeconds( destroyTimer );
        Destroy(muzzleFlash);

    }

  



   
}