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

public enum ActionType { Discrete, Continuous };
public enum TaskType
{
    ReachThePuck,
    FullGameUntilGoal,
    FullGameMultipleGoals
}

public enum ObservationType
{
    AgentPuckPos,
    AgentPuckPosVel,
    AgentPuckHumanPos,
    AgentPuckHumanPosVel,
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
    [Tooltip("Choose the type of observation the agent receives. Remember to adapt the space size in the Behavior Parameters.")]
    public ObservationType observationType;

    [Space(5)]
    [Header("Agent Pusher Steering Behavior")]
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
    public bool randomAgentPosition;

    [Space(5)]
    [Header("Training Scenario")]
    public TaskType taskType;
    public int maxStepsPerGame;
    public bool writeCSV;

    [Space(5)]
    [Header("Human Pusher Behavior")]
    public HumanBehavior humanBehavior;

    [Space(5)]
    [Header("Reward Composition")]
    [Tooltip("Agent scored a goal.")]
    [Range(0f, 10f)]
    public float agentScoredReward;
    [Tooltip("Human scored a goal.")]
    [Range(-10f, 0f)]
    public float humanScoredReward;
    //[Range(-1f, 0f)]
    //public float avoidBoundariesReward;
    [Tooltip("Avoid unnecessary movement and direction changes of the agent.")]
    [Range(-1f, 0f)]
    public float avoidDirectionChangesReward;
    [Tooltip("Reward the agent if it is aggressive and moves the puck.")]
    [Range(0f, 1f)]
    public float encouragePuckMovementReward;
    [Tooltip("Reward if the puck does not move anymore.")]
    [Range(-1f, 0f)]
    public float puckStopReward;
    [Tooltip("Reward the agent if it pushes the puck to the opposite back wall.")]
    [Range(0f, 5f)]
    public float backWallReward;
    public bool endOnBackWall;
    [Tooltip("Reward the agent if it deflects/touches the puck.")]
    [Range(0f, 1f)]
    public float deflectOnlyReward;
    [Range(-5f, 0f)]
    public float maxStepReward;
    [Range(-1f, 0f)]
    public float stepReward;

    public Dictionary<string, float> episodeReward;

    
    #endregion

    #region Private Parameters
    // PRIVATE
    private FieldBoundary agentBoundary;

    private MjBody agentBody;
    private MjBody puckBody;
    private MjBody humanBody;
    private PuckScript puck;
    private static TrainingSituation trainingSituation;
    private static PusherController pusherControllerAgent;
    private static PusherController pusherControllerHuman;
    private static SceneController sceneController;
    private ArriveSteeringBehavior arriveSteeringBehavior;
    private HumanPlayer humanPlayer;
    private Rigidbody guidanceRods;
    private const float PusherOffsetY = 0.01f;

    private Vector3 startingPosition;
    private Vector3 lastDirection;
    private Vector3 position;

    private int agentScore;
    private int humanScore;
    private Text scoreText;

    private int gamesSinceNewEpisode;
    private int lastGameReset;
    private StreamWriter writer;

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        // Initialize Reward Dictionary
        episodeReward = new Dictionary<string, float>();
        episodeReward["StepReward"] = 0f;
        episodeReward["DirectionReward"] = 0f;
        episodeReward["BoundaryReward"] = 0f;
        episodeReward["PuckVelocityReward"] = 0f;
        episodeReward["ScoreReward"] = 0f;

        // Get Rigidbodies, Puck and Human Player and initilize their scripts
        

        // Get Guidance Rods and UI
        guidanceRods = GameObject.Find("GuidanceRods").GetComponent<Rigidbody>();
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();

        
        if (humanBehavior == HumanBehavior.Selfplay)
        {
            //humanPlayerClone.Init(transform, humanRB, puckBody, observationType, actionType, humanBoundary, maxHumanPusherVelocity, maxHumanPusherAcceleration);
        }
        else if (humanBehavior == HumanBehavior.TrainingScenario)
        {
            // init Trainingsituation script for human agent
            trainingSituation.StartTraining(true);
        }
        

