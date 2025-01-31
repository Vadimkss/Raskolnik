using UnityEngine;

public class SphereCastVisualizer : MonoBehaviour
{
    public Transform origin; // Начальная точка SphereCast
    public float radius = 0.1f; // Радиус SphereCast
    public float maxDistance = 10f; // Максимальное расстояние SphereCast
    public Color sphereColor = Color.red; // Цвет сферы
    public Color hitColor = Color.green; // Цвет точки попадания

    private void OnDrawGizmos()
    {
        Gizmos.color = sphereColor;
        Gizmos.DrawWireSphere(origin.position, radius);

        RaycastHit hit;
        if (Physics.SphereCast(origin.position, radius, origin.forward, out hit, maxDistance))
        {
            Gizmos.color = hitColor;
            Gizmos.DrawSphere(hit.point, radius);
            Gizmos.DrawLine(origin.position, hit.point);
        }
        else
        {
            Gizmos.DrawLine(origin.position, origin.position + origin.forward * maxDistance);
        }
    }
}
