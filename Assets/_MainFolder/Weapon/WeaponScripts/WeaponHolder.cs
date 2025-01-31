using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Tooltip("Список оружий, которые можно переключать.")]
    public List<GameObject> weapons = new List<GameObject>();

    private int currentWeaponIndex = 0;

    void Start()
    {
        // Делаем только первое оружие активным по умолчанию
        SelectWeapon(currentWeaponIndex);
    }

    void Update()
    {
        HandleWeaponSwitch();
    }

    // Обрабатываем нажатие цифр для смены оружия
    void HandleWeaponSwitch()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectWeapon(i);
                break;
            }
        }
    }

    // Метод для выбора оружия по индексу
    void SelectWeapon(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= weapons.Count) return;

        // Отключаем все оружия
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].SetActive(i == weaponIndex);
        }

        // Устанавливаем новое текущее оружие
        currentWeaponIndex = weaponIndex;
    }
}
