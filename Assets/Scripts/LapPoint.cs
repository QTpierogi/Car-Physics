using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapPoint : MonoBehaviour
{
    public bool active;
    public bool finishLap;

    [HideInInspector]
    public bool lapOverNextPass;

    private void Start()
    {
        Register();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (active && other.transform.root.tag == "Player")
        {
            LapsManager.OnEnterLapPoint?.Invoke(this);
        }
    }

    protected void Register()
    {
        active = true;
        LapsManager.OnRegisterPoint?.Invoke(this);
    }
}
