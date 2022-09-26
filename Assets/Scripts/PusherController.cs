using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using Assets.Scripts;

public enum ControlMode
{
    Selfplay,
    Human
}

public class PusherController : MonoBehaviour
{
    [SerializeField] private float maxVelocity;
    public ControlMode ControlMode;

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
    private GameObject cursor;
    private readonly Vector3 cursorOffset = new Vector3(0, 5, -10);


    // Accelaration calculation
    Vector2 lastVelocity;
    Vector2 currentAccelaration;

    // Additional observations calculations
    private Vector3 agentGoalPos;
    private Vector3 humanGoalPos;
    private Vector3 puckPos;

    //Cameras
    private Camera playerViewCamera;
    private Camera agentViewCamera;
    private Collider colliderPlaneTable;
    private Collider colliderPlaneLeft;
    private Collider colliderPlaneRight;
    private Collider colliderPlaneGoal;
    private Collider colliderPlaneAgentSide;

    // Update is called once per frame
    private void Start()
    {
        playerViewCamera = GameObject.Find("PlayerViewCamera").GetComponent<Camera>();
        agentViewCamera = GameObject.Find("AgentViewCamera").GetComponent<Camera>();
        colliderPlaneTable = GameObject.Find("AirHockeyTableTop").GetComponent<Collider>();
        colliderPlaneLeft = GameObject.Find("ColliderOutOfBoundsLeft").GetComponent<Collider>();
        colliderPlaneRight = GameObject.Find("ColliderOutOfBoundsRight").GetComponent<Collider>();
        colliderPlaneGoal = GameObject.Find("ColliderOutOfBoundsHumanGoal").GetComponent<Collider>();
        colliderPlaneAgentSide = GameObject.Find("ColliderOutOfBoundsAgentSide").GetComponent<Collider>();
        agentGoalPos = GameObject.Find("AgentPlayerGoal").GetComponent<Transform>().position;
        humanGoalPos = GameObject.Find("AgentPlayerGoal").GetComponent<Transform>().position;
        puckPos = GameObject.Find("AgentPlayerGoal").GetComponent<Transform>().position;
        cursor = GameObject.Find("HandCursor");

        arriveSteeringBehavior = new ArriveSteeringBehavior();
        targetPosition = GetCurrentPosition();

        Cursor.visible = false;
    }
    void Update()
    {
        switch (ControlMode)
        {
            case ControlMode.Selfplay:
                //if (cursor.activeSelf is true)
                //{
                //    cursor.SetActive(false);
                //}
                break;
            case ControlMode.Human:
                //if (cursor.activeSelf is false)
                //{
                //    cursor.SetActive(true);
                //}
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
        Vector3 mousePosTable;
        Vector2 targetPosition = new Vector2();
        Ray ray = new Ray();
        // get current active display
        int display = Display.activeEditorGameViewTarget;
        // get other cameras
        if (display == 1)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        else if (display == 0)
        {
            ray = playerViewCamera.ScreenPointToRay(Input.mousePosition);
        }
        else if (display == 2)
        {
            ray = agentViewCamera.ScreenPointToRay(Input.mousePosition);
        }
        // Todo: maybe change order of if-condition to detect raycast with collider agent side first
        if (colliderPlaneTable.Raycast(ray, out RaycastHit hitData, 1000f))
        {
            mousePosTable = hitData.point;            
            targetPosition = new Vector2(mousePosTable.x, mousePosTable.z);
            cursor.transform.position = transform.position + cursorOffset;
        }
        else
        {
            DetermineColliderRaycast(ref colliderPlaneGoal, ray);
            DetermineColliderRaycast(ref colliderPlaneLeft, ray);
            DetermineColliderRaycast(ref colliderPlaneRight, ray);
            DetermineColliderRaycast(ref colliderPlaneAgentSide, ray);

            targetPosition = GetCurrentPosition();
        }   
        return targetPosition;
    }

    public void DetermineColliderRaycast(ref Collider collider, Ray ray)
    {
        Vector3 mousePosWorld;
        if (collider.Raycast(ray, out RaycastHit hit, 1000f))
        {
            mousePosWorld = hit.point;
            cursor.transform.position = mousePosWorld + cursorOffset;
        }
    }

    public void Reset(string pusherType, bool setToNirvana)
    {
        if (pusherType == "Agent")
        {
            transform.position = new Vector3(0, 0, 45.75f);
        }
        else if(pusherType == "Human")
        {
            if (setToNirvana)
            {
                transform.position = new Vector3(0, 0, -450.75f);
            }
            else
            {
                transform.position = new Vector3(0, 0, -45.75f);
            }
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

    public Vector2 GetDistanceAgentGoal()
    {
        Vector2 goalPos = new Vector2(agentGoalPos.x, agentGoalPos.z);
        return goalPos - GetCurrentPosition();
    }

    public Vector2 GetDistanceHumanGoal()
    {
        Vector2 goalPos = new Vector2(humanGoalPos.x, humanGoalPos.z);
        return goalPos - GetCurrentPosition();
    }

    public Vector2 GetDistancePuck()
    {
        Vector2 pos = new Vector2(puckPos.x, puckPos.z);
        return pos - GetCurrentPosition();
    }

    private void FixedUpdate()
    {
        var currentVelocity = new Vector2(pusherActuatorX.Velocity, pusherActuatorZ.Velocity);
        currentAccelaration = currentVelocity - lastVelocity;
        lastVelocity = currentVelocity;
    }
}

