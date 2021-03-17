using System;
using System.Net.Mime;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

using Debug = UnityEngine.Debug;
public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)){
            
            if (GameIsPaused)
            {
                Resume();
            }
            else {
                Pause();
            }
        }
    }

        void Pause() 
        {
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f; //freezes global time
            GameIsPaused = true;
        }

        public void Resume()
        {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1.0f; //freezes global time
            GameIsPaused = false;
        }

    public void LoadMenu() 
    {
        Time.timeScale = 1f;
        Debug.Log("Loading Menu"); 
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame() 
    {
        Debug.Log("Quitting Game... ");
        Application.Quit();
    }
}
