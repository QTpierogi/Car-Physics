using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomWheel
    : MonoBehaviour
{
    private Rigidbody rb;
    public GameObject visualWheel;

    public RaycastHit hit;
    public bool GroundHit;

    [Header("Suspension")]
    public float restLength;
    public float springTravel;
    public float springStiffness;
    public float damperStiffness;

    [Header("Wheel Attributes")]
    public float mass = 20f;
    public float grip = 1f;

    [Header("Movement Variables")]
    public float motorTorque;
    public float rpm;
    public float brakeTorque;

    private float minLength;
    private float maxLength;
    private float lastLength;
    private float springLength;
    private float springVelocity;
    private float springForce;
    private float damperForce;

    private Vector3 suspensionForce;
    private Vector3 localWheelVelocity;
    public float movingForce;
    private float centrifugalForce;

    private float wheelAngle = 0f;

    [Header("Wheel")]
    public float wheelRadius;
    public float steerAngle;
    public float steerTime;
    public bool front;
    public bool right;

    private void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();

        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;
    }

    private void Update()
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, steerTime * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
    }

    private void FixedUpdate()
    {
        if (GroundHit = Physics.Raycast(transform.position, -transform.up, out hit, maxLength + wheelRadius))
        {
            localWheelVelocity = rb.GetPointVelocity(hit.point);
            Suspension(hit);
            CentrifulgarForce();
            MovingForce();
            rb.AddForceAtPosition(suspensionForce + (movingForce * transform.forward) + (centrifugalForce * transform.right), transform.position);            
        }
        Animate();
    }

    private void Suspension(RaycastHit hit)
    {
        lastLength = springLength;
        springLength = hit.distance - wheelRadius;
        springLength = Mathf.Clamp(springLength, minLength, maxLength);
        springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;
        springForce = springStiffness * (restLength - springLength);
        damperForce = damperStiffness * springVelocity;
        suspensionForce = (springForce + damperForce) * transform.up;
    }

    private void CentrifulgarForce()
    {
        Vector3 steeringDir = transform.right;
        float steeringVel = Vector3.Dot(steeringDir, localWheelVelocity);
        float desiredVelChange = -steeringVel * grip;
        float desiredAccel = desiredVelChange / Time.fixedDeltaTime;
        centrifugalForce = desiredAccel * mass;
    }

    private void MovingForce()
    {
        rpm = rb.velocity.magnitude * 60 / (wheelRadius * 2 * Mathf.PI);
        var brakeForce = Mathf.Sign(transform.InverseTransformDirection(localWheelVelocity).z) * brakeTorque;
        movingForce = (motorTorque - brakeForce) / wheelRadius;
    }

    private void Animate()
    {
        visualWheel.transform.Rotate(360 * (rb.velocity.magnitude / (wheelRadius * 2 * Mathf.PI)) * Time.fixedDeltaTime, 0, 0);
    }
}
