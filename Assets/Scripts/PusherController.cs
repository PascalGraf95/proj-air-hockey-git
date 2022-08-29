using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using Assets.Scripts;

enum ControlMode
{
    Selfplay,
    Human
}

public class PusherController : MonoBehaviour
{
    //[SerializeField] private MjActuator pusherActuatorX;
    //[SerializeField] private MjActuator pusherActuatorZ;

    //[SerializeField] private MjSlideJoint slideJointX;
    //[SerializeField] private MjSlideJoint slideJointZ;

    [SerializeField] private float maxVelocity;
    [SerializeField] private ControlMode controlMode;

    public MjActuator pusherActuatorZ;
    public MjActuator pusherActuatorX;
    public MjSlideJoint slideJointX;
    public MjSlideJoint slideJointZ;

    [Header("Steering Behavior")]
    [Tooltip("Maximum acceleration the Character is able to reach.")]
    public float MaxAcceleration;
    [Tooltip("Maximum speed the Character is able to reach.")]
    public float MaxSpeed;
    [Tooltip("Radius for arriving at the target.")]
    public float TargetRadius;
    [Tooltip("Radius for beginning to slow down.")]
    public float SlowDownRadius;
    [Tooltip("Time over which to achieve the target speed.")]
    public float TimeToTarget;

    // steering behavior fields
    public Character Character;
    private Vector2 targetPosition;
    private Vector2 accelaration;
    private ArriveSteeringBehavior arriveSteeringBehavior;

    // Accelartion calculation
    Vector2 lastVelocity;
    Vector2 currentAccelaration;

    //Cameras
    private Camera playerViewCamera;
    private Camera agentViewCamera;
    private Collider colliderPlane;

    // Update is called once per frame
    private void Start()
    {
        playerViewCamera = GameObject.Find("PlayerViewCamera").GetComponent<Camera>();
        agentViewCamera = GameObject.Find("AgentViewCamera").GetComponent<Camera>();
        colliderPlane = GameObject.Find("AirHockeyTableTop").GetComponent<Collider>();

        arriveSteeringBehavior = new ArriveSteeringBehavior();
        targetPosition = GetCurrentPosition();

    }
    void Update()
    {
        float xInput;
        float zInput;
        switch (controlMode)
        {
            case ControlMode.Selfplay:
                break;
            case ControlMode.Human:
                // get current mouse position on left mouse button click
                if (Input.GetMouseButton(0))
                {
                    targetPosition = GetMousePosition();
                }

                // compute arrive steering behavior
                accelaration = arriveSteeringBehavior.Arrive(targetPosition, GetCurrentPosition(), GetCurrentVelocity(), TargetRadius, SlowDownRadius, MaxSpeed, MaxAcceleration, TimeToTarget);

                // set actuator acceleration
                pusherActuatorX.Control = -accelaration.x;
                pusherActuatorZ.Control = -accelaration.y;

                break;
        }

    }

    /// <summary>
    /// Gets the current mouse position in relation to a plane.
    /// </summary>
    /// <returns>The target position</returns>
    private Vector2 GetMousePosition()
    {
        Vector3 mousePosWorld = new Vector3();
        Ray ray = new Ray();
        // get current active display
        int display = 1;//Display.activeEditorGameViewTarget;
        // get other cameras

        if (display == 0)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        else if (display == 1)
        {
            ray = playerViewCamera.ScreenPointToRay(Input.mousePosition);
        }
        else if (display == 2)
        {
            ray = agentViewCamera.ScreenPointToRay(Input.mousePosition);
        }
        if (colliderPlane.Raycast(ray, out RaycastHit hitData, 1000f))
        {
            mousePosWorld = hitData.point;
        }
        Vector2 targetPosition = new Vector2(mousePosWorld.x, mousePosWorld.z);
        return targetPosition;
    }


    public void Reset(string pusherType)
    {
        if(pusherType == "Agent")
        {
            transform.position = new Vector3(0, 0, 45.75f);
        }
        else if(pusherType == "Human")
        {
            transform.position = new Vector3(0, 0, -45.75f);
        }
        slideJointX.Configuration = 0f;
        slideJointZ.Configuration = 0f;
        slideJointX.Velocity = 0f;
        slideJointZ.Velocity = 0f;
        targetPosition = GetCurrentPosition();
    }

    public void Act(Vector2 targetVelocity)
    {
        pusherActuatorX.Control = targetVelocity.x * maxVelocity;
        pusherActuatorZ.Control = targetVelocity.y * maxVelocity;
    }

    public Vector2 GetCurrentPosition()
    {
        return new Vector2(transform.position.x, transform.position.z);
    }

    public Vector2 GetCurrentVelocity()
    {
        return new Vector2(pusherActuatorX.Velocity, pusherActuatorZ.Velocity);
    }

    public Vector2 GetCurrentAccelaration()
    {
        return currentAccelaration;
    }

    private void FixedUpdate()
    {
        var currentVelocity = new Vector2(pusherActuatorX.Velocity, pusherActuatorZ.Velocity);
        currentAccelaration = currentVelocity - lastVelocity;
        lastVelocity = currentVelocity;
    }
}

