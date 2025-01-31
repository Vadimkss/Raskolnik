using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using FMODUnity;
using FMOD.Studio;
using FlowCanvas;
using System;

public class SmartRevolver : MonoBehaviour
{
    public Camera mainCamera; // Камера для определения положения целей
    public LayerMask targetLayer; // Слой, на котором находятся цели
    public float focusRadius = 10f; // Радиус поиска целей
    public float aimWindowSize = 200f; // Размер окна прицеливания на экране
    public LineRenderer lineRendererPrefab; // Префаб линии для отображения связи между револьвером и целью
    public Transform firePoint; // Точка, откуда вылетают снаряды

    public float animationDuration = 0.5f; // Длительность анимации
    public GameObject homingProjectilePrefab; // Префаб снаряда с управлением
    public float timeBetweenShots = 0.5f; // Время между выстрелами
    public int maxTargets = 5; // Максимальное количество целей
    public AnimationCurve lineCurve; // Кривая для отображения линии
    public Animator HydeAnimator; // Аниматор для анимаций револьвера
    public GameObject headColliderVisualizerPrefab; // Префаб для визуализации коллайдера головы цели
    public float focusTime = 1.5f; // Время, необходимое для фокусировки на цели
    public float focusCooldown = 2f; // Время перезарядки между фокусировками

    private List<GameObject> lockedTargets = new List<GameObject>(); // Список заблокированных целей
    private List<GameObject> currentTargets = new List<GameObject>(); // Список текущих целей
    private Dictionary<GameObject, LineRenderer> targetLines = new Dictionary<GameObject, LineRenderer>(); // Словарь для хранения линий, соединяющих револьвер и цели
    private bool isFocusing = false; // Флаг, указывающий, фокусируется ли револьвер
    private bool canFocusNextTarget = true; // Флаг, разрешающий фокусировку на следующей цели
    private bool canStartFocus = true; // Флаг, разрешающий начало фокусировки
    private Dictionary<GameObject, float> focusProgress = new Dictionary<GameObject, float>(); // Словарь для отслеживания прогресса фокусировки на каждой цели

    public FlowScriptController canvas; // Контроллер FlowCanvas для взаимодействия с графом

    private AudioManager audioManager; // Менеджер звука

    public GameObject crosshair;
    public GameObject TargetField;

    void Start()
    {
        // Инициализация при старте
    }

    void Update()
    {
        if (Input.GetMouseButton(1) && canStartFocus)
        {
            if (!isFocusing)
            {
                isFocusing = true;
                canvas.SendEvent("SmartGun"); // Отправка события в FlowCanvas
                DOTween.Restart("SmartGun"); // Перезапуск анимации для умного оружия

                crosshair.SetActive(!isActiveAndEnabled);

                AudioManager.instance.PlayOneShot(FMODEvents.Instance.Focusing, this.transform.position);

            }
            UpdateFocus(); // Обновление фокусировки на целях
        }
        else if (isFocusing && Input.GetMouseButtonUp(1))
        {
            StartCoroutine(FireProjectiles()); // Запуск корутины для стрельбы по целям
            StartCoroutine(FocusCooldown()); // Запуск корутины для перезарядки фокусировки
            ClearFocus(); // Очистка фокусировки
            isFocusing = false;
            DOTween.PlayBackwards("SmartGun"); // Отмена анимации

            crosshair.SetActive(isActiveAndEnabled);

            AudioManager.instance.PlayOneShot(FMODEvents.Instance.Focusing, this.transform.position);

        }
    }

    IEnumerator FocusCooldown()
    {
        canStartFocus = false; // Отключение возможности фокусировки
        yield return new WaitForSeconds(focusCooldown); // Ожидание времени перезарядки
        canStartFocus = true; // Включение возможности фокусировки
    }

    void UpdateFocus()
    {
        // Поиск целей в радиусе фокусировки
        Collider[] hits = Physics.OverlapSphere(transform.position, focusRadius, targetLayer);
        List<GameObject> potentialTargets = new List<GameObject>();

        foreach (Collider hit in hits)
        {
            GameObject target = hit.gameObject;
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(target.transform.position);

            if (IsPointWithinAimWindow(screenPoint))
            {
                potentialTargets.Add(target); // Добавление цели в список потенциальных целей
            }
        }

        // Сортировка целей по близости к центру экрана
        potentialTargets.Sort((a, b) =>
        {
            Vector3 screenPointA = mainCamera.WorldToScreenPoint(a.transform.position);
            Vector3 screenPointB = mainCamera.WorldToScreenPoint(b.transform.position);

            float distanceA = Vector2.Distance(new Vector2(screenPointA.x, screenPointA.y), new Vector2(Screen.width / 2, Screen.height / 2));
            float distanceB = Vector2.Distance(new Vector2(screenPointB.x, screenPointB.y), new Vector2(Screen.width / 2, Screen.height / 2));

            return distanceA.CompareTo(distanceB);
        });

        if (canFocusNextTarget && lockedTargets.Count < maxTargets)
        {
            StartCoroutine(FocusOnTarget(potentialTargets)); // Начало фокусировки на цели
        }

        // Обновление линий связи с заблокированными целями
        foreach (var target in lockedTargets)
        {
            if (!targetLines.ContainsKey(target))
            {
                LineRenderer line = Instantiate(lineRendererPrefab);
                targetLines[target] = line;
            }
            UpdateLineRendererWithCurve(targetLines[target], firePoint.position, target); // Обновление линии с кривой
        }
    }

