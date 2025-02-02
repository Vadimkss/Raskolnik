using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HydeProjectile : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageAmount = 10f; // Урон за один удар
    public float damageRadius = 5f; // Радиус поражения
    public float damageFrequency = 0.5f; // Частота нанесения урона (в секундах)
    public int maxTargets = 5; // Максимальное количество целей, которым будет наноситься урон за цикл
    public float fixedSpeed;

    [Header("Line Renderer Settings")]
    public LineRenderer lineRenderer; // Существующий компонент LineRenderer
    public float lineDuration = 0.2f; // Время отображения линии

    private List<Health> targetsInRange = new List<Health>();
    private int currentTargetIndex = 0; // Индекс текущей цели

    [Header("MashRelated")]
    public float meshRefrashRate = 0.1f;
    public Transform positionToSpawn;
    public float meshDestroyDelay = 0.5f;

    [Header("Shader Related")]
    public Material trailMat;
    public string shaderVarRef;
    public float shaderVarRate = 0.1f;
    public float shaderVarRefreshRate = 0.05f;

 
  


    private Mesh[] cachedMeshes;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private RevolverWeapon rw;
    private InputManager im;
    private void OnEnable()
    {
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer не назначен! Пожалуйста, назначьте LineRenderer в инспекторе.");
        }
        im = FindObjectOfType<InputManager>();

        rw = FindObjectOfType<RevolverWeapon>();

        // Отключаем линию при старте
        lineRenderer.enabled = false;

        // Начать цикл нанесения урона
        StartCoroutine(DamageCycle());

        StartCoroutine(ActiveTrail(10f));


        StartCoroutine(ThrowRevolver());


    }

    private void Update()
    {
        // Обновляем список целей в радиусе
        UpdateTargetsInRange();
    }

    private IEnumerator ThrowRevolver()
    {
        Debug.Log("Ган спин");

        GameObject hydeThrowObject = GameObject.FindGameObjectWithTag("HydeThrowPoint");

        Transform hydeThrowPoint = hydeThrowObject.transform;

        Vector3 thrownDirection = hydeThrowPoint.forward.normalized;
        Debug.Log($"Направление броска: {thrownDirection}");

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("На префабе отсутствует Rigidbody!");
            yield break;
        }

        rb.velocity = thrownDirection * fixedSpeed;

        // Ждём 3 секунды
        yield return new WaitForSeconds(3f);

        StartCoroutine(HomeToPlayer(gameObject, rb));
    }

    private IEnumerator HomeToPlayer(GameObject thrownRevolver, Rigidbody rb)
    {
        while (thrownRevolver != null)
        {
            GameObject backPointObject = GameObject.FindGameObjectWithTag("RevolverBackPoint");

            Transform backPoint = backPointObject.transform;

            // Направление к текущему положению игрока
            Vector3 directionToPlayer = (backPoint.position - thrownRevolver.transform.position).normalized;
            rb.velocity = directionToPlayer * fixedSpeed * 2;

            // Проверяем дистанцию до игрока
            if (Vector3.Distance(backPoint.position, thrownRevolver.transform.position) < 1f)
            {
                Destroy(thrownRevolver); // Удаляем револьвер
                rw.revolverAnim.SetBool("RevolverActive", true);
                rw.revolverAnim.SetBool("Throwed", false);
                im.revolverActive = true;  // Снова делаем револьвер активным
                Debug.Log("Револьвер вернулся!");
                yield break; // Выходим из цикла
            }

            yield return null; // Ждем до следующего кадра
        }
    }

    private void UpdateTargetsInRange()
    {
        // Очищаем список и ищем цели в радиусе
        targetsInRange.Clear();
        Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadius);
        foreach (Collider collider in colliders)
        {
            Health health = collider.GetComponent<Health>();
            if (health != null && !targetsInRange.Contains(health))
            {
                targetsInRange.Add(health);
            }
        }
    }

    private IEnumerator DamageCycle()
    {
        while (true)
        {
            if (targetsInRange.Count > 0)
            {
                // Убедимся, что currentTargetIndex находится в допустимых пределах
                if (currentTargetIndex >= targetsInRange.Count)
                {
                    currentTargetIndex = 0; // Сбрасываем индекс, если он вышел за границы
                }

                Health target = targetsInRange[currentTargetIndex];
                if (target != null)
                {
                    // Наносим урон цели
                    target.TakeDamage(damageAmount, DamageHandler.AttackType.Other);

                    // Отображаем линию выстрела
                    DrawLineToTarget(target.transform.position);
                }

                // Переключаемся на следующую цель
                currentTargetIndex = (currentTargetIndex + 1) % targetsInRange.Count;
            }

            // Ждем перед нанесением урона следующей цели
            yield return new WaitForSeconds(damageFrequency);
        }
    }

    private void DrawLineToTarget(Vector3 targetPosition)
    {
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer не найден!");
            return;
        }

        // Включаем линию
        lineRenderer.enabled = true;

        // Запускаем анимацию линии
        StartCoroutine(AnimateLineRenderer(transform.position, targetPosition));
    }

    private IEnumerator AnimateLineRenderer(Vector3 startPoint, Vector3 endPoint)
    {
        float animationTime = 0.1f; // Время анимации линии
        float elapsedTime = 0f;

        // Анимация "появления" линии
        while (elapsedTime < animationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationTime;

            // Рассчитываем текущую точку линии
            Vector3 currentEndPoint = Vector3.Lerp(startPoint, endPoint, t);

            // Устанавливаем точки линии
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, currentEndPoint);

            yield return null;
        }

        // Устанавливаем конечную точку после завершения анимации
        lineRenderer.SetPosition(1, endPoint);

        // Эффект затухания
        StartCoroutine(FadeOutLine());
    }

    private IEnumerator FadeOutLine()
    {
        float fadeDuration = 0.2f; // Время затухания
        float elapsedTime = 0f;

        Gradient gradient = lineRenderer.colorGradient;
        GradientAlphaKey[] alphaKeys = gradient.alphaKeys;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            // Обновляем альфа-канал линии
            alphaKeys[0].alpha = alpha;
            alphaKeys[1].alpha = alpha;

            gradient.alphaKeys = alphaKeys;
            lineRenderer.colorGradient = gradient;

            yield return null;
        }

        // Выключаем линию
        lineRenderer.enabled = false;
    }
    private IEnumerator DisableLineAfterDuration()
    {
        yield return new WaitForSeconds(lineDuration);
        lineRenderer.enabled = false;
    }

    private void CacheMeshes()
    {
        if (skinnedMeshRenderers == null)
            skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        cachedMeshes = new Mesh[skinnedMeshRenderers.Length];
        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            cachedMeshes[i] = new Mesh();
            skinnedMeshRenderers[i].BakeMesh(cachedMeshes[i]);
        }
    }

    IEnumerator ActiveTrail(float timeActive)
    {
        CacheMeshes();

        while (timeActive > 0)
        {
            timeActive -= meshRefrashRate;

            for (int i = 0; i < cachedMeshes.Length; i++)
            {
                GameObject gObj = new GameObject();
                gObj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                MeshFilter mf = gObj.AddComponent<MeshFilter>();

                mf.mesh = cachedMeshes[i];
                mr.material = trailMat;

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                Destroy(gObj, meshDestroyDelay);
            }

            yield return new WaitForSeconds(meshRefrashRate);
        }

      
    }

  

    IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshRate)
    {
        float valueToAnimate = mat.GetFloat(shaderVarRef);

        while (valueToAnimate > goal)
        {
            valueToAnimate += rate;
            mat.SetFloat(shaderVarRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Рисуем радиус поражения для наглядности
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
