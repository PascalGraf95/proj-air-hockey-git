using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using Assets.Scripts;

public class TrainingSituation : MonoBehaviour
{
    enum TrainingSituations
    {
        RandomShot,
        RandomBoundaryShot,
        RandomShotOnGoal,
        RandomBoundaryShotOnGoal,
    }

    [Header("Training Situations")]
    [SerializeField]private TrainingSituations TrainingSituationChoice;
    public float MinPosX = -25f;
    public float MinPosZ = 60f;
    public float MaxPosX = 25f;
    public float MaxPosZ = 15f;
    public float MinPusherOffset = -10f;
    public float MaxPusherOffset = -20f;

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

    [Header("General")]
    public MjBody PusherBody;
    public MjBody PuckBody;
    public MjGeom PuckGeom;
    public MjActuator ActuatorZ;
    public MjActuator ActuatorX;
    public MjSlideJoint JointX;
    public MjSlideJoint JointZ;
    public Collider GoalCollider;
    public Collider BoundaryRightCollider;
    public Collider BoundaryLeftCollider;
    
    private const float PusherOffsetY = 0.01f;
    private const float PuckOffsetY = 0.5f;

    private Vector3 targetPos;
    private ArriveSteeringBehavior arriveSteering;
    private MjScene _scene;
    public Character Character;

    // Start is called before the first frame update
    void Start()
    {
        Character = new Character();
        targetPos = PusherBody.transform.position;
        arriveSteering = new ArriveSteeringBehavior();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // get pusher velocity and position
        Character.Velocity = new Vector3(ActuatorX.Velocity, 0, ActuatorZ.Velocity);
        Character.Position = PusherBody.transform.position;

        StartTraining();        

        // update Character values
        Character.Position = PusherBody.transform.position;
        Character.Velocity = new Vector3(ActuatorX.Velocity, 0, ActuatorZ.Velocity);

        Vector3 acceleration = arriveSteering.Arrive(targetPos, Character.Position, Character.Velocity, TargetRadius, SlowDownRadius, MaxSpeed, MaxAcceleration, TimeToTarget);

        // update Character values
        Character.Position = PusherBody.transform.position;
        Character.Velocity = new Vector3(ActuatorX.Velocity, 0, ActuatorZ.Velocity);
        Character.Accelaration = acceleration;

        // set actuator acceleration
        ActuatorX.Control = acceleration.x;
        ActuatorZ.Control = acceleration.z;
    }

    public void StartTraining(bool triggerByScript = false)
    {
        // create new training situation
        Vector3 minPos = new Vector3(MinPosX, PusherOffsetY, MinPosZ);
        Vector3 maxPos = new Vector3(MaxPosX, PusherOffsetY, MaxPosZ);
        Vector3 minPusherOffset = new Vector3(MinPusherOffset, 0, MinPusherOffset);
        Vector3 maxPusherOffset = new Vector3(MinPusherOffset, 0, MaxPusherOffset);

        if (Input.GetKey(KeyCode.T))
        {
            targetPos = CreateTrainingSituation(minPos, maxPos, minPusherOffset, maxPusherOffset);
        }
        else if (triggerByScript)
        {
            targetPos = CreateTrainingSituation(minPos, maxPos, minPusherOffset, maxPusherOffset);
        }

    }

