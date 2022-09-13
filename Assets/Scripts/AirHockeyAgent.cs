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

public enum ActionType { Discrete, Continuous };
public enum TaskType
{
    FullGameUntilGoal,
    FullGameMultipleGoals
}

public enum HumanBehavior
{
    None,
    StartingPosition,
    RandomPosition,
    OscillatingMovement,
    Heuristic,
    Selfplay,
    ManualMovement,
    TrainingScenario,
}

public enum ObservationSpace
{
    KinematicNoAccel,
    Kinematic,
    Full,
}

public class AirHockeyAgent : Agent
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
    [Header("Human Pusher Behavior")]
    public HumanBehavior humanBehavior;

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

    // Used to track the influence of each reward component in each episode
    public Dictionary<string, float> episodeReward;
    public Dictionary<string, float[]> episodeRewardShift;

    
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

    private void ResetEpisodeRewards()
    {
        shiftIdx = 0;
        episodeRewardShift = new Dictionary<string, float[]>();
        episodeRewardShift["DirectionRewardShift"] = new float[shiftLen];
        episodeRewardShift["BoundaryRewardShift"] = new float[shiftLen];
        episodeRewardShift["PuckVelocityRewardShift"] = new float[shiftLen];
        episodeRewardShift["PuckInAgentsHalfRewardShift"] = new float[shiftLen];
        episodeRewardShift["ScoreRewardShift"] = new float[shiftLen];
        episodeRewardShift["BackwallRewardShift"] = new float[shiftLen];
        episodeRewardShift["StayInCenterRewardShift"] = new float[shiftLen];

        episodeReward = new Dictionary<string, float>();
        episodeReward["StepReward"] = 0f;
        episodeReward["DirectionReward"] = 0f;
        episodeReward["BoundaryReward"] = 0f;
        episodeReward["PuckVelocityReward"] = 0f;
        episodeReward["PuckInAgentsHalfReward"] = 0f;
        episodeReward["ScoreReward"] = 0f;
        episodeReward["BackwallReward"] = 0f;
        episodeReward["OutOfBounds"] = 0f;
        episodeReward["StayInCenterReward"] = 0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        SetupAirHockeyAgent();
    }

    public void SetupAirHockeyAgent()
    {
        // Initialize Reward Dictionary
        ResetEpisodeRewards();

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
        //guidanceRods = GameObject.Find("GuidanceRods").GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        string dict_string = "";
        foreach (KeyValuePair<string, float> kvp in episodeReward)
        {
            dict_string += kvp.Key + ": " + kvp.Value + ";   ";
        }
        //print(dict_string);
        sceneController.ResetScene(false);
        ResetEpisodeRewards();

        if(episodesPlayed % 15 == 0)
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
                sensor.AddObservation(puckController.GetCurrentPosition());
                sensor.AddObservation(puckController.GetCurrentVelocity());
                sensor.AddObservation(puckController.GetCurrentAccelaration());
                break;
            case ObservationSpace.Full:
                sensor.AddObservation(pusherAgentController.GetCurrentPosition());
                sensor.AddObservation(pusherAgentController.GetCurrentVelocity());
                sensor.AddObservation(pusherAgentController.GetCurrentAccelaration());
                sensor.AddObservation(pusherHumanController.GetCurrentPosition());
                sensor.AddObservation(pusherHumanController.GetCurrentVelocity());
                sensor.AddObservation(pusherHumanController.GetCurrentAccelaration());
                sensor.AddObservation(puckController.GetCurrentPosition());
                sensor.AddObservation(puckController.GetCurrentVelocity());
                sensor.AddObservation(puckController.GetCurrentAccelaration());

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
        if(actionType == ActionType.Continuous)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            continuousActionsOut[0] = horizontalInput;
            continuousActionsOut[1] = verticalInput;
        }
        else
        {
            var discreteActionsOut = actionsOut.DiscreteActions;
            int horizontalInput = Mathf.RoundToInt(Input.GetAxis("Horizontal"));
            int verticalInput = Mathf.RoundToInt(Input.GetAxis("Vertical"));

            if (horizontalInput == 1)
            {
                discreteActionsOut[0] = 1;
            }
            else if(horizontalInput == -1)
            {
                discreteActionsOut[0] = 2;
            }
            else if(verticalInput == 1)
            {
                discreteActionsOut[0] = 3;
            }
            else if(verticalInput == -1)
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

        if(actionType == ActionType.Continuous)
        {
            var continouosActions = actionsIn.ContinuousActions;
            // MOVEMENT CALCULATIONS
            x = continouosActions[0];
            z = continouosActions[1];
        }
        else
        {
            var discreteActions = actionsIn.DiscreteActions;
            
            switch(discreteActions[0])
            {
                case 0:
                    x = 0;
                    z = 0;
                    break;
                case 1:
                    x = 1;
                    z = 0;
                    break;
                case 2:
                    x = -1;
                    z = 0;
                    break;
                case 3:
                    x = 0;
                    z = 1;
                    break;
                case 4:
                    x = 0;
                    z = -1;
                    break;
            }
        }

        // Action Dead Zone to avoid unneccessary movement
        if (Mathf.Abs(x) < 0.03f)
        {
            x = 0f;
        }
        if (Mathf.Abs(z) < 0.03f)
        {
            z = 0f;
        }
        #endregion

        #region RewardComposition
        if (shiftIdx >= shiftLen)
        {
            shiftIdx = 0;
        }
        episodeRewardShift["DirectionRewardShift"][shiftIdx] = 0;
        episodeRewardShift["BoundaryRewardShift"][shiftIdx] = 0;
        episodeRewardShift["PuckVelocityRewardShift"][shiftIdx] = 0;
        episodeRewardShift["PuckInAgentsHalfRewardShift"][shiftIdx] = 0;
        episodeRewardShift["ScoreRewardShift"][shiftIdx] = 0;
        episodeRewardShift["BackwallRewardShift"][shiftIdx] = 0;
        episodeRewardShift["StayInCenterRewardShift"][shiftIdx] = 0;

        #region UniversalRewards

        // Rewards that are available for all Task Types
        // PUCK CLIPPED OUT OF FIELD
        if (!sceneController.puckIsInGame())
        {
            print("PUCK OUT OF BOUNDS!!!");
            AddReward(outOfBoundsReward);
            episodeReward["OutOfBounds"] += outOfBoundsReward;
            EndEpisode();
            return;
        }

        // AGENT SCORED
        if (sceneController.CurrentGameState == GameState.agentScored)
        {
            episodeReward["ScoreReward"] += agentScoredReward;
            episodeRewardShift["ScoreRewardShift"][shiftIdx] = agentScoredReward;
            SetReward(agentScoredReward);
            
            if(taskType != TaskType.FullGameMultipleGoals || gamesSinceNewEpisode >= 9)
            {
                EndEpisode();
                return;
            }
            else
            {
                ResetGameWithoutNewEpisode();
                return;
            }
        }
        // HUMAN SCORED
        else if (sceneController.CurrentGameState == GameState.playerScored)
        {
            episodeReward["ScoreReward"] += humanScoredReward;
            episodeRewardShift["ScoreRewardShift"][shiftIdx] = humanScoredReward;
            SetReward(humanScoredReward);

            if (taskType != TaskType.FullGameMultipleGoals || gamesSinceNewEpisode >= 9)
            {
                EndEpisode();
                return;
            }
            else
            {
                ResetGameWithoutNewEpisode();
                return;
            }
        }
        // Reward the puck touching the opponent's back wall.
        else if (sceneController.CurrentGameState == GameState.backWallReached)
        {
            sceneController.CurrentGameState = GameState.normal;
            if(backWallReward > 0)
            {
                episodeReward["BackwallReward"] += backWallReward;
                episodeRewardShift["BackwallRewardShift"][shiftIdx] = backWallReward;
                AddReward(backWallReward);

                if (endOnBackWall)
                {
                    if (taskType != TaskType.FullGameMultipleGoals || gamesSinceNewEpisode >= 9)
                    {
                        EndEpisode();
                        return;
                    }
                    else
                    {
                        ResetGameWithoutNewEpisode();
                        return;
                    }
                }
            }

        }
        // Punish if the puck is in the agent's half.
        if (puckInAgentsHalfReward < 0f)
        {
            // Get Puck Position
            var puckPosition = puckController.GetCurrentPosition();
            if(puckPosition.y > 0f)
            {
                episodeReward["PuckInAgentsHalfReward"] += puckInAgentsHalfReward;
                episodeRewardShift["PuckInAgentsHalfRewardShift"][shiftIdx] = puckInAgentsHalfReward;
                AddReward(puckInAgentsHalfReward);
            }
        }
        // Punish changing direction too much.
        if (avoidDirectionChangesReward < 0f)
        {
            var currentVel = pusherAgentController.GetCurrentVelocity();
            currentVelMag = currentVel.magnitude;
            var currentAcc = lastVel - currentVel;
            currentAccMag = currentAcc.magnitude;

            var currentJerk = lastAcc - currentAcc;
            if((pusherAgentController.GetCurrentPosition() - puckController.GetCurrentPosition()).magnitude > 10)
            {
                currentJerkMag = currentJerk.magnitude / 100;
            }
            else
            {
                currentJerkMag = 0;
            }
            
            /*
            if (currentJerkMag > 10f)
            {
                var jerkMagClamped = Mathf.Clamp(currentJerkMag, 0f, 50f);
                AddReward(jerkMagClamped * avoidDirectionChangesReward);
                episodeReward["DirectionReward"] += jerkMagClamped * avoidDirectionChangesReward;
            }


            var currentVel = new Vector2(x, z);
            currentVelMag = currentVel.magnitude;
            var currentAcc = lastVel - currentVel;
            currentAccMag = currentAcc.magnitude;
            */
            //var currentJerk = lastAcc - currentAcc;
            //currentJerkMag = currentJerk.magnitude;
            AddReward(currentJerkMag * avoidDirectionChangesReward);
            episodeReward["DirectionReward"] += currentJerkMag * avoidDirectionChangesReward;
            episodeRewardShift["DirectionRewardShift"][shiftIdx] = currentJerkMag * avoidDirectionChangesReward;
            //csv.AppendLine(currentJerkMag.ToString());


            lastVel = currentVel;
            lastAcc = currentAcc;
        }
        // Punish running into boundaries.
        if (avoidBoundariesReward < 0f)
        {
            var agentPosition = pusherAgentController.GetCurrentPosition();
            // Hard boundaries: Left: -29.9f Right: 29.9f Top: 68.7f Bottom: 5.8f
            // Soft boundaries: Left: -24.9f Right: 24.9f Top: 63.7f Bottom: 10.8f
            if(agentPosition.x < -24.9f || agentPosition.x > 24.9f ||
                agentPosition.y > 63.7f || agentPosition.y < 10.8f)
            {
                float currentBoundaryRewardX = 0f;
                if (agentPosition.x < -24.9f || agentPosition.x > 24.9f) { currentBoundaryRewardX = Mathf.Clamp(Mathf.Abs(agentPosition.x * 0.2f) - 4.98f, 0f, 1f); }
                float currentBoundaryRewardY = 0f;
                if(agentPosition.y > 63.7f) { currentBoundaryRewardY = Mathf.Clamp(agentPosition.y * 0.2f - 12.74f, 0f, 1f); }
                else if(agentPosition.y < 10.8f) {currentBoundaryRewardY = Mathf.Clamp(- agentPosition.y * 0.2f + 2.16f, 0f, 1f); }
                //print(currentBoundaryRewardX.ToString("0.00") + " " + currentBoundaryRewardY.ToString("0.00"));
                var currentBoundaryReward = avoidBoundariesReward * Mathf.Sqrt(Mathf.Pow(currentBoundaryRewardX, 2) + Mathf.Pow(currentBoundaryRewardY, 2));
                AddReward(currentBoundaryReward);
                episodeReward["BoundaryReward"] += currentBoundaryReward;
                episodeRewardShift["BoundaryRewardShift"][shiftIdx] = currentBoundaryReward;
            }
            /*
            if (agentPosition.x < -29.9f || agentPosition.x > 29.9f ||
                agentPosition.y > 68.7f || agentPosition.y < 5.8f)
            {
                AddReward(avoidBoundariesReward);
                episodeReward["BoundaryReward"] += avoidBoundariesReward;
                episodeRewardShift["BoundaryRewardShift"][shiftIdx] = avoidBoundariesReward;
            }
            */
        }
        // Punish staying away from the center as soon as the puck is in the opponent's half.
        if (stayInCenterReward < 0f)
        {
            var puckPosition = puckController.GetCurrentPosition();
            if(puckPosition.y < 0f)
            {
                var agentPosition = pusherAgentController.GetCurrentPosition();
                // Hard boundaries: Left: -20f Right: 20f Top: 65f Bottom: 33f
                // Soft boundaries: Left: -10f Right: 10f Top: 55f Bottom: 43f
                if (agentPosition.y < 43f || agentPosition.y > 55f || agentPosition.x < -10f || agentPosition.x > 10f)
                {
                    float currentCenterRewardX = 0f;
                    if (agentPosition.x < -10f || agentPosition.x > 10f) { currentCenterRewardX = Mathf.Clamp(Mathf.Abs(agentPosition.x * 0.1f) - 1f, 0f, 1f); }
                    float currentCenterRewardY = 0f;
                    if (agentPosition.y > 55f) { currentCenterRewardY = Mathf.Clamp(agentPosition.y * 0.1f - 5.5f, 0f, 1f); }
                    else if (agentPosition.y < 43f) { currentCenterRewardY = Mathf.Clamp(-agentPosition.y * 0.1f + 4.3f, 0f, 1f); }

                    //print("CENTER: " + currentCenterRewardX.ToString("0.00") + " " + currentCenterRewardY.ToString("0.00"));
                    var currentCenterReward = stayInCenterReward * 0.1f * Mathf.Sqrt(Mathf.Pow(currentCenterRewardX, 2) + Mathf.Pow(currentCenterRewardY, 2));
                    AddReward(currentCenterReward);
                    episodeReward["StayInCenterReward"] += currentCenterReward;
                    episodeRewardShift["StayInCenterRewardShift"][shiftIdx] = currentCenterReward;
                }
            }
        }
        // Reward high puck velocities
        if (encouragePuckMovementReward > 0f)
        {
            var puckVelocity = puckController.GetCurrentVelocity()/100;
            AddReward(Mathf.Clamp(puckVelocity.magnitude * encouragePuckMovementReward, 0f, 1f));
            episodeReward["PuckVelocityReward"] += Mathf.Clamp(puckVelocity.magnitude * encouragePuckMovementReward, 0f, 1f);
            episodeRewardShift["PuckVelocityRewardShift"][shiftIdx] = Mathf.Clamp(puckVelocity.magnitude * encouragePuckMovementReward, 0f, 1f);
        }
        // STEP REWARD
        AddReward(stepReward);
        episodeReward["StepReward"] += stepReward;
        #endregion
        
        #region TaskSpecificRewards
        // Task specific Rewards
        if(taskType == TaskType.FullGameUntilGoal)
        {
            if (StepCount == MaxStep)
            {
                AddReward(maxStepReward);
                episodeReward["StepReward"] += stepReward;
                return;
            }
        }
        else if(taskType == TaskType.FullGameMultipleGoals)
        {
            if(StepCount - lastGameReset > maxStepsPerGame)
            {
                AddReward(maxStepReward);
                episodeReward["StepReward"] += stepReward;
                ResetGameWithoutNewEpisode();
                return;
            }
        }
        #endregion
        shiftIdx++;
        #endregion

        #region Movement and Clipping
        //print((puckController.GetCurrentPosition() - pusherAgentController.GetCurrentPosition()).magnitude);
        pusherAgentController.Act(new Vector2(x, z));
        #endregion
    }

    void FixedUpdate()
    {
        //guidanceRods.position = new Vector3(0, 0, pusherAgentController.transform.position.z);
        //guidanceRods.velocity = new Vector3(0, 0, agentRB.velocity.z);
    }

    public void GetRewardComposition(out Dictionary<string, float> dict1, out Dictionary<string, float[]> dict2)
    {
        dict1 = episodeReward;
        dict2 = episodeRewardShift;      
    }

    private void OnApplicationQuit()
    {
        //File.WriteAllText("AccelerationLog.csv", csv.ToString());
    }
}
