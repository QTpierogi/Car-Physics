using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LapsManager : MonoBehaviour
{
    public TMP_Text lapCounter;
    public List<LapPoint> lapPoints;
    public int currentLap;
    public int lapsToFinish;
    public int startingTimeInSecs;
    public int timePerLapFinished;
    public bool isCompleted { get; private set; }

    public static Action<LapPoint> OnEnterLapPoint;
    public static Action<LapPoint> OnRegisterPoint;

    private TimeManager timeManager; 

    private void Awake()
    {
        OnEnterLapPoint += MovedThroughPoint;
        OnRegisterPoint += RegisterPoint;
        currentLap = 1;
        timeManager = FindObjectOfType<TimeManager>();
    }

    IEnumerator Start()
    {
        TimeManager.OnSetTime(startingTimeInSecs, true);
        yield return new WaitForEndOfFrame();
    }

    public int NumberOfActivePickupsRemaining()
    {
        int total = 0;
        for (int i = 0; i < lapPoints.Count; i++)
        {
            if (lapPoints[i].active) total++;
        }

        return total;
    }

    private void MovedThroughPoint(LapPoint point)
    {
        point.active = false;
        Debug.Log(NumberOfActivePickupsRemaining() + " points left");
        if (!point.finishLap) return;

        if (!point.lapOverNextPass)
        {
            //TimeDisplay.OnUpdateLap();
            point.lapOverNextPass = true;
            point.active = true;
            return;
        }
        Debug.Log("Finish reached");
        if (NumberOfActivePickupsRemaining() != 0) return;

        ReachCheckpoint(0);
        ResetPickups();
    }

    protected void ReachCheckpoint(int remaining)
    {
        if (currentLap == lapsToFinish)
        {
            isCompleted = true;
            timeManager.IsOver = true;
        }

        if (isCompleted)
            return;

        currentLap++;
        lapCounter.text = currentLap + "/" + lapsToFinish;
        TimeManager.OnAdjustTime(timePerLapFinished);
    }

    public void ResetPickups()
    {
        for (int i = 0; i < lapPoints.Count; i++)
        {
            lapPoints[i].active = true;
        }
    }

    public void RegisterPoint(LapPoint pickup)
    {
        lapPoints.Add(pickup);
    }
}