    private Vector3 CreateTrainingSituation(Vector3 minPuckPos, Vector3 maxPuckPos, Vector3 minPusherOffset, Vector3 maxPusherOffset)
    {
        // set puck position
        Vector3 puckStartPosition = new Vector3(Random.Range(minPuckPos.x, maxPuckPos.x),
                    PuckOffsetY, Random.Range(minPuckPos.z, maxPuckPos.z));
        PuckBody.transform.position = puckStartPosition;
        
        Vector3 direction;
        targetPos = puckStartPosition;
        Ray ray;
        float angle;

        switch (TrainingSituationChoice)
        {
            case TrainingSituations.RandomShot:
                // set pusher position                
                PusherBody.transform.position = puckStartPosition + new Vector3(0,
                    PusherOffsetY, Random.Range(minPusherOffset.z, maxPusherOffset.z)); ;

                // create new mujoco scene
                Reset();

                // create random offset in X direction within radius of puck to hit it from different angles
                float targetPosOffsetX = Random.Range(-PuckGeom.Cylinder.Radius / 1, PuckGeom.Cylinder.Radius / 1);

                // set target position for pusher
                targetPos = puckStartPosition + new Vector3(targetPosOffsetX, PusherOffsetY, 0);
                break;
            case TrainingSituations.RandomBoundaryShot:
                // get positions of lateral boundaries
                Vector3 boundaryRightPos = BoundaryRightCollider.transform.position;
                Vector3 boundaryLeftPos = BoundaryLeftCollider.transform.position;
                // randomize which side will be hit
                List<Vector3> sides = new List<Vector3>();
                sides.Add(boundaryLeftPos);
                sides.Add(boundaryRightPos);
                System.Random random = new System.Random();
                int side = random.Next(sides.Count);

                direction = (sides[side] - puckStartPosition).normalized;
                angle = Random.Range(-15f, 15f);
                direction = Quaternion.AngleAxis(angle, Vector3.up) * direction;
                // make sure direction vector still intersects with boundary collider
                ray = new Ray() { direction = direction, origin = puckStartPosition };
                Debug.DrawRay(puckStartPosition, direction, Color.blue, 50f);
                if (!BoundaryLeftCollider.Raycast(ray, out RaycastHit hitData, 1000f) && !BoundaryRightCollider.Raycast(ray, out RaycastHit hitData2, 1000f))
                {
                    // randomize angle
                    direction = Quaternion.AngleAxis(Random.Range(-15f, 15f), Vector3.up) * direction;
                    ray.direction = direction;
                }
                else
                {
                    PusherBody.transform.position = puckStartPosition + new Vector3(Random.Range(minPusherOffset.x, maxPusherOffset.x) * direction.x,
                        PusherOffsetY, Random.Range(minPusherOffset.z, maxPusherOffset.z) * direction.z);

                    Reset();
                }

                break;
            case TrainingSituations.RandomShotOnGoal:
                Vector3 goalPos = GoalCollider.transform.position;
                direction = (goalPos - puckStartPosition).normalized;
                angle = Random.Range(-15f, 15f);
                direction = Quaternion.AngleAxis(angle, Vector3.up) * direction;
                // make sure direction vector still intersects with goal collider
                ray = new Ray() { direction = direction, origin = puckStartPosition };
                if (!GoalCollider.Raycast(ray, out RaycastHit hitData3, 1000f))
                {
                    // randomize angle
                    direction = Quaternion.AngleAxis(Random.Range(-15f, 15f), Vector3.up) * direction;
                    ray.direction = direction;
                    //Debug.DrawRay(puckStartPosition, direction, Color.green, 100f);
                }
                else
                {
                    PusherBody.transform.position = puckStartPosition + new Vector3(Random.Range(minPusherOffset.x, maxPusherOffset.x) * direction.x,
                    PusherOffsetY, Random.Range(minPusherOffset.z, maxPusherOffset.z) * direction.z);

                    //Debug.DrawRay(puckStartPosition, direction, Color.green, 100f);

                    Reset();
                }               

                break;
            case TrainingSituations.RandomBoundaryShotOnGoal:
                break;
            default:
                break;
        }

        // DEBUG
        //targetPos = PusherBody.transform.position;

        return targetPos;
    }    

    public void Reset()
    {
        // reset velocity
        JointX.Velocity = 0f;
        JointZ.Velocity = 0f;

        // reset sensor readings
        JointX.Configuration = 0f;
        JointZ.Configuration = 0f;

        if (_scene == null)
        {
            _scene = GameObject.Find("MjScene").GetComponent<MjScene>();
        }
        _scene.DestroyScene();
        _scene.CreateScene();
    }
}

