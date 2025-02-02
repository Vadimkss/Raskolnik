using UnityEngine;
using UnityEngine.Events;

public class WeaponManager : MonoBehaviour
{
    public UnityEvent OnWeaponFire;
    [SerializeField] private WeaponConfigSO weaponConfig;
    private IShootable currentWeapon; 
    private WeaponBase weaponBase;
    

    private void Start()
    {
        if (OnWeaponFire == null || OnWeaponFire.GetPersistentEventCount() == 0)
        {
            Debug.LogWarning("На событие OnWeaponFire никто не подписан.");
        }

        if (weaponConfig == null)
        {
            Debug.LogError("WeaponConfig не назначен! Пожалуйста, назначьте конфигурацию оружия в инспекторе.");
            return;
        }
        CreateWeapon();

        weaponBase = GetComponent<WeaponBase>();
    }

    private void CreateWeapon()
    {
        GameObject weaponObj = new GameObject($"Weapon_{weaponConfig.weaponName}");
        weaponObj.transform.parent = transform;
        weaponObj.transform.localPosition = Vector3.zero;

        // Сначала получаем компонент
        if (weaponConfig.shootType == ShootType.Projectile)
        {
            weaponBase = weaponObj.AddComponent<ProjectileWeapon>();
            currentWeapon = weaponBase;
        }
        else
        {
            weaponBase = weaponObj.AddComponent<RaycastWeapon>();
            currentWeapon = weaponBase;
        }

        // Затем устанавливаем WeaponManager
        weaponBase.SetWeaponManager(this);

        // И только потом настраиваем оружие
        currentWeapon.SetupWeapon(weaponConfig);

        if (weaponConfig.weaponModelPrefab != null)
        {
            GameObject model = Instantiate(weaponConfig.weaponModelPrefab, weaponObj.transform);
            model.transform.localPosition = Vector3.zero;
        }
    }


    private void Update()
    {
        if (currentWeapon == null) return;

        // Обработка начала стрельбы
        if (Input.GetButtonDown("Fire1"))
        {
            currentWeapon.StartShooting();
        }

        // Обработка окончания стрельбы
        if (Input.GetButtonUp("Fire1"))
        {
            currentWeapon.StopShooting();
        }

        // Переключение альтернативного режима стрельбы
        if (Input.GetKeyDown(KeyCode.B))
        {
            currentWeapon.ToggleAlternativeFire();
        }
    }

    public void DebugMessage(string msg)
    {
        Debug.Log(msg);
    }
}