using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcadePhysics : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Car stats")]
    public float TopSpeed = 10f;
    public float Acceleration = 5f;
    public float AccelerationCurve = 4f;
    public float Braking = 10f;
    public float ReverseAcceleration = 5f;
    public float ReverseSpeed = 5f;
    public float Steer = 5f;
    public float CoastingDrag = 4f;
    public float Grip = .95f;

    public Transform CenterOfMass;

    [Header("Drifting")]
    public float DriftGrip = 0.4f;
    public float DriftAdditionalSteer = 5.0f;
    public float MinAngleToFinishDrift = 10.0f;
    public float MinSpeedPercentToFinishDrift = 0.5f;
    public float DriftControl = 10.0f;
    public float DriftDampening = 10.0f;


    [Header("Suspensions")]
    public float SuspensionHeight = 0.2f;
    public float SuspensionSpring = 20000.0f;
    public float SuspensionDamp = 500.0f;
    public float WheelsPositionVerticalOffset = 0.0f;

    [Header("Physical Wheels")]
    public WheelCollider FrontLeftWheel;
    public WheelCollider FrontRightWheel;
    public WheelCollider RearLeftWheel;
    public WheelCollider RearRightWheel;

    [Header("Visual Wheels")]
    public MeshRenderer FLWheel;
    public MeshRenderer FRWheel;
    public MeshRenderer RLWheel;
    public MeshRenderer RRWheel;

    const float k_NullInput = 0.01f;
    const float k_NullSpeed = 0.01f;

    
    private bool WantsToDrift = false;
    private bool IsDrifting = false;
    float m_CurrentGrip = 1.0f;
    float m_DriftTurningPower = 0.0f;

    void SetSuspensionParams(WheelCollider wheel)
    {
        wheel.suspensionDistance = SuspensionHeight;
        wheel.center = new Vector3(0.0f, WheelsPositionVerticalOffset, 0.0f);
        JointSpring spring = wheel.suspensionSpring;
        spring.spring = SuspensionSpring;
        spring.damper = SuspensionDamp;
        wheel.suspensionSpring = spring;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        SetSuspensionParams(FrontLeftWheel);
        SetSuspensionParams(FrontRightWheel);
        SetSuspensionParams(RearLeftWheel);
        SetSuspensionParams(RearRightWheel);

        m_CurrentGrip = Grip;
    }

    void FixedUpdate()
    {
        rb.centerOfMass = transform.InverseTransformPoint(CenterOfMass.position);
        
        MoveVehicle(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        UpdateWheels();
    }

    void MoveVehicle(float accelerate, float turnInput)
    {
        float accelInput = (accelerate > 0 ? 1.0f : 0.0f) - (accelerate < 0 ? 1.0f : 0.0f);

        float accelerationCurveCoeff = 5;
        Vector3 localVel = transform.InverseTransformVector(rb.velocity);

        bool accelDirectionIsFwd = accelInput >= 0;
        bool localVelDirectionIsFwd = localVel.z >= 0;

        float maxSpeed = localVelDirectionIsFwd ? TopSpeed : ReverseSpeed;
        float accelPower = accelDirectionIsFwd ? Acceleration : ReverseAcceleration;

        float currentSpeed = rb.velocity.magnitude;
        float accelRampT = currentSpeed / maxSpeed;
        float multipliedAccelerationCurve = AccelerationCurve * accelerationCurveCoeff;
        float accelRamp = Mathf.Lerp(multipliedAccelerationCurve, 1, accelRampT * accelRampT);

        bool isBraking = (localVelDirectionIsFwd && accelerate < 0) || (!localVelDirectionIsFwd && accelerate > 0);

        float finalAccelPower = isBraking ? Braking : accelPower;

        float finalAcceleration = finalAccelPower * accelRamp;

        float turningPower = IsDrifting ? m_DriftTurningPower : turnInput * Steer;

        Quaternion turnAngle = Quaternion.AngleAxis(turningPower, transform.up);
        Vector3 fwd = turnAngle * transform.forward;
        Vector3 movement = fwd * accelInput * finalAcceleration;
 
        bool wasOverMaxSpeed = currentSpeed >= maxSpeed;

        if (wasOverMaxSpeed && !isBraking)
            movement *= 0.0f;
        
        Vector3 newVelocity = rb.velocity + movement * Time.fixedDeltaTime;
        newVelocity.y = rb.velocity.y;
        
        if (!wasOverMaxSpeed)
        {
            newVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);
        }

        if (Mathf.Abs(accelInput) < k_NullInput)
        {
            newVelocity = Vector3.MoveTowards(newVelocity, new Vector3(0, rb.velocity.y, 0), Time.fixedDeltaTime * CoastingDrag);
        }

        rb.velocity = newVelocity;

        float angularVelocitySteering = 0.4f;
        float angularVelocitySmoothSpeed = 20f;

        if (!localVelDirectionIsFwd && !accelDirectionIsFwd)
            angularVelocitySteering *= -1.0f;

        var angularVel = rb.angularVelocity;

        angularVel.y = Mathf.MoveTowards(angularVel.y, turningPower * angularVelocitySteering, Time.fixedDeltaTime * angularVelocitySmoothSpeed);

        rb.angularVelocity = angularVel;

        float velocitySteering = 25f;

        if (!IsDrifting)
        {
            if ((WantsToDrift || isBraking) && currentSpeed > maxSpeed * MinSpeedPercentToFinishDrift)
            {
                IsDrifting = true;
                m_DriftTurningPower = turningPower + (Mathf.Sign(turningPower) * DriftAdditionalSteer);
                m_CurrentGrip = DriftGrip;
            }
        }

        if (IsDrifting)
        {
            float turnInputAbs = Mathf.Abs(turnInput);
            if (turnInputAbs < k_NullInput)
                m_DriftTurningPower = Mathf.MoveTowards(m_DriftTurningPower, 0.0f, Mathf.Clamp01(DriftDampening * Time.fixedDeltaTime));

            float driftMaxSteerValue = Steer + DriftAdditionalSteer;
            m_DriftTurningPower = Mathf.Clamp(m_DriftTurningPower + (turnInput * Mathf.Clamp01(DriftControl * Time.fixedDeltaTime)), -driftMaxSteerValue, driftMaxSteerValue);

            bool facingVelocity = Vector3.Dot(rb.velocity.normalized, transform.forward * Mathf.Sign(accelInput)) > Mathf.Cos(MinAngleToFinishDrift * Mathf.Deg2Rad);

            bool canEndDrift = true;
            if (isBraking)
                canEndDrift = false;
            else if (!facingVelocity)
                canEndDrift = false;
            else if (turnInputAbs >= k_NullInput && currentSpeed > maxSpeed * MinSpeedPercentToFinishDrift)
                canEndDrift = false;

            if (canEndDrift || currentSpeed < k_NullSpeed)
            {
                IsDrifting = false;
                m_CurrentGrip = Grip;
            }
        }

        rb.velocity = Quaternion.AngleAxis(turningPower * Mathf.Sign(localVel.z) * velocitySteering * m_CurrentGrip * Time.fixedDeltaTime, transform.up) * rb.velocity;
    }

    private void UpdateWheels()
    {
        UpdateWheelVisual(FrontLeftWheel, FLWheel);
        UpdateWheelVisual(FrontRightWheel, FRWheel);
        UpdateWheelVisual(RearLeftWheel, RLWheel);
        UpdateWheelVisual(RearRightWheel, RRWheel);
    }

    private void UpdateWheelVisual(WheelCollider collider, MeshRenderer wheelMesh)
    {
        Quaternion quat;
        Vector3 pos;
        collider.GetWorldPose(out pos, out quat);
        wheelMesh.transform.position = pos;
        wheelMesh.transform.rotation = quat;
    }
}
