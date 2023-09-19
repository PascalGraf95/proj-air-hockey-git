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

enum ResetPusherMode
{
    Standard,
    Random
}


public class PusherController : MonoBehaviour
{
    private float maxVelocity;

    public ControlMode ControlMode;
    [SerializeField] private ResetPusherMode resetMode;

    public MjGeom geom;
    public MjActuator pusherActuatorZ;
    public MjActuator pusherActuatorX;
    public MjSlideJoint slideJointX;
    public MjSlideJoint slideJointZ;

    // steering behavior fields
    private ActionType actionType;
    private Vector2 targetPosition;
    private GameObject cursor;
    private readonly Vector3 cursorOffset = new Vector3(0, 5, -10);


    // Acceleration calculation
    Vector2 lastVelocity;
    Vector2 currentAcceleration;

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

    private Material selfplayMaterial;
    private Material humanplayMaterial;
    private GameObject hand;
    private float pusherOffset = 45.75f;
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
        hand = GameObject.Find("StylizedHand");
        selfplayMaterial = Resources.Load("White-Hand-Selfplay") as Material;
        humanplayMaterial = Resources.Load("White-Hand") as Material;
        targetPosition = GetCurrentPosition();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void SetPusherConfiguration(PusherConfiguration pusherConfiguration, ActionType actionType)
    {
        this.actionType = actionType;
        maxVelocity = pusherConfiguration.maxVelocity;
        slideJointX.Settings.Spring.Damping = pusherConfiguration.jointDamping;
        slideJointZ.Settings.Spring.Damping = pusherConfiguration.jointDamping;
        geom.Mass = pusherConfiguration.mass;
        if(actionType == ActionType.ContinuousVelocity)
        {
            pusherActuatorX.Type = MjActuator.ActuatorType.Velocity;
            pusherActuatorZ.Type = MjActuator.ActuatorType.Velocity;
            pusherActuatorX.CustomParams.Kv = pusherConfiguration.velocityControlFactor;
            pusherActuatorZ.CustomParams.Kv = pusherConfiguration.velocityControlFactor;
        }
        else if(actionType == ActionType.ContinuousPosition)
        {
            pusherActuatorX.Type = MjActuator.ActuatorType.Position;
            pusherActuatorZ.Type = MjActuator.ActuatorType.Position;
            pusherActuatorX.CustomParams.Kp = pusherConfiguration.velocityControlFactor;
            pusherActuatorZ.CustomParams.Kp = pusherConfiguration.velocityControlFactor;
        }
    }

    void Update()
    {
        targetPosition = GetMousePosition();
        switch (ControlMode)
        {
            case ControlMode.Selfplay:
                hand.GetComponent<SkinnedMeshRenderer>().material = selfplayMaterial;
                cursor.transform.position = new Vector3(targetPosition.x, cursorOffset.y, targetPosition.y + cursorOffset.z);
                break;
            case ControlMode.Human:
                hand.GetComponent<SkinnedMeshRenderer>().material = humanplayMaterial;
                // get current mouse position on left mouse button click
                if (Input.GetMouseButton(0))
                {
                    if (targetPosition.x < Boundaries.humanPusherBoundaryHard.right &&
                        targetPosition.x > Boundaries.humanPusherBoundaryHard.left)
                    {
                        pusherActuatorX.Control = -targetPosition.x;
                    }
                    if (targetPosition.y < Boundaries.humanPusherBoundaryHard.up &&
                        targetPosition.y > Boundaries.humanPusherBoundaryHard.down)
                    {
                        pusherActuatorZ.Control = -targetPosition.y - pusherOffset; // offset between AirHockeyTable and Pusher coordinate system
                    }
                }
                else
                {
                    cursor.transform.position = new Vector3(targetPosition.x, cursorOffset.y, targetPosition.y + cursorOffset.z);
                }
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
        Vector2 targetPosition;
        Ray ray = new Ray();
        // get current active display
        int display = 0;
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
            List<Collider> colliderList = new List<Collider>();
            colliderList.Add(colliderPlaneGoal);
            colliderList.Add(colliderPlaneLeft);
            colliderList.Add(colliderPlaneRight);
            colliderList.Add(colliderPlaneAgentSide);

            foreach (Collider collider in colliderList)
            {
                Vector3 mousePosWorld;
                if (collider.Raycast(ray, out RaycastHit hit, 1000f))
                {
                    mousePosWorld = hit.point;
                    targetPosition = new Vector2(mousePosWorld.x, mousePosWorld.z);
                    cursor.transform.position = mousePosWorld;
                }
            }
            targetPosition.x = cursor.transform.position.x;
            targetPosition.y = cursor.transform.position.z;
        }
        return targetPosition;
    }

