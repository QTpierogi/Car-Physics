using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    private LapsManager lapsManager;
    private TimeManager timeManager;
    public GameObject winPopUp;
    public GameObject losePopUp;
    public GameObject pausePopUp;
    private bool paused;

    private void Awake()
    {
        lapsManager = FindObjectOfType<LapsManager>();
        timeManager = FindObjectOfType<TimeManager>();
        paused = false;
    }

    private void Update()
    {
        if (lapsManager)
        {
            if (lapsManager.isCompleted)
            {
                winPopUp.SetActive(true);
                Time.timeScale = 0f;
            }
            else if (timeManager.IsOver)
            {
                losePopUp.SetActive(true);
                Time.timeScale = 0f;
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused)
                Resume();
            else Pause();
        }
    }

    private void Pause()
    {
        paused = true;
        Time.timeScale = 0f;
        pausePopUp.SetActive(true);
    }

    public void Resume()
    {
        paused = false;
        Time.timeScale = 1f;
        pausePopUp.SetActive(false);
    }
}
