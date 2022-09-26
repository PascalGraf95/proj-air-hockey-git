using UnityEngine;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System.IO;
using Mujoco;
using Assets.Scripts;
using System;

public class MinimumAgentScript : Agent
{
    #region Public Parameters
    // PUBLIC
    [Space(5)]
    [Header("Learning Agent Parameters")]
    [Tooltip("Choose between Discrete and Continuous action space. This depends on your training algorithm.")]
    public ActionType actionType;

    [Space(5)]
    [Header("Training Scenario")]
    public TaskType taskType;
    public ResetPuckState resetPuckState;
    public int maxStepsPerGame;

    [Space(5)]
    [Header("Observation Space")]
    public ObservationSpace observationSpace;

    [Space(5)]
    [Header("Reward Composition")]
    [Tooltip("Agent scored a goal.")]
    [Range(0f, 10f)]
    [SerializeField] private float agentScoredReward;
    [Range(-10f, 0f)]
    [SerializeField] private float humanScoredReward;
    [Range(-0.1f, 0f)]
    [SerializeField] private float avoidBoundariesReward;
    [Range(-1f, 0f)]
    [SerializeField] private float avoidDirectionChangesReward;
    [Range(0f, 0.1f)]
    [SerializeField] private float encouragePuckMovementReward;
    [Range(-1f, 0f)]
    [SerializeField] private float puckStopReward;
    [Range(0f, 5f)]
    [SerializeField] private float backWallReward;
    [SerializeField] private bool endOnBackWall;
    [Range(0f, 1f)]
    [SerializeField] private float deflectOnlyReward;
    [Range(-0.1f, 0f)]
    [SerializeField] private float puckInAgentsHalfReward;
    [Range(-5f, 0f)]
    [SerializeField] private float maxStepReward;
    [Range(-1f, 0f)]
    [SerializeField] private float stepReward;
    [Range(-10f, 0f)]
    [SerializeField] private float outOfBoundsReward;
    [Range(-0.1f, 0f)]
    [SerializeField] private float stayInCenterReward;


    #endregion

    #region Private Parameters
    private SceneController sceneController;
    private PuckController puckController;
    private PusherController pusherAgentController;
    private PusherController pusherHumanController;
    private Rigidbody guidanceRods;
    private const float PusherOffsetY = 0.01f;

    private Vector2 lastVel;
    private Vector2 lastDirection;
    private Vector2 lastAcc;
    public float currentVelMag;
    public float currentAccMag;
    public float currentJerkMag;

    private int gamesSinceNewEpisode;
    private int lastGameReset;
    private int shiftIdx;
    private int shiftLen = 100;

    private int episodesPlayed = 0;

    //StringBuilder csv = new StringBuilder();
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        SetupAirHockeyAgent();
    }

    public void SetupAirHockeyAgent()
    {
        // Get the controllers for scene, puck and the two pushers
        sceneController = GetComponent<SceneController>();
        pusherAgentController = GameObject.Find("PusherAgent").GetComponent<PusherController>();
        puckController = GameObject.Find("Puck").GetComponent<PuckController>();
        try
        {
            pusherHumanController = GameObject.Find("PusherHuman").GetComponent<PusherController>();
        }
        catch (NullReferenceException e)
        {
            pusherHumanController = GameObject.Find("PusherHumanSelfplay").GetComponent<PusherController>();
        }


        // Get Guidance Rods and UI
        guidanceRods = GameObject.Find("GuidanceRods").GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        sceneController.ResetScene(false);
        if (episodesPlayed % 15 == 0)
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
        episodesPlayed++;
    }

    public void ResetGameWithoutNewEpisode()
    {
        sceneController.ResetScene(false);
        gamesSinceNewEpisode++;
        lastGameReset = StepCount;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        switch (observationSpace)
        {
            case ObservationSpace.KinematicNoAccel:
                sensor.AddObservation(pusherAgentController.GetCurrentPosition());
                sensor.AddObservation(pusherAgentController.GetCurrentVelocity());
                sensor.AddObservation(pusherHumanController.GetCurrentPosition());
                sensor.AddObservation(pusherHumanController.GetCurrentVelocity());
                sensor.AddObservation(puckController.GetCurrentPosition());
                sensor.AddObservation(puckController.GetCurrentVelocity());
                break;
            case ObservationSpace.Kinematic:
                sensor.AddObservation(pusherAgentController.GetCurrentPosition());
                sensor.AddObservation(pusherAgentController.GetCurrentVelocity());
                sensor.AddObservation(pusherAgentController.GetCurrentAccelaration());
                sensor.AddObservation(pusherHumanController.GetCurrentPosition());
                sensor.AddObservation(pusherHumanController.GetCurrentVelocity());
                sensor.AddObservation(pusherHumanController.GetCurrentAccelaration());
                sensor.AddObservation(0);
                sensor.AddObservation(0);
                sensor.AddObservation(0);
                break;
            case ObservationSpace.Full:
                sensor.AddObservation(pusherAgentController.GetCurrentPosition());
                sensor.AddObservation(pusherAgentController.GetCurrentVelocity());
                sensor.AddObservation(pusherAgentController.GetCurrentAccelaration());
                sensor.AddObservation(pusherHumanController.GetCurrentPosition());
                sensor.AddObservation(pusherHumanController.GetCurrentVelocity());
                sensor.AddObservation(pusherHumanController.GetCurrentAccelaration());
                sensor.AddObservation(0);
                sensor.AddObservation(0);
                sensor.AddObservation(0);

                sensor.AddObservation(pusherAgentController.GetDistanceAgentGoal());
                sensor.AddObservation(pusherHumanController.GetDistanceHumanGoal());
                sensor.AddObservation(pusherAgentController.GetDistancePuck());
                sensor.AddObservation(pusherHumanController.GetDistancePuck());
                break;
            default:
                break;
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (actionType == ActionType.Continuous)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            continuousActionsOut[0] = horizontalInput;
            continuousActionsOut[1] = verticalInput;
        }
        else
        {
            var discreteActionsOut = new List<float> { 1f }; //actionsOut.DiscreteActions;
            int horizontalInput = Mathf.RoundToInt(Input.GetAxis("Horizontal"));
            int verticalInput = Mathf.RoundToInt(Input.GetAxis("Vertical"));

            if (horizontalInput == 1)
            {
                discreteActionsOut[0] = 1;
            }
            else if (horizontalInput == -1)
            {
                discreteActionsOut[0] = 2;
            }
            else if (verticalInput == 1)
            {
                discreteActionsOut[0] = 3;
            }
            else if (verticalInput == -1)
            {
                discreteActionsOut[0] = 4;
            }
            else
            {
                discreteActionsOut[0] = 0;
            }
        }

    }


    public override void OnActionReceived(ActionBuffers actionsIn)
    {
        #region Action Calculations
        float x = 0f;
        float z = 0f;

        #region Movement and Clipping
        //pusherAgentController.Act(new Vector2(x, z));
        #endregion
        #endregion
    }

    void FixedUpdate()
    {
        guidanceRods.position = new Vector3(0, 0, pusherAgentController.transform.position.z);
        //guidanceRods.velocity = new Vector3(0, 0, agentRB.velocity.z);
    }
}