    public void Reset(string pusherType, bool setToNirvana)
    {
        float xPos = 0f;
        float zPos = 0f;


        if (pusherType == "Agent")
        {
            if (resetMode == ResetPusherMode.Standard)
            {
                transform.position = new Vector3(0, 0, pusherOffset);
            }
            else if (resetMode == ResetPusherMode.Random)
            {
                xPos = Random.Range(-30f, 30f);
                zPos = Random.Range(-40f, 23f);
                transform.position = new Vector3(xPos, 0, pusherOffset + zPos);
            }
        }
        else if (pusherType == "Human")
        {
            if (setToNirvana)
            {
                transform.position = new Vector3(0, 0, -450.75f);
            }
            else
            {
                if (resetMode == ResetPusherMode.Standard)
                {
                    transform.position = new Vector3(0, 0, -pusherOffset);
                }
                else if (resetMode == ResetPusherMode.Random)
                {
                    xPos = Random.Range(-30f, 30f);
                    zPos = Random.Range(-23f, 40f);
                    transform.position = new Vector3(xPos, 0, -pusherOffset + zPos);
                }
            }
        }
        slideJointX.Configuration = -xPos;
        slideJointZ.Configuration = -zPos;
        slideJointX.Velocity = 0f;
        slideJointZ.Velocity = 0f;
        targetPosition = GetCurrentPosition();
    }

    /// <summary>
    /// Control pusher agents with maximum velocity. 
    /// </summary>
    /// <param name="targetVelocity"></param>
    public void Act(Vector2 target)
    {
        if (actionType == ActionType.ContinuousVelocity)
        {
            pusherActuatorX.Control = target.x * maxVelocity;
            pusherActuatorZ.Control = target.y * maxVelocity;
        }
        else if(actionType == ActionType.ContinuousPosition)
        {
            float x_mapped = RemapValue(target.x, -1f, 1f, -30, 30);
            float z_mapped = RemapValue(target.y, -1f, 1f, -23, 40);

            pusherActuatorX.Control = x_mapped;
            pusherActuatorZ.Control = z_mapped;
        }

    }

    private static float RemapValue(float x, float min_old, float max_old, float min_new, float max_new)
    {
        return (x - min_old)/(max_old - min_old)*(max_new - min_new) + min_new;
    }

    /// <summary>
    /// Get current pusher position.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetCurrentPosition()
    {
        return new Vector2(transform.position.x, transform.position.z);
    }

    /// <summary>
    /// Get current pusher velocity.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetCurrentVelocity()
    {
        return new Vector2(pusherActuatorX.Velocity, pusherActuatorZ.Velocity);
    }

    /// <summary>
    /// Get current pusher acceleration.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetCurrentAcceleration()
    {
        return currentAcceleration;
    }

    /// <summary>
    /// Get distance between current pusher to the goal on the agent side.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetDistanceAgentGoal()
    {
        Vector2 goalPos = new Vector2(agentGoalPos.x, agentGoalPos.z);
        return goalPos - GetCurrentPosition();
    }

    /// <summary>
    /// Get distance between current pusher to the goal on the human side.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetDistanceHumanGoal()
    {
        Vector2 goalPos = new Vector2(humanGoalPos.x, humanGoalPos.z);
        return goalPos - GetCurrentPosition();
    }

    /// <summary>
    /// Get distance between current pusher and puck.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetDistancePuck()
    {
        Vector2 pos = new Vector2(puckPos.x, puckPos.z);
        return pos - GetCurrentPosition();
    }

    private void FixedUpdate()
    {
        var currentVelocity = new Vector2(pusherActuatorX.Velocity, pusherActuatorZ.Velocity);
        currentAcceleration = currentVelocity - lastVelocity;
        lastVelocity = currentVelocity;
    }
}
