using UnityEngine;
using UnityEngine.SceneManagement; // Для работы с управлением сценами
using DG.Tweening;
using FMOD.Studio;

public class MainMenuController : MonoBehaviour
{
    public Animator caseAnimator;  // Аниматор для коробки
    public GameObject monitor;     // Объект монитора
    public GameObject caseLight;
    public Camera mainCamera;      // Ссылка на основную камеру

    public bool isMouseOverCase = false;  // Для отслеживания, наведен ли курсор на коробку
    public bool isMouseOverMonitor = false;  // Для отслеживания, наведен ли курсор на монитор

    public bool caseFocused = false;  // Флаг, который блокирует закрытие коробки после клика
    public bool CaseCanReactToMouse = true;

    private bool canCameraRotate;

    public GameObject playMenu;

    // Параметры для поворота камеры
    public float rotationSpeed = 2.0f;     // Скорость поворота камеры
    public Vector2 rotationLimit = new Vector2(15f, 15f);  // Ограничение угла поворота камеры по оси X и Y

    private Vector3 currentRotation = Vector2.zero;  // Текущий угол поворота камеры

    public Vector3 baseRotation = new Vector2(0f, 0f);  // Базовые углы поворота камеры

   




    private void Start()
    {
        canCameraRotate = true;

        if (mainCamera == null)

        {
          mainCamera = Camera.main;  // Если камера не назначена, берем основную камеру
        }
        
        currentRotation = baseRotation;  // Инициализируем текущее вращение базовым значением
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        caseAnimator = GameObject.FindGameObjectWithTag("Case")?.GetComponent<Animator>();
        caseLight = GameObject.FindGameObjectWithTag("CaseLight");
        monitor = GameObject.FindGameObjectWithTag("Monitor");
        playMenu = GameObject.FindGameObjectWithTag("PlayMenu");
        mainCamera = Camera.main;

        if (caseAnimator == null || caseLight == null || monitor == null || playMenu == null || mainCamera == null)
        {
            Debug.LogError("Some objects are not found!");
            return;
        }

        caseFocused = false;

        caseAnimator.ResetTrigger("CaseOpen");
        caseAnimator.ResetTrigger("CaseClose");

        caseLight.SetActive(false);
        playMenu.SetActive(false);

        AudioManager.instance.PlayTimeline(FMODEvents.Instance.MenuAmbient, this.transform.position);
        caseAnimator.SetTrigger("CaseClose");
    }

