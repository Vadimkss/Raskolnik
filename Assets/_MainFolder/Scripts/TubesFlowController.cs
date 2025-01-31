using UnityEngine;

public class TubesFlowController : MonoBehaviour
{
    public Material tubesMaterial;  // Материал с шейдером TubesFlow

    public float flowSpeed = 1.0f;  // Скорость течения
    public Vector2 flowDirection = new Vector2(1, 0);  // Направление течения

    void Update()
    {
        // Обновление параметров шейдера
        tubesMaterial.SetFloat("_FlowSpeed", flowSpeed);
        tubesMaterial.SetVector("_FlowDirection", new Vector4(flowDirection.x, flowDirection.y, 0, 0));
    }
}
