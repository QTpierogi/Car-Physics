using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EngineUI : MonoBehaviour
{
    public TMP_Text speedText;
    public TMP_Text gearText;
    public Transform rpmNeedle;
    public float minNeedleRotation;
    public float maxNeedleRotation;

    private EngineSimulation engine;

    private void Awake()
    {
        engine = FindObjectOfType<EngineSimulation>();
        if (engine == null)
            gameObject.SetActive(false);
    }

    private void Update()
    {
        rpmNeedle.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(minNeedleRotation, maxNeedleRotation, engine.RPM / (engine.redLine * 1.1f)));
        speedText.text = engine.speed.ToString("0") + " km/h";
        gearText.text = (engine.gearState == GearState.Neutral) ? "N" : ((engine.currentGear == 0) ? "R" : engine.currentGear.ToString());
    }
}
