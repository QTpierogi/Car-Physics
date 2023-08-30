using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastWheelModelPhysics : MonoBehaviour
{
    public CustomWheel[] wheels;
    
    [Header("Car stats")]
    public float brakeForce;
    private float DownForceValue = 10f;
    public float AntirollForce = 5000f;
    public Transform CenterOfMass;

    private Rigidbody rb;
    private AckermannSteering steering;
    private EngineSimulation engine;
    private float currentTorque;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = CenterOfMass.localPosition;
        steering = GetComponent<AckermannSteering>();
        engine = GetComponent<EngineSimulation>();
    }

    void Update()
    {
        addDownForce();
        MoveVehicle();
        ApplyBrake();
        AntiRoll();
        Steer(Input.GetAxis("Horizontal"));
    }

    private void addDownForce()
    {
        rb.AddForce(-transform.up * DownForceValue * rb.velocity.magnitude);
    }


    private void MoveVehicle()
    {
        float wheelTorque = 0;
        foreach(var w in wheels)
        {
            if (!w.front)
            {
                wheelTorque += w.rpm;
            }
        }
        wheelTorque /= 2f;
        currentTorque = engine.SimulateMotor(Input.GetAxis("Vertical"), wheelTorque);
        currentTorque = Mathf.Max(currentTorque, 0f);
        foreach (var w in wheels)
        {
            if (!w.front)
            {
                w.motorTorque = currentTorque / 2 * Input.GetAxis("Vertical");
            }
        }
    }

    private void Steer(float steerInput)
    {
        float ackermannAngleLeft = 0;
        float ackermannAngleRight = 0;
        steering.Steer(steerInput, out ackermannAngleLeft, out ackermannAngleRight);

        foreach (CustomWheel w in wheels)
        {
            if (w.front && w.right)
            {
                w.steerAngle = ackermannAngleRight;
            }
            else if (w.front && !w.right)
            {
                w.steerAngle = ackermannAngleLeft;
            }
            else
            {
                w.steerAngle = 0f;
            }
        }
    }

    private void ApplyBrake()
    {
        var brakeInput = engine.brakeInput;
        foreach(var w in wheels)
        {
            if(w.front)
            {
                w.brakeTorque = brakeInput * brakeForce * 0.3f;
            }
            else
            {
                w.brakeTorque = brakeInput * brakeForce * 0.7f;
            }
        }
    }

    private void AntiRoll()
    {
        float travelL = 1f;
        float travelR = 1f;
        CustomWheel leftWheel = new CustomWheel();
        CustomWheel rightWheel = new CustomWheel();

        foreach (CustomWheel w in wheels)
        {
            if (!w.front && w.right)
            {
                rightWheel = w;
            }
            if (!w.front && !w.right)
            {
                leftWheel = w;
            }
        }

        if (leftWheel.GroundHit)
        {
            travelL = (-leftWheel.transform.InverseTransformPoint(leftWheel.hit.point).y - leftWheel.wheelRadius) / leftWheel.springTravel;
        }

        if (rightWheel.GroundHit)
        {
            travelR = (-rightWheel.transform.InverseTransformPoint(rightWheel.hit.point).y - rightWheel.wheelRadius) / rightWheel.springTravel;
        }

        float antiRollForce = (travelL - travelR) * AntirollForce;
        if (leftWheel.GroundHit)
            rb.AddForceAtPosition(leftWheel.transform.up * -antiRollForce, leftWheel.transform.position);
        if (rightWheel)
            rb.AddForceAtPosition(rightWheel.transform.up * antiRollForce, rightWheel.transform.position);
    }
}

