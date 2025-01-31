using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastWeapon : WeaponBase
{
    public override void Shoot()
    {
        if (!CanShoot()) return;

        int shotsCount = isAltFireMode ?
            config.alternativeProjectilesPerShot : config.projectilesPerShot;

        foreach (Transform shootPoint in shootPoints)
        {
            for (int i = 0; i < shotsCount; i++)
            {
                Vector3 spreadDirection = GetShootDirection(shootPoint);
                PerformShot(shootPoint, spreadDirection);
            }
        }

        UpdateNextFireTime();
    }

    protected virtual void PerformShot(Transform shootPoint, Vector3 spreadDirection)
    {
        Vector3 startPoint = shootPoint.position;
        Vector3 hitPoint;
        bool hitTarget = false;

        // Проверяем, не находимся ли мы внутри коллайдера
        Collider[] overlappingColliders = Physics.OverlapSphere(startPoint, 0.1f, config.targetLayers);
        if (overlappingColliders.Length > 0)
        {
            // Если мы внутри врага, применяем урон напрямую
            hitPoint = startPoint + spreadDirection * 0.1f; // Минимальная дистанция для трассера

            foreach (var collider in overlappingColliders)
            {
                if (collider.TryGetComponent<IDamageable>(out var target))
                {
                    float damage = isAltFireMode ? config.alternativeDamage : config.damage;
                    target.TakeDamage(damage, config.attackType);
                    hitTarget = true;
                }
            }
        }
        else
        {
            // Стандартный рейкаст, если мы не внутри коллайдера
            Ray ray = new Ray(startPoint, spreadDirection);
            if (Physics.Raycast(ray, out RaycastHit hit, config.range, config.targetLayers))
            {
                hitPoint = hit.point;
                if (hit.collider.TryGetComponent<IDamageable>(out var target))
                {
                    float damage = isAltFireMode ? config.alternativeDamage : config.damage;
                    if (config.damageDecreaseByDistance)
                    {
                        damage = config.damage - (hit.distance/config.range) * config.damage;
                        target.TakeDamage(damage, config.attackType);
                    }

                    else
                    {
                        damage = isAltFireMode ? config.alternativeDamage : config.damage;
                        target.TakeDamage(damage, config.attackType);
                       
                    }

                    hitTarget = true;
                }
            }
            else
            {
                hitPoint = startPoint + spreadDirection * config.range;
            }
        }

        // Показываем визуальные эффекты
        ShowTracerFromShootPoint(startPoint, hitPoint);
        StartCoroutine(MuzzleFlash(0.5f));

        // Отладочная визуализация
        if (Debug.isDebugBuild)
        {
            Color debugColor = hitTarget ? Color.red : Color.yellow;
            Debug.DrawLine(startPoint, hitPoint, debugColor, 1f);
            Debug.Log($"Shot from {startPoint} to {hitPoint}, Hit target: {hitTarget}");
        }
    }

        private void ShowTracerFromShootPoint(Vector3 startPoint, Vector3 endPoint)
    {
        if (config.tracerPrefab == null)
            return;

        GameObject tracerObject = Instantiate(config.tracerPrefab, startPoint, Quaternion.identity);
        TrailRenderer tracer = tracerObject.GetComponent<TrailRenderer>();

        if (tracer != null)
        {
            tracer.widthMultiplier = 0.5f;
            tracer.minVertexDistance = 0.1f;
            StartCoroutine(MoveTracer(tracer, startPoint, endPoint));

            if (Debug.isDebugBuild)
            {
                float distance = Vector3.Distance(startPoint, endPoint);
                Debug.Log($"Tracer: Start={startPoint}, End={endPoint}, Distance={distance}");
            }
        }
    }
}
    