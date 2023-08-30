using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GearState
{
    Neutral,
    Running,
    CheckingChange,
    Changing
}

public class EngineSimulation : MonoBehaviour
{
    private Rigidbody rb;
    public float accelInput;
    public float brakeInput;

    [Header("Engine")]
    public float motorPower;
    public float RPM;
    public float speed;
    public float redLine;
    public float idleRPM;
    public float[] gearRatios;
    public float differentialRatio = 4f;
    public float wheelRPM;
    private float wheelTorque;
    public AnimationCurve horsePowerCurve;

    [Header("Transmission")]
    public int currentGear = 1;
    private float clutch;
    public GearState gearState;
    public float increaseGearRPM;
    public float decreaseGearRPM;
    public float changeGearTime = 0.5f;
    private bool inReverse = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        gearState = GearState.Running;
    }

    private void Update()
    {
        speed = rb.velocity.magnitude * 3.6f;
    }

    public float SimulateMotor(float accel, float torque)
    {
        accelInput = accel;
        wheelTorque = torque;

        HandleTransmission();
        var currentTorque = CalculateTorque();
        return currentTorque;
    }

    public float GetBraking()
    {
        return brakeInput;
    }

    private void HandleTransmission()
    {
        if (gearState != GearState.Changing)
        {
            if (gearState == GearState.Neutral)
            {
                clutch = 0;
                if (Mathf.Abs(accelInput) > 0)
                    gearState = GearState.Running;
            }
            else
            {
                clutch = Mathf.Lerp(clutch, 1, Time.deltaTime);
            }
        }
        else
        {
            clutch = 0;
        }

        float movingDirection = Vector3.Dot(transform.forward, rb.velocity);

        if (movingDirection < -0.5f)
        {
            if (accelInput > 0)
            {
                brakeInput = Mathf.Abs(accelInput);
                inReverse = false;
            }
            else
            {
                inReverse = true;
                brakeInput = 0;
            }
        }
        else if (movingDirection > 0.5f && accelInput < 0)
        {
            brakeInput = Mathf.Abs(accelInput);
            inReverse = false;
        }
        else
        {
            brakeInput = 0;
            inReverse = false;
        }
    }

    private float CalculateTorque()
    {
        float torque = 0;
        if (RPM < idleRPM + 200 && accelInput == 0 && currentGear == 1)
        {
            gearState = GearState.Neutral;
        }
        if(inReverse && currentGear != 0)
        {
            StartCoroutine(ChangeGear(-gearRatios.Length));
        }
        if (gearState == GearState.Running && clutch > 0 && !inReverse)
        {
            if (RPM > increaseGearRPM)
            {
                StartCoroutine(ChangeGear(1));
            }
            else if (RPM < decreaseGearRPM)
            {
                StartCoroutine(ChangeGear(-1));
            }
        }
        if (clutch < 0.1f)
        {
            RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM, redLine * accelInput) + Random.Range(-50, 50), Time.deltaTime);
        }
        else
        {
            wheelRPM = wheelTorque * gearRatios[currentGear] * differentialRatio;
            RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM - 100, wheelRPM), Time.deltaTime * 3f);
            torque = (horsePowerCurve.Evaluate(RPM / redLine) * motorPower / RPM) * gearRatios[currentGear] * differentialRatio * 5252f * clutch;
            //Debug.Log("Torgue = " + torque + ". RPM = " + RPM + ". WheelRPM = " + wheelRPM);
        }
        return torque;
    }

    public float GetSpeedRatio()
    {
        var gas = Mathf.Clamp(Mathf.Abs(accelInput), 0.5f, 1f);
        return RPM * gas / redLine;
    }

    IEnumerator ChangeGear(int gearChange)
    {
        gearState = GearState.CheckingChange;
        if (currentGear + gearChange > 0)
        {
            if (gearChange > 0)
            {
                yield return new WaitForSeconds(0.7f);
                if (RPM < increaseGearRPM || currentGear >= gearRatios.Length - 1)
                {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            if (gearChange < 0)
            {
                yield return new WaitForSeconds(0.1f);
                if (RPM > decreaseGearRPM || currentGear <= 1)
                {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            gearState = GearState.Changing;
            yield return new WaitForSeconds(changeGearTime);
            currentGear += gearChange;
        }
        else if(inReverse)
        {
            gearState = GearState.Changing;
            yield return new WaitForSeconds(changeGearTime);
            currentGear = 0;
        }
        if (gearState != GearState.Neutral)
            gearState = GearState.Running;
    }
}
