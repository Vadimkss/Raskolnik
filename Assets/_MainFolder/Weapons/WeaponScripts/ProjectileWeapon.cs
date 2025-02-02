using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWeapon : WeaponBase
{
    public override void Shoot()
    {
        foreach (Transform shootPoint in shootPoints)
        {
            for (int i = 0; i < config.projectilesPerShot; i++)
            {
                Vector3 shootDirection = GetShootDirection(shootPoint);
                GameObject projectileObj = Instantiate(config.projectilePrefab,
                    shootPoint.position,
                    Quaternion.LookRotation(shootDirection));

                // Добавляем явную установку масштаба
                projectileObj.transform.localScale = Vector3.one * 0.2f; // Или другой подходящий размер

                Projectile projectile = projectileObj.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Initialize(
                        config.damage,
                        config.projectileSpeed,
                        config.attackType,
                        config
                    );
                }
                else
                {
                    Debug.LogError("Projectile component not found on prefab!");
                    Destroy(projectileObj);
                    return;
                }
            }
        }

      
        StartCoroutine(MuzzleFlash(0.5f));
    }
}