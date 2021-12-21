using UnityEngine;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using UnityEngine.UI;

public enum ActionType { Discrete, Continuous };
public enum TaskType
{
    ReachThePuck,
    FullGameUntilGoal,
    FullGameMultipleGoals
}

public enum ObservationType
{
    AgentPuck,
    AgentPuckVelocity,
    AgentPuckHuman,
    AgentPuckHumanVelocity
}

public enum HumanBehavior
{
    None,
    StartingPosition,
    RandomPosition,
    OscillatingMovement,
    Heuristic,
    Selfplay,
    ManualMovement
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
    [Header("Agent Pusher Behavior")]
    public float maxAgentPusherVelocity;
    public float maxAgentPusherAcceleration;
    public bool randomAgentPosition;

    [Space(5)]
    [Header("Training Scenario")]
    public TaskType taskType;
    public ResetPuckState resetPuckState;
    public GameObject puckMarkerPrefab;
    public int maxStepsPerGame;

    [Space(5)]
    [Header("Human Pusher Behavior")]
    public float maxHumanPusherVelocity;
    public float maxHumanPusherAcceleration;
    public HumanBehavior humanBehavior;



    [Space(5)]
    [Header("Reward Composition")]
    [Range(0f, 10f)]
    public float agentScoredReward;
    [Range(-10f, 0f)]
    public float humanScoredReward;
    [Range(-1f, 0f)]
    public float avoidBoundariesReward;
    [Range(-1f, 0f)]
    public float avoidDirectionChangesReward;
    [Range(0f, 1f)]
    public float encouragePuckMovementReward;
    [Range(-1f, 0f)]
    public float puckStopReward;
    [Range(0f, 5f)]
    public float backWallReward;
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

    private Rigidbody agentRB;
    private Rigidbody puckRB;
    private PuckScript puck;
    private HumanPlayer humanPlayer;
    private Rigidbody guidanceRods;

    private Vector3 startingPosition;
    private Vector3 lastDirection;
    private Vector3 position;

    private int agentScore;
    private int humanScore;
    private Text scoreText;

    private int gamesSinceNewEpisode;
    private int lastGameReset;

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
        agentRB = GetComponent<Rigidbody>();
        startingPosition = agentRB.position;

        // Get Guidance Rods and UI
        guidanceRods = GameObject.Find("GuidanceRods").GetComponent<Rigidbody>();
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();

        // Get Boundaries
        agentBoundary = GameObject.Find("AgentBoundaries").GetComponent<FieldBoundary>();

        var puckGameObject = GameObject.Find("Puck");
        puckRB = puckGameObject.GetComponent<Rigidbody>();
        puck = puckGameObject.AddComponent<PuckScript>();
        puck.Init(resetPuckState, agentBoundary, puckMarkerPrefab);

        var humanPlayerGameObject = GameObject.Find("PusherHuman");
        humanPlayer = humanPlayerGameObject.AddComponent<HumanPlayer>();
        humanPlayer.Init(humanBehavior, maxHumanPusherVelocity, maxHumanPusherAcceleration);


    }

    public override void OnEpisodeBegin()
    {
        agentRB.velocity = Vector2.zero;

        if (randomAgentPosition)
        {
            agentRB.position = new Vector3(Random.Range(agentBoundary.xMin, agentBoundary.xMax), 0f, Random.Range(agentBoundary.zMin, agentBoundary.zMax));
        }
        else
        {
            agentRB.position = startingPosition;
        }
        // Player Position Reset
        humanPlayer.ResetPosition();
        puck.Reset();

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
        agentRB.velocity = Vector2.zero;

        if (randomAgentPosition)
        {
            agentRB.position = new Vector3(Random.Range(agentBoundary.xMin, agentBoundary.xMax), 0f, Random.Range(agentBoundary.zMin, agentBoundary.zMax));
        }
        else
        {
            agentRB.position = startingPosition;
        }
        // Player Position Reset
        humanPlayer.ResetPosition();
        puck.Reset();
        gamesSinceNewEpisode++;
        lastGameReset = StepCount;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(puckRB.position);

        if (observationType == ObservationType.AgentPuckHuman || observationType == ObservationType.AgentPuckHumanVelocity)
        {
            sensor.AddObservation(humanPlayer.transform.position);
        }
        if(observationType == ObservationType.AgentPuckVelocity || observationType == ObservationType.AgentPuckHumanVelocity)
        {
            sensor.AddObservation(puckRB.velocity);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        continuousActionsOut[0] = horizontalInput;
        continuousActionsOut[1] = verticalInput;
    }
    

    public override void OnActionReceived(ActionBuffers actionsIn) 
    {
        #region Action Calculations
        var continouosActions = actionsIn.ContinuousActions;

        // MOVEMENT CALCULATIONS
        float x = continouosActions[0];
        float z = continouosActions[1];

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
            SetReward(backWallReward);

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
        // Punish running into boundaries.
        if (avoidBoundariesReward < 0f)
        {
            if (agentRB.position.x < agentBoundary.xMin || agentRB.position.x > agentBoundary.xMax || agentRB.position.z > agentBoundary.zMax || agentRB.position.z < agentBoundary.zMin)
            {
                AddReward(avoidBoundariesReward);
                episodeReward["BoundaryReward"] += avoidBoundariesReward;
            }
        }
        // Reward high puck velocities
        if (encouragePuckMovementReward > 0f)
        {
            AddReward(puckRB.velocity.magnitude * encouragePuckMovementReward);
            episodeReward["PuckVelocityReward"] += puckRB.velocity.magnitude * encouragePuckMovementReward;
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
                SetReward(1f - Vector2.Distance(agentRB.position, puck.PuckRB.position));
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
        agentRB.AddForce(direction * maxAgentPusherAcceleration * agentRB.mass * Time.deltaTime);
        
        // Limit Velocity
        if (agentRB.velocity.magnitude > maxAgentPusherVelocity)
        {
            agentRB.velocity = agentRB.velocity.normalized * maxAgentPusherVelocity;
        }
        // Limit Position
        if (agentRB.position.x < agentBoundary.xMin)
        {
            agentRB.velocity = new Vector3(0, 0, agentRB.velocity.z);
            agentRB.position = new Vector3(agentBoundary.xMin, 0, agentRB.position.z);
        }
        else if (agentRB.position.x > agentBoundary.xMax)
        {
            agentRB.velocity = new Vector3(0, 0, agentRB.velocity.z);
            agentRB.position = new Vector3(agentBoundary.xMax, 0, agentRB.position.z);
        }
        if (agentRB.position.z < agentBoundary.zMin)
        {
            agentRB.velocity = new Vector3(agentRB.velocity.x, 0, 0);
            agentRB.position = new Vector3(agentRB.position.x, 0, agentBoundary.zMin);
        }
        else if (agentRB.position.z > agentBoundary.zMax)
        {
            agentRB.velocity = new Vector3(agentRB.velocity.x, 0, 0);
            agentRB.position = new Vector3(agentRB.position.x, 0, agentBoundary.zMax);
        }
        #endregion
    }

    void FixedUpdate()
    {
        guidanceRods.position = new Vector3(0, 0, transform.position.z);
        guidanceRods.velocity = new Vector3(0, 0, agentRB.velocity.z);
    }
}
