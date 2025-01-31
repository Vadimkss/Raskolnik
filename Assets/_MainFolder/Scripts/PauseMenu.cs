using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool PauseGame;
    public GameObject PauseGameMenu;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PauseGameMenu.SetActive(false);
        Time.timeScale = 1f;
        PauseGame = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PauseGame)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        PauseGameMenu.SetActive(false);
        Time.timeScale = 1f;
        PauseGame = false;

        // Скрываем курсор и блокируем его
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Pause()
    {
        PauseGameMenu.SetActive(true);
        Time.timeScale = 0f;
        PauseGame = true;

        StartCoroutine(ShowCursor());
    }

    private IEnumerator ShowCursor()
    {
        yield return new WaitForEndOfFrame();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);

        PauseGameMenu.SetActive(false);
        Time.timeScale = 1f;
        PauseGame = false;

        
        



    }
}
