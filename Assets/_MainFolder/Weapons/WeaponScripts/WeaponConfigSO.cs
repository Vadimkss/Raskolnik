using FlowCanvas.Nodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEngine.Events;


[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon Configuration")]
public class WeaponConfigSO : ScriptableObject
{
    [Header("Основные параметры")]
    public string weaponName = "Default Weapon";
    public ShootType shootType;
    public FireMode fireMode;

    [Header("Параметры стрельбы")]
    public float damage = 10f;
    public float fireRate = 1f;
    [Tooltip("снижение урона с расстоянием")]
    public bool damageDecreaseByDistance = false;
    [Tooltip("Количество снарядов за выстрел")]
    public int projectilesPerShot = 1;
    [Tooltip("Размер очереди (для режима Burst)")]
    public int burstSize = 3;
    public DamageHandler.AttackType attackType;

    [Header("Точки стрельбы")]
    public List<Vector3> shootPointOffsets = new List<Vector3>() { Vector3.forward };
    public string shootPointBaseName = "ShootPoint";

    [Header("Разброс")]
    public SpreadConfig spreadConfig;

  

    [Header("Альтернативный режим стрельбы")]
    public bool hasAlternativeFire = false;
    public ShootType alternativeShootType = ShootType.Raycast;
    public float alternativeDamage = 10f;
    public float alternativeFireRate = 1f;
    public int alternativeProjectilesPerShot = 1;
    public SpreadConfig alternativeSpreadConfig;

    [Header("Параметры дальности и цели")]
    public float projectileSpeed = 20f;
    public float range = 100f;
    public LayerMask targetLayers;

    [Header("Префабы и эффекты")]
    public GameObject projectilePrefab;
    public GameObject weaponModelPrefab;
    public GameObject muzzleFlashPrefab;
    public AudioClip shootSound;
    public LineRenderer shootLine;

    [Header("Line Renderer Settings")]
    public GameObject tracerPrefab;

    [Header("Tracer Settings")]
    [Tooltip("Скорость движения трассера в единицах в секунду")]
    public float tracerSpeed = 100f;

    [Tooltip("Время жизни трассера в секундах")]
    public float tracerLifetime = 0.5f;

}

[System.Serializable]
public class SpreadConfig
{
    [Tooltip("Базовый разброс в градусах")]
    public float baseSpread = 1f;
    [Tooltip("Максимальный разброс при стрельбе")]
    public float maxSpread = 5f;
    [Tooltip("Скорость увеличения разброса")]
    public float spreadIncrease = 0.5f;
    [Tooltip("Скорость уменьшения разброса")]
    public float spreadDecrease = 1f;
}