    IEnumerator FocusOnTarget(List<GameObject> potentialTargets)
    {
        canFocusNextTarget = false; // Отключение возможности фокусировки на новой цели

        foreach (var target in potentialTargets)
        {
            if (!lockedTargets.Contains(target))
            {
                if (!focusProgress.ContainsKey(target))
                {
                    focusProgress[target] = 0f;
                }

                // Постепенное увеличение прогресса фокусировки
                while (focusProgress[target] < focusTime)
                {
                    if (focusProgress.TryGetValue(target, out float progress))
                    {
                        // Если цель была удалена из словаря, прервать фокусировку
                        break;
                    }

                    focusProgress[target] += Time.deltaTime;
                    yield return null;
                }

                lockedTargets.Add(target); // Добавление цели в заблокированные

                // Визуализация коллайдера головы цели
                if (headColliderVisualizerPrefab != null && target.GetComponentInChildren<SphereCollider>())
                {
                    SphereCollider sphereCollider = target.GetComponent<SphereCollider>();

                    if (sphereCollider != null)
                    {
                        Vector3 colliderCenter = sphereCollider.bounds.center;
                        GameObject headColliderVisualizer = Instantiate(headColliderVisualizerPrefab, colliderCenter, Quaternion.identity, target.transform);
                        headColliderVisualizer.name = headColliderVisualizerPrefab.name;

                        headColliderVisualizer.transform.localPosition = target.transform.InverseTransformPoint(colliderCenter);
                    }
                }

                break; // Прерывание цикла после фокусировки на одной цели
            }
        }

        yield return new WaitForSeconds(focusTime); // Ожидание окончания фокусировки
        canFocusNextTarget = true; // Включение возможности фокусировки на новой цели
    }

    void ClearFocus()
    {
        foreach (var line in targetLines.Values)
        {
            Destroy(line.gameObject); // Удаление линий
        }
        targetLines.Clear();
        focusProgress.Clear();
        lockedTargets.Clear(); // Очистка списка заблокированных целей
    }

    IEnumerator FireProjectiles()
    {
        List<GameObject> targetsToShoot = new List<GameObject>(lockedTargets);

        foreach (var target in targetsToShoot)
        {
            if (target != null)
            {
                FireProjectile(target); // Запуск снаряда по цели
                yield return new WaitForSeconds(timeBetweenShots); // Ожидание между выстрелами

                Transform headVisualizer = target.transform.Find(headColliderVisualizerPrefab.name);
                if (headVisualizer != null)
                {
                    Destroy(headVisualizer.gameObject); // Удаление визуализатора коллайдера головы
                }
            }
        }

        ClearFocus(); // Очистка фокусировки после стрельбы
    }
    void FireProjectile(GameObject target)
    {
        HydeAnimator.SetTrigger("SH");
        HydeAnimator.SetInteger("SHIndex", 4);

        canvas.SendEvent("Shoot");

     
        GameObject projectile = Instantiate(homingProjectilePrefab, firePoint.position, firePoint.rotation);
        HomingProjectile homingProjectile = projectile.GetComponent<HomingProjectile>();

        if (homingProjectile != null)
        {
            SphereCollider headCollider = target.GetComponentInChildren<SphereCollider>();
            if (headCollider != null)
            {
                // Используем headCollider для установки целевой трансформации
                homingProjectile.target = headCollider.gameObject;
                homingProjectile.targetHeadCollider = headCollider; // Устанавливаем целевой коллайдер головы
            }
            else
            {
                // В случае отсутствия коллайдера головы, используем целевой объект и его трансформ
                homingProjectile.target = target;
                homingProjectile.targetHeadCollider = target.GetComponentInChildren<SphereCollider>(); // Устанавливаем коллайдер головы, если он есть
            }
        }
    }
    bool IsPointWithinAimWindow(Vector3 point)
    {
        // Проверка, находится ли точка в пределах окна прицеливания
        return point.x > (Screen.width / 2 - aimWindowSize / 2) &&
               point.x < (Screen.width / 2 + aimWindowSize / 2) &&
               point.y > (Screen.height / 2 - aimWindowSize / 2) &&
               point.y < (Screen.height / 2 + aimWindowSize / 2);
    }

    void UpdateLineRendererWithCurve(LineRenderer line, Vector3 start, GameObject target)
    {
        // Получаем центр коллайдера цели
        Vector3 end = target.GetComponent<Collider>().bounds.center;

        // Определяем среднюю точку между началом и концом для создания кривой
        Vector3 middle = (start + end) / 2;

        // Добавляем смещение вверх для создания кривизны
        middle += Vector3.up * lineCurve.Evaluate(Vector3.Distance(start, end));

        // Задаем количество точек для линии
        line.positionCount = 3; // Начало, середина, конец

        // Устанавливаем позиции для линии
        line.SetPosition(0, start); // Начальная точка (из позиции выстрела)
        line.SetPosition(1, middle); // Контрольная точка для создания кривой
        line.SetPosition(2, end); // Конечная точка (позиция врага)
    }
}
