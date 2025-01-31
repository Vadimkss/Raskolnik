using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void UnloadCurrentScene()
    {
        // Получаем имя текущей активной сцены
        string currentSceneName = SceneManager.GetActiveScene().name;
        // Выгружаем текущую сцену
        SceneManager.UnloadSceneAsync(currentSceneName);
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);  // Не уничтожать объект при загрузке новой сцены
    }
}