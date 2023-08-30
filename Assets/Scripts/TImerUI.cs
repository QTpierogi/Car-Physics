using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class TImerUI : MonoBehaviour
{
    public TMP_Text timerText;
    TimeManager m_TimeManager;

    private void Start()
    {
        m_TimeManager = FindObjectOfType<TimeManager>();

        if (m_TimeManager.IsFinite)
        {
            timerText.text = "";
        }
    }

    void Update()
    {
        if (m_TimeManager.IsFinite)
        {
            timerText.gameObject.SetActive(true);
            int timeRemaining = (int)Math.Ceiling(m_TimeManager.TimeRemaining);
            timerText.text = "Time left: " + string.Format("{0}:{1:00}", timeRemaining / 60, timeRemaining % 60);
        }
        else
        {
            timerText.gameObject.SetActive(false);
        }
    }
}