        if(writeCSV)
        {
            writer = new StreamWriter("./export.csv");
        }
    }

    public override void OnEpisodeBegin()
    {

        if (randomAgentPosition)
        {
            agentBody.transform.position = new Vector3(Random.Range(agentBoundary.xMin, agentBoundary.xMax), PusherOffsetY, Random.Range(agentBoundary.zMin, agentBoundary.zMax));
        }
        else
        {
            agentBody.transform.position = startingPosition;
        }

        // Player Position Reset
        //humanPlayer.ResetPosition();
        //puck.Reset();

        if (taskType == TaskType.FullGameMultipleGoals)
        {
            agentScore = 0;
            humanScore = 0;
            scoreText.text = "0:0";
            gamesSinceNewEpisode = 0;
            lastGameReset = 0;
        }

        /*
        print("StepReward:" + episodeReward["StepReward"]);
        print("DirectionReward:" + episodeReward["DirectionReward"]);
        print("BoundaryReward:" + episodeReward["BoundaryReward"]);
        print("PuckVelocityReward:" + episodeReward["PuckVelocityReward"]);
        print("ScoreReward:" + episodeReward["ScoreReward"]);
        */
        episodeReward = new Dictionary<string, float>();
        episodeReward["StepReward"] = 0f;
        episodeReward["DirectionReward"] = 0f;
        episodeReward["BoundaryReward"] = 0f;
        episodeReward["PuckVelocityReward"] = 0f;
        episodeReward["ScoreReward"] = 0f;
    }

    public void ResetGameWithoutNewEpisode()
    {
        //agentBody.velocity = Vector2.zero;

        if (randomAgentPosition)
        {
            agentBody.transform.position = new Vector3(Random.Range(agentBoundary.xMin, agentBoundary.xMax), 0f, Random.Range(agentBoundary.zMin, agentBoundary.zMax));
        }
        else
        {
            agentBody.transform.position = startingPosition;
        }
        // Player Position Reset
        humanBody.transform.position = startingPosition;
        puckBody.transform.position = Vector3.zero;
        sceneController.ResetScene();

        gamesSinceNewEpisode++;
        lastGameReset = StepCount;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(pusherControllerAgent.Character.Position);
        sensor.AddObservation(puck.Character.Position);

        switch (observationType)
        {
            case ObservationType.AgentPuckPos:
                // nothing to do
                break;
            case ObservationType.AgentPuckPosVel:
                sensor.AddObservation(puck.Character.Velocity);
                sensor.AddObservation(pusherControllerAgent.Character.Velocity);
                break;
            case ObservationType.AgentPuckHumanPos:
                if (humanBehavior == HumanBehavior.TrainingScenario)
                {
                    //sensor.AddObservation(trainingSituation.Character.Accelaration);
                    sensor.AddObservation(trainingSituation.Character.Velocity);
                    sensor.AddObservation(trainingSituation.Character.Position);
                }
                else
                {
                    //sensor.AddObservation(pusherControllerHuman.Character.Accelaration);
                    sensor.AddObservation(pusherControllerHuman.Character.Velocity);
                    sensor.AddObservation(pusherControllerHuman.Character.Position);
                }
                break;
            case ObservationType.AgentPuckHumanPosVel:
                if (humanBehavior == HumanBehavior.TrainingScenario)
                {
                    //sensor.AddObservation(trainingSituation.Character.Accelaration);
                    sensor.AddObservation(trainingSituation.Character.Velocity);
                    sensor.AddObservation(trainingSituation.Character.Position);
                }
                else
                {
                    //sensor.AddObservation(pusherControllerHuman.Character.Accelaration);
                    sensor.AddObservation(pusherControllerHuman.Character.Velocity);
                    sensor.AddObservation(pusherControllerHuman.Character.Position);
                }
                break;
            default:
                break;
        }


        //if (writeCSV)
        //{
        //    writer.WriteLine(transform.position[0].ToString().Replace(",", ".") + "," + transform.position[2].ToString().Replace(",", ".") + ";" + agentBody.velocity[0].ToString().Replace(",", ".") + "," + agentBody.velocity[2].ToString().Replace(",", "."));
        //}
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

        // Normalize so going diagonally doesn't speed things up
        Vector3 direction = new Vector3(x, 0f, z);
        if (direction.magnitude > 1f)
        {
            direction.Normalize();
        }
        #endregion

        #region RewardComposition

        #region UniversalRewards
        // Rewards that are available for all Task Types
        // AGENT SCORED
        if (puck.gameState == GameState.agentScored)
        {
            episodeReward["ScoreReward"] += agentScoredReward;
            SetReward(agentScoredReward);
            agentScore++;
            scoreText.text = humanScore.ToString() + ":" + agentScore.ToString();
            
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
        else if (puck.gameState == GameState.playerScored)
        {
            episodeReward["ScoreReward"] += humanScoredReward;
            SetReward(humanScoredReward);
            humanScore++;
            scoreText.text = humanScore.ToString() + ":" + agentScore.ToString();

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
        else if (puck.gameState == GameState.backWallReached && backWallReward > 0)
        {
            episodeReward["ScoreReward"] += backWallReward;
            AddReward(backWallReward);

            if(endOnBackWall)
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
            else
            {
                puck.gameState = GameState.normal;
            }

        }
        // PUCK HAS BEEN DEFLECTED INTO THE OPPONENT'S FIELD
        else if (puck.transform.position.z < 0 && puck.AgentContact == true && deflectOnlyReward > 0f)
        {
            SetReward(deflectOnlyReward);
            EndEpisode();
            return;
        }
        // PUCK HAS STOPPED MOVING
        if (puck.gameState == GameState.puckStopped && puckStopReward < 0f)
        {
            AddReward(puckStopReward);
        }
        // Punish changing direction too much.
        if (avoidDirectionChangesReward < 0f)
        {
            if(lastDirection.magnitude > 0f && direction.magnitude > 0f)
            {
                float cosAngle = Vector2.Dot(direction, lastDirection) / (lastDirection.magnitude * direction.magnitude);
                cosAngle = Mathf.Clamp(cosAngle, -1, 1);
                float angle = Mathf.Acos(cosAngle);
                AddReward(angle * avoidDirectionChangesReward);
                episodeReward["DirectionReward"] += angle * avoidDirectionChangesReward;
            }
        }
        lastDirection = direction;
        //// Punish running into boundaries.
        //if (avoidBoundariesReward < 0f)
        //{
        //    if (agentBody.transform.position.x < agentBoundary.xMin || agentBody.transform.position.x > agentBoundary.xMax || agentBody.transform.position.z > agentBoundary.zMax || agentBody.transform.position.z < agentBoundary.zMin)
        //    {
        //        AddReward(avoidBoundariesReward);
        //        episodeReward["BoundaryReward"] += avoidBoundariesReward;
        //    }
        //}
        // Reward high puck velocities
        if (encouragePuckMovementReward > 0f)
        {
            AddReward(puck.Character.Velocity.magnitude * encouragePuckMovementReward);
            episodeReward["PuckVelocityReward"] += puck.Character.Velocity.magnitude * encouragePuckMovementReward;
        }
        // STEP REWARD
        AddReward(stepReward);
        episodeReward["StepReward"] += stepReward;
        #endregion
        
        #region TaskSpecificRewards
        // Task specific Rewards
        if (taskType == TaskType.ReachThePuck)
        {
            if (puck.AgentContact)
            {
                SetReward(1f);
                EndEpisode();
                return;
            }
            else if (StepCount == MaxStep)
            {
                SetReward(1f - (0.1f*Vector2.Distance(agentBody.transform.position, puck.transform.position)));
                return;
            }
        }
        else if(taskType == TaskType.FullGameUntilGoal)
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

        // Apply Force
        arriveSteeringBehavior = new ArriveSteeringBehavior(); // because direction-vector is normalized arrive steerign should be sufficient
        pusherControllerAgent.Character.Velocity = new Vector3(pusherControllerAgent.ActuatorX.Velocity, 0, pusherControllerAgent.ActuatorZ.Velocity);
        pusherControllerAgent.Character.Position = agentBody.transform.position;

        Vector3 acceleration = arriveSteeringBehavior.Arrive(direction, pusherControllerAgent.Character, TargetRadius, SlowDownRadius, MaxSpeed, MaxAcceleration, TimeToTarget);

        // update Character values
        pusherControllerAgent.Character.Position = agentBody.transform.position;
        pusherControllerAgent.Character.Velocity = new Vector3(pusherControllerAgent.ActuatorX.Velocity, 0, pusherControllerAgent.ActuatorZ.Velocity);
        pusherControllerAgent.Character.Accelaration = acceleration;

        // set actuator acceleration
        pusherControllerAgent.ActuatorX.Control = acceleration.x;
        pusherControllerAgent.ActuatorZ.Control = acceleration.z;
        #endregion
    }

    void FixedUpdate()
    {
        guidanceRods.position = new Vector3(0, 0, transform.position.z);
        //guidanceRods.velocity = new Vector3(0, 0, agentBody.velocity.z);
    }
}
