using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    public WheelColliders colliders;
    private WheelFrictionCurve forwardFriction, sidewaysFriction;
    public float handBrakeFrictionMultiplier = 2f;
    public float grip = 1;
    private float driftFactor;
    private float steerInput;

    public void AdjustTraction(float steeringInput, float KPH)
    {
        steerInput = steeringInput;
        float driftSmothFactor = 1 * Time.deltaTime;

        sidewaysFriction = colliders.FLWheel.sidewaysFriction;
        forwardFriction = colliders.FLWheel.forwardFriction;

        float velocity = 0;

        if (Input.GetKey(KeyCode.Space))

            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue =
                Mathf.SmoothDamp(forwardFriction.asymptoteValue, driftFactor * handBrakeFrictionMultiplier, ref velocity, driftSmothFactor);

        else

            forwardFriction.extremumValue = forwardFriction.asymptoteValue = sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue =
                ((KPH * handBrakeFrictionMultiplier) / 300) + grip;


        SetUpFriction(colliders.FLWheel);
        SetUpFriction(colliders.FRWheel);
        SetUpFriction(colliders.RLWheel);
        SetUpFriction(colliders.RRWheel);

        sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue = 1.1f;
        if (Input.GetKey(KeyCode.Space))
        {
            SetUpFriction(colliders.FLWheel);
            SetUpFriction(colliders.FRWheel);
        }

        CheckSlip(colliders.FLWheel);
        CheckSlip(colliders.FRWheel);

    }

    void SetUpFriction(WheelCollider wheel)
    {
        wheel.sidewaysFriction = sidewaysFriction;
        wheel.forwardFriction = forwardFriction;
    }

    void CheckSlip(WheelCollider wheel)
    {
        WheelHit wheelHit;

        wheel.GetGroundHit(out wheelHit);


        if (wheelHit.sidewaysSlip < 0) 
            driftFactor = (1 + -steerInput) * Mathf.Abs(wheelHit.sidewaysSlip);

        if (wheelHit.sidewaysSlip > 0) 
            driftFactor = (1 + steerInput) * Mathf.Abs(wheelHit.sidewaysSlip);
    }
}
