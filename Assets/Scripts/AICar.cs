using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICar : MonoBehaviour
{
    public Transform waypointContainer;

    private Transform[] waypoints;

    public float topSpeed = 150;
    private float currentSpeed;
    public float maxReverseSpeed = -50;
    public float maxTurnAngle = 10;
    public float maxTorque = 10;
    public float decelerationTorque = 30;
    public Vector3 centerOfMassAdjustment = new Vector3(0f, -0.9f, 0f);
    public float spoilerRatio = 0.1f;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelBL;
    public WheelCollider wheelBR;
    public Transform wheelTransformFL;
    public Transform wheelTransformFR;
    public Transform wheelTransformBL;
    public Transform wheelTransformBR;
    private Rigidbody body;
    public MeshRenderer brakeLights;

    public float maxBreakTorque = 100;
    private bool applyHandbrake = false;

    public float handbrakeForwardSlip = 0.04f;
    public float handbrakeSidewaysSlip = 0.08f;

    public GameObject leftHeadlight;
    public GameObject rightHeadlight;

    private int currentWaypoint = 0;
    private float inputSteer;
    private float inputTorque;

    void Start()
    {

        //lower center of mass for roll-over resistance
        body = GetComponent<Rigidbody>();
        body.centerOfMass += centerOfMassAdjustment;
        //get the waypoints from the track.
        GetWaypoints();


    }

   

    void FixedUpdate()
    {
        //calculate turn angle
        Vector3 RelativeWaypointPosition = transform.InverseTransformPoint(new Vector3(waypoints[currentWaypoint].position.x, transform.position.y, waypoints[currentWaypoint].position.z));
        inputSteer = RelativeWaypointPosition.x / RelativeWaypointPosition.magnitude;

        //Spoilers add down pressure based on the car’s speed. (Upside-down lift)
        Vector3 localVelocity = transform.InverseTransformDirection(body.velocity);
        body.AddForce(-transform.up * (localVelocity.z * spoilerRatio), ForceMode.Impulse);

        //calculate torque.		
        if (Mathf.Abs(inputSteer) < 0.5f)
        {
            //when making minor turning adjustments speed is based on how far to the next point.
            inputTorque = (RelativeWaypointPosition.z / RelativeWaypointPosition.magnitude);
            applyHandbrake = false;
        }
        else
        {//we need to make a hard turn, if moving fast apply handbrake to slide.
            if (body.velocity.magnitude > 10)
            {
                applyHandbrake = true;
            }
            //if not moving forward backup and turn opposite.
            else if (localVelocity.z < 0)
            {
                applyHandbrake = false;
                inputTorque = -1;
                inputSteer *= -1;
            }
            //let off the gas while making a hard turn.
            else
            {
                applyHandbrake = false;
                inputTorque = 0;
            }
        }
        //set slip values
        if (applyHandbrake)
        {
            SetSlipValues(handbrakeForwardSlip, handbrakeSidewaysSlip);
        }
        else
        {
            SetSlipValues(1f, 1f);
        }
        
        //if close enough, change waypoints.
        if (RelativeWaypointPosition.magnitude < 25)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = 0;
            }
        }

        //front wheel steering
        wheelFL.steerAngle = inputSteer * maxTurnAngle;
        wheelFR.steerAngle = inputSteer * maxTurnAngle;

        //calculate max speed in KM/H
        currentSpeed = wheelBL.radius * wheelBL.rpm * Mathf.PI * 0.12f;
        if (currentSpeed < topSpeed && currentSpeed > maxReverseSpeed)
        {
            //rear wheel drive.
            wheelBL.motorTorque = inputTorque * maxTorque;
            wheelBR.motorTorque = inputTorque * maxTorque;
        }
        else
        {
            //can't go faster, already at top speed that engine produces.
            wheelBL.motorTorque = 0;
            wheelBR.motorTorque = 0;
        }
    }

    void SetSlipValues(float forward, float sideways)
    {
        WheelFrictionCurve tempStruct = wheelBR.forwardFriction;
        tempStruct.stiffness = forward;
        wheelBR.forwardFriction = tempStruct;

        tempStruct = wheelBR.sidewaysFriction;
        tempStruct.stiffness = sideways;
        wheelBR.sidewaysFriction = tempStruct;

        tempStruct = wheelBL.forwardFriction;
        tempStruct.stiffness = forward;
        wheelBL.forwardFriction = tempStruct;

        tempStruct = wheelBL.sidewaysFriction;
        tempStruct.stiffness = sideways;
        wheelBL.sidewaysFriction = tempStruct;
    }

    void UpdateWheelPositions()
    {
        //move wheels based on their suspension.
        WheelHit contact = new WheelHit();
        if (wheelFL.GetGroundHit(out contact))
        {
            Vector3 temp = wheelFL.transform.position;
            temp.y = (contact.point + (wheelFL.transform.up * wheelFL.radius)).y;
            wheelTransformFL.position = temp;
        }
        if (wheelFR.GetGroundHit(out contact))
        {
            Vector3 temp = wheelFR.transform.position;
            temp.y = (contact.point + (wheelFR.transform.up * wheelFR.radius)).y;
            wheelTransformFR.position = temp;
        }
        if (wheelBL.GetGroundHit(out contact))
        {
            Vector3 temp = wheelBL.transform.position;
            temp.y = (contact.point + (wheelBL.transform.up * wheelBL.radius)).y;
            wheelTransformBL.position = temp;
        }
        if (wheelBR.GetGroundHit(out contact))
        {
            Vector3 temp = wheelBR.transform.position;
            temp.y = (contact.point + (wheelBR.transform.up * wheelBR.radius)).y;
            wheelTransformBR.position = temp;
        }
    }

    void Update()
    {
        //rotate the wheels based on RPM
        float rotationThisFrame = 360 * Time.deltaTime;
        wheelTransformFL.Rotate(wheelFL.rpm / rotationThisFrame, 0, 0);
        wheelTransformFR.Rotate(wheelFR.rpm / rotationThisFrame, 0, 0);
        wheelTransformBL.Rotate(wheelBL.rpm / rotationThisFrame, 0, 0);
        wheelTransformBR.Rotate(wheelBR.rpm / rotationThisFrame, 0, 0);

        //turn wheels according to steering. make sure to take into account rotation being applied above
        wheelTransformFL.localEulerAngles = new Vector3(wheelTransformFL.localEulerAngles.x, wheelFL.steerAngle - wheelTransformFL.localEulerAngles.z, wheelTransformFL.localEulerAngles.z);
        wheelTransformFR.localEulerAngles = new Vector3(wheelTransformFR.localEulerAngles.x, wheelFR.steerAngle - wheelTransformFR.localEulerAngles.z, wheelTransformFR.localEulerAngles.z);

        UpdateWheelPositions();



    }

    void GetWaypoints()
    {
  
        Transform[] potentialWaypoints = waypointContainer.GetComponentsInChildren<Transform>();

        //initialize the waypoints array so that is has enough space to store the nodes.
        waypoints = new Transform[(potentialWaypoints.Length - 1)];

        //loop through the list and copy the nodes into the array.
        //start at 1 instead of 0 to skip the WaypointContainer’s transform.
        for (int i = 1; i < potentialWaypoints.Length; ++i)
        {
            waypoints[i - 1] = potentialWaypoints[i];
        }
    }

    public Transform GetCurrentWaypoint()
    {
        return waypoints[currentWaypoint];
    }

    public Transform GetLastWaypoint()
    {
        if (currentWaypoint - 1 < 0)
        {
            return waypoints[waypoints.Length - 1];
        }

        return waypoints[currentWaypoint - 1];
    }
}







