using UnityEngine;
using System.Collections;

public class Car : MonoBehaviour 
{
    public Transform waypointContainer;
    public static float topSpeed = 250;
    private float currentSpeed;
	public float maxReverseSpeed = -50;
	public float maxTurnAngle = 10;
	public float maxTorque = 10;
	public float decelerationTorque = 30;
	public Vector3 centerOfMassAdjustment = new Vector3(0f,-0.9f,0f);
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
    public GameObject leftHeadlight;
    public GameObject rightHeadlight;
    public float maxBreakTorque = 100;
    private bool applyHandbrake = false;

    public float handbrakeForwardSlip = 0.04f;
    public float handbrakeSidewaysSlip = 0.08f;

    private Transform[] waypoints;
    private int currentWaypoint = 0;

    public int laps = 3;


    void Start()
	{
		//lower center of mass for roll-over resistance
		body = GetComponent<Rigidbody>();
		body.centerOfMass += centerOfMassAdjustment;

        GetWaypoints();
    }
	
	// FixedUpdate is called once per physics frame
	void FixedUpdate () 
	{
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            leftHeadlight.SetActive(!leftHeadlight.activeInHierarchy);
            rightHeadlight.SetActive(!rightHeadlight.activeInHierarchy);
        }

        if (Input.GetKey(KeyCode.S) || (Input.GetKey(KeyCode.DownArrow) || Input.GetButton("Jump")))
        {
            brakeLights.enabled = true;
        }
        else
        {
            brakeLights.enabled = false;
        }

        if (Input.GetButton("Jump"))
        {
            applyHandbrake = true;
            wheelFL.brakeTorque = maxBreakTorque;
            wheelFR.brakeTorque = maxBreakTorque;
            if(GetComponent<Rigidbody>().velocity.magnitude > 1)
            {
                SetSlipValues(handbrakeForwardSlip, handbrakeSidewaysSlip);
            } else
            {
                SetSlipValues(1f, 1f);
            }
        }
        else
        {
            applyHandbrake = false;
            wheelFL.brakeTorque = 0;
            wheelFR.brakeTorque = 0;
            SetSlipValues(1f, 1f);
        }


        //calculate max speed in KM/H (optimized calc)
        currentSpeed = wheelBL.radius*wheelBL.rpm*Mathf.PI*0.12f;
		if(currentSpeed < topSpeed && currentSpeed > maxReverseSpeed)
		{
			//rear wheel drive.
			wheelBL.motorTorque = Input.GetAxis("Vertical") * maxTorque;
			wheelBR.motorTorque = Input.GetAxis("Vertical") * maxTorque;
		}
		else
		{
			//can't go faster, already at top speed that engine produces.
			wheelBL.motorTorque = 0;
			wheelBR.motorTorque = 0;
		}
		
		//Spoilers add down pressure based on the car’s speed. (Upside-down lift)
		Vector3 localVelocity = transform.InverseTransformDirection(body.velocity);
		body.AddForce(-transform.up * (localVelocity.z * spoilerRatio),ForceMode.Impulse);


        //front wheel steering
        wheelFL.steerAngle = Input.GetAxis("Horizontal") * maxTurnAngle;
		wheelFR.steerAngle = Input.GetAxis("Horizontal")* maxTurnAngle;

		
		//apply deceleration when not pressing the gas or when breaking in either direction.
		if (!applyHandbrake && ((Input.GetAxis("Vertical") <= -0.5f && localVelocity.z > 0)||(Input.GetAxis("Vertical") >= 0.5f && localVelocity.z < 0)))
		{
			wheelBL.brakeTorque = decelerationTorque + maxTorque;
			wheelBR.brakeTorque = decelerationTorque + maxTorque;
		}
		else if(Input.GetAxis("Vertical") == 0)
		{
			wheelBL.brakeTorque = decelerationTorque;
			wheelBR.brakeTorque = decelerationTorque;
		}
		else
		{
			wheelBL.brakeTorque = 0;
			wheelBR.brakeTorque = 0;
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
		if(wheelFL.GetGroundHit(out contact))
		{
			Vector3 temp = wheelFL.transform.position;
			temp.y = (contact.point + (wheelFL.transform.up*wheelFL.radius)).y;
			wheelTransformFL.position = temp;
		}
		if(wheelFR.GetGroundHit(out contact))
		{
			Vector3 temp = wheelFR.transform.position;
			temp.y = (contact.point + (wheelFR.transform.up*wheelFR.radius)).y;
		    wheelTransformFR.position = temp;
		}
		if(wheelBL.GetGroundHit(out contact))
		{
			Vector3 temp = wheelBL.transform.position;
			temp.y = (contact.point + (wheelBL.transform.up*wheelBL.radius)).y;
			wheelTransformBL.position = temp;
		}
		if(wheelBR.GetGroundHit(out contact))
		{
			Vector3 temp = wheelBR.transform.position;
			temp.y = (contact.point + (wheelBR.transform.up*wheelBR.radius)).y;
			wheelTransformBR.position = temp;
		}
	}
	
	void Update()
	{
		//rotate the wheels based on RPM
		float rotationThisFrame = 360*Time.deltaTime;
		wheelTransformFL.Rotate(wheelFL.rpm/rotationThisFrame,0,0);
		wheelTransformFR.Rotate(wheelFR.rpm/rotationThisFrame,0,0);
		wheelTransformBL.Rotate(wheelBL.rpm/rotationThisFrame,0,0);
		wheelTransformBR.Rotate(wheelBR.rpm/rotationThisFrame,0,0);

        //turn wheels according to steering. make sure to take into account rotation being applied above
        wheelTransformFL.localEulerAngles = new Vector3(wheelTransformFL.localEulerAngles.x, wheelFL.steerAngle - wheelTransformFL.localEulerAngles.z, wheelTransformFL.localEulerAngles.z);
        wheelTransformFR.localEulerAngles = new Vector3(wheelTransformFR.localEulerAngles.x, wheelFR.steerAngle - wheelTransformFR.localEulerAngles.z, wheelTransformFR.localEulerAngles.z);

        UpdateWheelPositions();

        //turns spedometer needle based on cars speed
        SpedoMeter.ShowSpeed(body.velocity.magnitude, 0, 50);

        

    }

    void GetWaypoints()
    {
        //NOTE: Unity named this function poorly it also returns the parent’s component.
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
