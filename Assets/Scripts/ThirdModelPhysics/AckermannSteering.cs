using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AckermannSteering : MonoBehaviour
{
    [Header("Car Specs")]
    public float wheelBase;
    public float rearTrack;
    public float turnRadius;


    public void Steer(float steerInput, out float ackermannAngleLeft, out float ackermannAngleRight)
    {
        if(steerInput < 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
        }
        else if(steerInput > 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
        }
        else
        {
            ackermannAngleLeft = 0;
            ackermannAngleRight = 0;
        }
    }
}
