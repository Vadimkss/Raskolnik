using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBullets : MonoBehaviour
{
    public GameObject bulletPrefab; // Префаб пули
    public int numberOfBullets = 5; // Количество пуль
    public float bulletSpeed = 15f; // Скорость пули
    public float spreadRadius = 0.8f; // Радиус рассеивания пуль
    public float maxDistance = 15f; // Максимальная дистанция пролета пули

    private void Update()
    {
        // Проверяем нажатие кнопки мыши
        if (Input.GetMouseButtonDown(0))
        {
            // Выпускаем пули
            Shoot();
        }
    }

    private void Shoot()
    {
        // Создаем несколько пуль
        for (int i = 0; i < numberOfBullets; i++)
        {
            // Создаем экземпляр пули
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

            // Вычисляем случайные углы смещения для разброса по горизонтали и вертикали
            float spreadAngleX = Random.Range(-spreadRadius, spreadRadius);
            float spreadAngleY = Random.Range(-spreadRadius, spreadRadius);

            // Передаем пули направление с учетом разброса
            Vector3 bulletDirection = Quaternion.Euler(spreadAngleY, spreadAngleX, 0) * transform.forward;

            // Передаем пули направление и скорость
            bullet.GetComponent<Rigidbody>().velocity = bulletDirection * bulletSpeed;

            // Запускаем таймер для удаления пули после пролета максимального расстояния
            StartCoroutine(DestroyBulletAfterTime(bullet, maxDistance / bulletSpeed));
        }
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(bullet);
    }
}
