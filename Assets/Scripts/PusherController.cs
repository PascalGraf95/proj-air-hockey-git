using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using Assets.Scripts;

enum ControlMode
{
    Keyboard,
    Click,
    Mouse
}

public class PusherController : MonoBehaviour
{
    //[SerializeField] private MjActuator pusherActuatorX;
    //[SerializeField] private MjActuator pusherActuatorZ;

    //[SerializeField] private MjSlideJoint slideJointX;
    //[SerializeField] private MjSlideJoint slideJointZ;

    //[SerializeField] private float acceleration;
    [SerializeField] private ControlMode controlMode;

    public MjBody PusherBody;
    public MjBody PuckBody;
    public MjGeom PuckGeom;
    public MjActuator ActuatorZ;
    public MjActuator ActuatorX;
    public MjSlideJoint JointX;
    public MjSlideJoint JointZ;
    public Collider ColliderPlane;

    private const float PusherOffsetY = 0.01f;
    private const float PuckOffsetY = 0.5f;

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

    // steering behavior properties
    public Character Character;
    private Vector3 targetPosition;
    private Vector3 accelaration;
    private ArriveSteeringBehavior arriveSteeringBehavior;

    // Start is called before the first frame update
    void Start()
    {
        Character = new Character();

        // set targetPosition to pusher position
        targetPosition = PusherBody.transform.position;
        accelaration = new Vector3();
        arriveSteeringBehavior = new ArriveSteeringBehavior();
    }

    // Update is called once per frame
    void Update()
    {
        float xInput;
        float zInput;

        switch (controlMode)
        {
            case ControlMode.Keyboard:
                xInput = Input.GetAxis("Horizontal");
                zInput = Input.GetAxis("Vertical");

                if (xInput > 0 && zInput > 0)
                {
                    ActuatorX.Control = xInput * accelaration.x;
                    ActuatorZ.Control = -zInput * accelaration.z;
                }
                break;
            case ControlMode.Click:
                break;
            case ControlMode.Mouse:
                // get pusher velocity and position
                Character.Velocity = new Vector3(ActuatorX.Velocity, 0, ActuatorZ.Velocity);
                Character.Position = PusherBody.transform.position;

                // get current mouse position on left mouse button click
                if (Input.GetMouseButton(0))
                {
                    targetPosition = GetMousePosition();
                }             
                               
                // compute arrive steering behavior
                accelaration = arriveSteeringBehavior.Arrive(targetPosition, Character, TargetRadius, SlowDownRadius, MaxSpeed, MaxAcceleration, TimeToTarget);

                // update Character values
                Character.Position = PusherBody.transform.position;
                Character.Velocity = new Vector3(ActuatorX.Velocity, 0, ActuatorZ.Velocity);
                Character.Accelaration = accelaration;

                // set actuator acceleration
                ActuatorX.Control = accelaration.x;
                ActuatorZ.Control = accelaration.z;
                break;
        }
        // compute arrive steering behavior
        accelaration = arriveSteeringBehavior.Arrive(targetPosition, Character, TargetRadius, SlowDownRadius, MaxSpeed, MaxAcceleration, TimeToTarget);
        // update Character values
        Character.Position = PusherBody.transform.position;
        Character.Velocity = new Vector3(ActuatorX.Velocity, 0, ActuatorZ.Velocity);
        Character.Accelaration = accelaration;
    }

    /// <summary>
    /// Gets the current mouse position in relation to a plane.
    /// </summary>
    /// <returns>The target position</returns>
    private Vector3 GetMousePosition()
    {
        Vector3 mousePosWorld = new Vector3();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (ColliderPlane.Raycast(ray, out RaycastHit hitData, 1000f))
        {
            mousePosWorld = hitData.point;
        }
        Vector3 targetPosition = new Vector3(mousePosWorld.x, 0, mousePosWorld.z);
        return targetPosition;
    }
       
    public void Reset()
    {
        // reset velocity
        JointX.Velocity = 0f;
        JointZ.Velocity = 0f;

        // reset sensor readings
        JointX.Configuration = 0f;
        JointZ.Configuration = 0f;
    }
}
