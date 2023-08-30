using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
    public WheelCollider wheelL;
    public WheelCollider wheelR;
    public float Antiroll = 5000f;

    private Rigidbody car;

    private void Start()
    {
        car = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float travelL = 1f;
        float travelR = 1f;

        WheelHit hit;
        bool groundedL = wheelL.GetGroundHit(out hit);
        if(groundedL)
        {
            travelL = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.suspensionDistance;
        }

        bool groundedR = wheelR.GetGroundHit(out hit);
        if (groundedR)
        {
            travelR = (-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;
        }

        float antiRollForce = (travelL - travelR) * Antiroll;
        if (groundedL)
            car.AddForceAtPosition(wheelL.transform.up * -antiRollForce, wheelL.transform.position);
        if (groundedR)
            car.AddForceAtPosition(wheelR.transform.up * antiRollForce, wheelR.transform.position);
    }
}
