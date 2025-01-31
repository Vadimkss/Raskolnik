using UnityEngine;
using DG.Tweening;
using NTC.MonoCache;

public class PlayerCam : MonoCache
{
    public float sensX;
    public float sensY;

    public float defaultSensX;
    public float defaultSensY;

    public float maxRotationSpeed = 30f; // Максимальная скорость вращения камеры

    public Transform orientation;
    public Transform camHolder;

    float xRotation;
    float yRotation;

    public Transform Player;
    private Sliding sl;

    private PauseMenu pauseMenu;

    [HideInInspector] public bool canRotate = true; // Переменная для управления вращением камеры

    void Start()
    {
        sl = Player.GetComponent<Sliding>();

        // Найдите компонент PauseMenu на сцене (например, на том же объекте, что и этот скрипт)
        pauseMenu = FindObjectOfType<PauseMenu>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        xRotation = defaultSensX;
        yRotation = defaultSensY;
    }

    protected override void Run()
    {
        // Проверка состояния паузы перед блокировкой курсора
        if (pauseMenu != null && !pauseMenu.PauseGame)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (canRotate)
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

            yRotation += mouseX;

            // Ограничиваем скорость вращения по оси Y
            float clampedMouseY = Mathf.Clamp(mouseY, -maxRotationSpeed, maxRotationSpeed);
            xRotation -= clampedMouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }

    public void RotateCameraSmoothly(float rotationAmount)
    {
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0f, 0f, rotationAmount);
        transform.DORotateQuaternion(targetRotation, 0.3f);
    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }

}
