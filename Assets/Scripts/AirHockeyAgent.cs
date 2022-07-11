﻿using UnityEngine;
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
    [Header("Reward Composition")]
    [Tooltip("Agent scored a goal.")]
    [Range(0f, 10f)]
    [SerializeField] private float agentScoredReward;
    [Range(-10f, 0f)]
    [SerializeField] private float humanScoredReward;
    [Range(-1f, 0f)]
    [SerializeField] private float avoidBoundariesReward;
    [Range(-1f, 0f)]
    [SerializeField] private float avoidDirectionChangesReward;
    [Range(0f, 1f)]
    [SerializeField] private float encouragePuckMovementReward;
    [Range(-1f, 0f)]
    [SerializeField] private float puckStopReward;
    [Range(0f, 5f)]
    [SerializeField] private float backWallReward;
    [SerializeField] private bool endOnBackWall;
    [Range(0f, 1f)]
    [SerializeField] private float deflectOnlyReward;
    [Range(-1f, 0f)]
    [SerializeField] private float puckInAgentsHalfReward;
    [Range(-5f, 0f)]
    [SerializeField] private float maxStepReward;
    [Range(-1f, 0f)]
    [SerializeField] private float stepReward;


    // Used to track the influence of each reward component in each episode
    public Dictionary<string, float> episodeReward;

    
    #endregion

    #region Private Parameters
    private SceneController sceneController;
    private PuckController puckController;
    private PusherController pusherAgentController;
    private PusherController pusherHumanController;
    private Rigidbody guidanceRods;
    private const float PusherOffsetY = 0.01f;

    private Vector3 startingPosition;
    private Vector3 lastDirection;
    private Vector3 position;

    private int gamesSinceNewEpisode;
    private int lastGameReset;
    #endregion

    private void ResetEpisodeRewards()
    {
        episodeReward = new Dictionary<string, float>();
        episodeReward["StepReward"] = 0f;
        episodeReward["DirectionReward"] = 0f;
        episodeReward["BoundaryReward"] = 0f;
        episodeReward["PuckVelocityReward"] = 0f;
        episodeReward["PuckInAgentsHalfReward"] = 0f;
        episodeReward["ScoreReward"] = 0f;
        episodeReward["BackwallReward"] = 0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize Reward Dictionary
        ResetEpisodeRewards();

        // Get the controllers for scene, puck and the two pushers
        sceneController = GetComponent<SceneController>();
        pusherAgentController = GameObject.Find("PusherAgent").GetComponent<PusherController>();
        puckController = GameObject.Find("Puck").GetComponent<PuckController>();
        pusherHumanController = GameObject.Find("PusherHuman").GetComponent<PusherController>();

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
        print(dict_string);
        sceneController.ResetScene();
        ResetEpisodeRewards();
    }

    public void ResetGameWithoutNewEpisode()
    {
        sceneController.ResetScene();
        gamesSinceNewEpisode++;
        lastGameReset = StepCount;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(pusherAgentController.GetCurrentPosition());
        sensor.AddObservation(pusherAgentController.GetCurrentVelocity());
        sensor.AddObservation(pusherHumanController.GetCurrentPosition());
        sensor.AddObservation(pusherAgentController.GetCurrentVelocity());
        sensor.AddObservation(puckController.GetCurrentPosition());
        sensor.AddObservation(puckController.GetCurrentVelocity());
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

        #region UniversalRewards

        // Rewards that are available for all Task Types
        // PUCK CLIPPED OUT OF FIELD
        if(!sceneController.puckIsInGame())
        {
            print("PUCK OUT OF BOUNDS!!!");
            EndEpisode();
            return;
        }

        // AGENT SCORED
        if (sceneController.CurrentGameState == GameState.agentScored)
        {
            episodeReward["ScoreReward"] += agentScoredReward;
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
        // PUCK REACHED OPPONENT'S BACKWALL
        else if (sceneController.CurrentGameState == GameState.backWallReached)
        {
            sceneController.CurrentGameState = GameState.normal;
            if(backWallReward > 0)
            {
                episodeReward["BackwallReward"] += backWallReward;
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
        // PUCK IS IN AGENT'S HALF
        if (puckInAgentsHalfReward < 0f)
        {
            // Get Puck Position
            var puckPosition = puckController.GetCurrentPosition();
            if(puckPosition.y > 0f)
            {
                episodeReward["PuckInAgentsHalfReward"] += puckInAgentsHalfReward; 
                AddReward(puckInAgentsHalfReward);
            }
        }
        // Punish changing direction too much.
        if (avoidDirectionChangesReward < 0f)
        {
            var direction = new Vector2(x, z);
            if(lastDirection.magnitude > 0f && direction.magnitude > 0f)
            {
                float cosAngle = Vector2.Dot(direction, lastDirection) / (lastDirection.magnitude * direction.magnitude);
                cosAngle = Mathf.Clamp(cosAngle, -1, 1);
                float angle = Mathf.Acos(cosAngle);
                AddReward(angle * avoidDirectionChangesReward);
                episodeReward["DirectionReward"] += angle * avoidDirectionChangesReward;
            }
            lastDirection = direction;
        }

        // Punish running into boundaries.
        if (avoidBoundariesReward < 0f)
        {
            var agentPosition = pusherAgentController.GetCurrentPosition();

            if (agentPosition.x < -29.9f || agentPosition.x > 29.9f ||
                agentPosition.y > 68.7f || agentPosition.y < 5.8f)
            {
                AddReward(avoidBoundariesReward);
                episodeReward["BoundaryReward"] += avoidBoundariesReward;
            }
        }
        // Reward high puck velocities
        if (encouragePuckMovementReward > 0f)
        {
            var puckVelocity = puckController.GetCurrentVelocity()/100;
            AddReward(Mathf.Clamp(puckVelocity.magnitude * encouragePuckMovementReward, 0f, 1f));
            episodeReward["PuckVelocityReward"] += Mathf.Clamp(puckVelocity.magnitude * encouragePuckMovementReward, 0f, 1f);
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
                SetReward(maxStepReward);
                episodeReward["StepReward"] += stepReward;
                return;
            }
        }
        else if(taskType == TaskType.FullGameMultipleGoals)
        {
            if(StepCount - lastGameReset > maxStepsPerGame)
            {
                SetReward(maxStepReward);
                episodeReward["StepReward"] += stepReward;
                ResetGameWithoutNewEpisode();
                return;
            }
        }
        #endregion
        #endregion

        #region Movement and Clipping
        pusherAgentController.Act(new Vector2(x, z));
        #endregion
    }

    void FixedUpdate()
    {
        //guidanceRods.position = new Vector3(0, 0, pusherAgentController.transform.position.z);
        //guidanceRods.velocity = new Vector3(0, 0, agentRB.velocity.z);
    }
}
