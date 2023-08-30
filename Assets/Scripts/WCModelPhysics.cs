using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WheelColliders
{
    public WheelCollider FLWheel;
    public WheelCollider FRWheel;
    public WheelCollider RLWheel;
    public WheelCollider RRWheel;
}

[System.Serializable]
public class WheelMeshes
{
    public MeshRenderer FLWheel;
    public MeshRenderer FRWheel;
    public MeshRenderer RLWheel;
    public MeshRenderer RRWheel;
}


public class WCModelPhysics : MonoBehaviour
{
    internal enum driveType
    {
        frontWheelDrive,
        rearWheelDrive,
        allWheelDrive,
    }
    [SerializeField] private driveType drive;

    private Rigidbody rb;
    public WheelColliders colliders;
    public WheelMeshes WheelVisual;
    
    [Header("Car stats")]
    public float brakePower;
    public float speed;
    public float KPH;
    
    private float accelInput;
    private float brakeInput;
    private float steeringInput;

    private float currentTorque;
    private AckermannSteering steering;
    private EngineSimulation engine;
    private WheelController wheelController;
    private float DownForceValue = 10f;
    public Transform CenterOfMass;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        steering = GetComponent<AckermannSteering>();
        engine = GetComponent<EngineSimulation>();
        wheelController = GetComponent<WheelController>();
        rb.centerOfMass = CenterOfMass.localPosition;
    }

    void Update()
    {
        speed = rb.velocity.magnitude;
        KPH = speed * 3.6f;
        GatherInputs();
        addDownForce();
        MotorSimulation();
        ApplySteering();
        ApplyBrake();
        UpdateWheels();
        wheelController.AdjustTraction(steeringInput, KPH);
    }

    private void UpdateWheels()
    {
        UpdateWheelVisual(colliders.FLWheel, WheelVisual.FLWheel);
        UpdateWheelVisual(colliders.FRWheel, WheelVisual.FRWheel);
        UpdateWheelVisual(colliders.RLWheel, WheelVisual.RLWheel);
        UpdateWheelVisual(colliders.RRWheel, WheelVisual.RRWheel);
    }

    private void UpdateWheelVisual(WheelCollider collider, MeshRenderer wheelMesh)
    {
        Quaternion quat;
        Vector3 pos;
        collider.GetWorldPose(out pos, out quat);
        wheelMesh.transform.position = pos;
        wheelMesh.transform.rotation = quat;
    }

    private void addDownForce()
    {
        rb.AddForce(-transform.up * DownForceValue * rb.velocity.magnitude);
    }

    void GatherInputs()
    {
        steeringInput = Input.GetAxis("Horizontal");
        accelInput = Input.GetAxis("Vertical");
    }

    private void MotorSimulation()
    {
        float wheelTorque = Mathf.Abs((colliders.RRWheel.rpm + colliders.RLWheel.rpm) / 2f);
        currentTorque = engine.SimulateMotor(accelInput, wheelTorque);
        if(drive == driveType.allWheelDrive)
        {
            colliders.RRWheel.motorTorque = currentTorque / 4 * accelInput;
            colliders.RLWheel.motorTorque = currentTorque / 4 * accelInput;
            colliders.FRWheel.motorTorque = currentTorque / 4 * accelInput;
            colliders.FLWheel.motorTorque = currentTorque / 4 * accelInput;
        }
        else if(drive == driveType.frontWheelDrive)
        {
            colliders.FRWheel.motorTorque = currentTorque / 2 * accelInput;
            colliders.FLWheel.motorTorque = currentTorque / 2 * accelInput;
        }
        else
        {
            colliders.RRWheel.motorTorque = currentTorque / 2 * accelInput;
            colliders.RLWheel.motorTorque = currentTorque / 2 * accelInput;
        }
    }

    private void ApplyBrake()
    {
        brakeInput = engine.brakeInput;
        if(drive == driveType.frontWheelDrive)
        {
            colliders.FRWheel.brakeTorque = brakeInput * brakePower * 0.3f;
            colliders.FLWheel.brakeTorque = brakeInput * brakePower * 0.3f;
            colliders.RRWheel.brakeTorque = brakeInput * brakePower * 0.7f;
            colliders.RLWheel.brakeTorque = brakeInput * brakePower * 0.7f;
        }
        else
        {
            colliders.FRWheel.brakeTorque = brakeInput * brakePower * 0.7f;
            colliders.FLWheel.brakeTorque = brakeInput * brakePower * 0.7f;
            colliders.RRWheel.brakeTorque = brakeInput * brakePower * 0.3f;
            colliders.RLWheel.brakeTorque = brakeInput * brakePower * 0.3f;
        }        
    }

    private void ApplySteering()
    {
        float ackermannAngleLeft = 0;
        float ackermannAngleRight = 0;
        steering.Steer(steeringInput, out ackermannAngleLeft, out ackermannAngleRight);
        colliders.FRWheel.steerAngle = ackermannAngleRight;
        colliders.FLWheel.steerAngle = ackermannAngleLeft;
    }
}
