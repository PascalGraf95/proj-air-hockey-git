using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using Assets.Scripts;

public class KinematicMovePusher : MonoBehaviour
{
    
    public MjBody PusherBody;
    public MjBody PuckBody;
    public MjGeom PuckGeom;
    public MjActuator ActuatorZ;
    public MjActuator ActuatorX;
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

    [Header("Training Situations")]
    public float MinPosX = -25f;
    public float MinPosZ = 60f;
    public float MaxPosX = 25f;
    public float MaxPosZ = 15f;
    public float MinPusherOffset = -10f;
    public float MaxPusherOffset = -20f;

    private Character Character;

    

    // define target position vector
    Vector3 targetPosition;

    private MjScene _scene;

    // Start is called before the first frame update
    void Start()
    {
        Character = new Character();
        // set targetPosition to pusher position
        targetPosition = PusherBody.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // get pusher velocity and position
        Character.Velocity = new Vector3(ActuatorX.Velocity, 0, ActuatorZ.Velocity);
        Character.Position = PusherBody.transform.position;        

        // get current mouse position on left mouse button click
        if (Input.GetMouseButton(0))
        {
            targetPosition = GetMousePosition();
        }

        // create new training situation
        Vector3 minPos = new Vector3(MinPosX, PusherOffsetY, MinPosZ);
        Vector3 maxPos = new Vector3(MaxPosX, PusherOffsetY, MaxPosZ);
        Vector3 minPusherOffset = new Vector3(0, 0, MinPusherOffset);
        Vector3 maxPusherOffset = new Vector3(0, 0, MaxPusherOffset);
        if (Input.GetKey(KeyCode.T))
        {
            targetPosition = CreateTrainingSituation(minPos, maxPos, minPusherOffset, maxPusherOffset);
        }

        // update Character values
        Character.Position = PusherBody.transform.position;
        Character.Velocity = new Vector3(ActuatorX.Velocity, 0, ActuatorZ.Velocity);

        // input.accelaration nochmal anschauen

        // compute arrive steering behavior
        Vector3 accelaration = Arrive(targetPosition, Character);

        // set actuator acceleration
        ActuatorX.Control = accelaration.x;
        ActuatorZ.Control = accelaration.z;
    }

    /// <summary>
    /// Returns the steering for a Character so it arrives at the target
    /// </summary>
    public Vector3 Arrive(Vector3 targetPosition, Character Character)
    {
        Debug.DrawLine(transform.position, targetPosition, Color.red, 0f, false);

        // get the direction to the target
        Vector3 targetVelocity = targetPosition - Character.Position;

        // Get the distance to the target
        float distance = targetVelocity.magnitude;

        // check if we have arrived
        if (distance < TargetRadius)
        {
            Character.Velocity = Vector3.zero;
            return Vector3.zero;
        }

        // if we are outside the slowRadius, then go max speed
        float targetSpeed;
        if (distance > SlowDownRadius)
        {
            targetSpeed = MaxSpeed;
        }
        else
        {
            targetSpeed = MaxSpeed * (distance / SlowDownRadius);
        }

        // Give targetVelocity the correct speed
        targetVelocity.Normalize();
        targetVelocity *= targetSpeed;

        // Calculate the linear acceleration we want
        Vector3 acceleration = targetVelocity - Character.Velocity;
        /* Rather than accelerate the Character to the correct speed in 1 second, 
         * accelerate so we reach the desired speed in timeToTarget seconds 
         * (if we were to actually accelerate for the full timeToTarget seconds). */
        acceleration *= 1 / TimeToTarget;

        // Make sure we are accelerating at max acceleration
        if (acceleration.magnitude > MaxAcceleration)
        {
            acceleration.Normalize();
            acceleration *= MaxAcceleration;
        }

        // add pusher y offset
        acceleration.y = PusherOffsetY;

        return acceleration;
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

    private Vector3 CreateTrainingSituation(Vector3 minPuckPos, Vector3 maxPuckPos, Vector3 minPusherOffset, Vector3 maxPusherOffset)
    {
        // set pusher and puck positions
        Vector3 puckStartPosition = new Vector3(Random.Range(minPuckPos.x, maxPuckPos.x),
            PuckOffsetY, Random.Range(minPuckPos.z, maxPuckPos.z));
        PuckBody.transform.position = puckStartPosition;
        PusherBody.transform.position = puckStartPosition + new Vector3(Random.Range(minPusherOffset.x, maxPusherOffset.x),
            PusherOffsetY, Random.Range(minPusherOffset.z, maxPusherOffset.z)); ;

        // create new mujoco scene
        Reset();

        // create random offset in X direction within radius of puck to hit it from different angles
        float targetPositionOffsetX = Random.Range(-PuckGeom.Cylinder.Radius / 1, PuckGeom.Cylinder.Radius / 1);

        // set target position for pusher
        Vector3 targetPosition = puckStartPosition + new Vector3(targetPositionOffsetX, PusherOffsetY, 0);

        return targetPosition;
    }

    private void Reset()
    {
        MjSlideJoint mjSlideJointX = GameObject.Find("JointX").GetComponent<MjSlideJoint>();
        MjSlideJoint mjSlideJointZ = GameObject.Find("JointZ").GetComponent<MjSlideJoint>();
        // reset velocity
        mjSlideJointX.Velocity = 0f;
        mjSlideJointZ.Velocity = 0f;

        // reset sensor readings
        mjSlideJointX.Configuration = 0f;
        mjSlideJointZ.Configuration = 0f;

        if (_scene == null)
        {
            _scene = GameObject.Find("MjScene").GetComponent<MjScene>();
        }
        _scene.DestroyScene();
        _scene.CreateScene();
    }
}

//public class Character
//{
//    public Vector3 Position;
//    public Vector3 Velocity;

//}

