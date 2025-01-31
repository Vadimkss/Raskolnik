using UnityEngine;

public class WeaponCamera : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask weaponLayer;
   

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Исключаем UI слой из маски рендеринга
        
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            mainCamera.cullingMask = weaponLayer;
        }
    }
}