    void Update()
    {
        if (!caseFocused)
        {

            HandleCameraRotation();

        }  

        // Создаем луч от камеры к позиции курсора
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Case"))
            {
                if (!isMouseOverCase && !caseFocused && CaseCanReactToMouse)
                {
                    AudioManager.instance.PlayOneShot(FMODEvents.Instance.CaseOpen, this.transform.position);
                

                caseLight.SetActive(true);
                    isMouseOverCase = true;
                    if (!caseFocused)
                    {
                       
                        caseAnimator.SetTrigger("CaseOpen");  // Запускаем анимацию открытия
                        caseAnimator.ResetTrigger("CaseClose");
                    }
                }

                // Обработка клика по коробке
                if (Input.GetMouseButtonDown(0) && !caseFocused) // Левый клик
                {
                    OnCaseClick();  // Вызываем метод при клике на кейс
                }
            }
            else if (isMouseOverCase && !caseFocused)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.Instance.CaseClose, this.transform.position);
                caseLight.SetActive(false);
                isMouseOverCase = false;
                caseAnimator.SetTrigger("CaseClose");  // Запускаем анимацию закрытия
            }

            // Проверяем, если курсор наведен на объект с тегом "Monitor"
            if (hit.collider.CompareTag("Monitor"))
            {
                if (!isMouseOverMonitor)
                {
                    isMouseOverMonitor = true;
                    Debug.Log("Mouse over Monitor");
                }

                // Обработка клика по монитору
                if (Input.GetMouseButtonDown(0)) // Левый клик
                {
                    Debug.Log("Monitor clicked!");
                    // Добавьте действие для клика по монитору здесь
                }
            }
            else
            {
                isMouseOverMonitor = false;
            }
        }
        else
        {
            // Если луч не попал ни в один объект и кейс не в фокусе
            if (isMouseOverCase && !caseFocused)
            {
                caseLight.SetActive(false);
                isMouseOverCase = false;
                caseAnimator.SetTrigger("CaseClose");  // Закрываем коробку
            }

            if (isMouseOverMonitor)
            {
                isMouseOverMonitor = false;
            }
        }

        // Проверяем, нажал ли игрок клавишу ESC для выхода из фокуса
        if (caseFocused && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitCaseFocus();  // Вызываем метод для выхода из фокуса
        }
    }

    // Метод для обработки плавного поворота камеры за курсором

    void HandleCameraRotation()
    {
        if (canCameraRotate)  // Убедимся, что камера может вращаться
        {
            // Получаем нормализованные координаты курсора (от -1 до 1)
            float mouseX = (Input.mousePosition.x / Screen.width) * 2 - 1;
            float mouseY = (Input.mousePosition.y / Screen.height) * 2 - 1;

            // Рассчитываем целевые углы поворота камеры, инвертируя оси для правильного направления
            float targetRotationX = Mathf.Clamp(-mouseY * rotationLimit.y, -rotationLimit.y, rotationLimit.y); // Инвертируем Y
            float targetRotationY = Mathf.Clamp(mouseX * rotationLimit.x, -rotationLimit.x, rotationLimit.x);  // Инвертируем X

            // Плавное изменение текущих углов поворота
            currentRotation = Vector2.Lerp(currentRotation, new Vector2(targetRotationX, targetRotationY), Time.deltaTime * rotationSpeed);

            // Получаем текущий угол по оси Z, чтобы его сохранить
            float currentZRotation = mainCamera.transform.localEulerAngles.z;

            // Применяем вращение к камере только по осям X и Y, сохраняя текущий угол Z
            mainCamera.transform.localRotation = Quaternion.Euler(baseRotation.x + currentRotation.x, baseRotation.y + currentRotation.y, currentZRotation);
        }
    }

    // Метод, который срабатывает при нажатии на кейс
    private void OnCaseClick()
    {
        Debug.Log("Case clicked!");

        DOTween.Restart("CaseFocuse");  // Перезапуск пути

        caseFocused = true;  // Устанавливаем фокус на кейс

        playMenu.SetActive(true);
    }


    public void OnPathStart()
    {
        Debug.Log("Path Started");

        CaseCanReactToMouse = false;

        canCameraRotate = false;

        AudioManager.instance.PlayOneShot(FMODEvents.Instance.DOPath, this.transform.position);


    }

    public void OnPathComplete()
    {
      

        Debug.Log("Path Completed");

        CaseCanReactToMouse = true;

        canCameraRotate = true;


    }


    // Метод для выхода из фокуса (закрытие коробки и сброс фокуса)
    private void ExitCaseFocus()
    {
        caseFocused = false;  // Сбрасываем фокус
        Debug.Log("Exit Case Focus");
        caseAnimator.SetTrigger("CaseClose");  // Запускаем анимацию закрытия
        caseLight.SetActive(false);  // Выключаем свет

        // Запуск анимации для возврата камеры
        DOTween.PlayBackwards("CaseFocuse");

        playMenu.SetActive(false);

      
    }

    public void LoadScene(string sceneName)
    {
        DOTween.KillAll(); // Остановить все анимации DOTween перед загрузкой сцены

        caseAnimator.SetTrigger("CaseClose");
        SceneManager.LoadScene(sceneName);
    }

    private void OnDestroy()
    {
        DOTween.Kill(gameObject); // Удаляет все анимации, привязанные к этому объекту
    }
}
