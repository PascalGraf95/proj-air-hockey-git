using UnityEngine;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public enum ActionType { Discrete, Continuous };
public enum TaskType
{
    Reaching,
    FullGame
}

public enum ObservationType
{
    AgentPuck,
    AgentPuckVelocity,
    AgentPuckHuman,
    AgentPuckHumanVelocity
}

public class AirHockeyAgent : Agent
{
    public ActionType actionType;
    public ObservationType observationType;
    public float maxMovementSpeed;
    public bool randomAgentPosition;
    
    private Boundary agentBoundary;

    private Rigidbody2D agentRB;
    private Rigidbody2D puckRB;
    private PuckScript puck;
    private HumanPlayer humanPlayer;

    private Vector2 startingPosition;
    private Vector2 lastDirection;
    private Vector2 position;
    public TaskType taskType;
    public ResetPuckState resetPuckState;

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

    // Start is called before the first frame update
    void Start()
    {
        episodeReward = new Dictionary<string, float>();
        episodeReward["StepReward"] = 0f;
        episodeReward["DirectionReward"] = 0f;
        episodeReward["BoundaryReward"] = 0f;
        episodeReward["PuckVelocityReward"] = 0f;
        episodeReward["ScoreReward"] = 0f;


        agentRB = GetComponent<Rigidbody2D>();

        var puckGameObject = GameObject.Find("Puck");
        puck = puckGameObject.GetComponent<PuckScript>();
        puckRB = puckGameObject.GetComponent<Rigidbody2D>();


        var humanPlayerGameObject = GameObject.Find("HumanPlayer");
        humanPlayer = humanPlayerGameObject.GetComponent<HumanPlayer>();

        var agentBoundaryHolder = GameObject.Find("AgentBoundaryHolder").GetComponent<Transform>();
        agentBoundary = new Boundary(agentBoundaryHolder.GetChild(0).position.y,
                      agentBoundaryHolder.GetChild(1).position.y,
                      agentBoundaryHolder.GetChild(2).position.x,
                      agentBoundaryHolder.GetChild(3).position.x);

        startingPosition = agentRB.position;
    }

    public override void OnEpisodeBegin()
    {
        while (true)
        {
            puck.Reset(resetPuckState, agentBoundary);
            agentRB.velocity = Vector2.zero;

            if (randomAgentPosition)
            {
                agentRB.position = new Vector2(Random.Range(agentBoundary.Left, agentBoundary.Right) * 0.8f, Random.Range(agentBoundary.Down, agentBoundary.Up) * 0.8f);
            }
            else
            {
                agentRB.position = startingPosition;
            }
            // Player Position Reset
            humanPlayer.ResetPosition();

            if (Mathf.Abs(puck.PuckRB.position.y - agentRB.position.y) >= 1.0 || Mathf.Abs(puck.PuckRB.position.x - agentRB.position.x) >= 1.0)
            {
                break;
            }
        }
        /*
        print("------------------------------------------------------");
        print("StepReward:" + episodeReward["StepReward"].ToString());
        print("DirectionReward:" + episodeReward["DirectionReward"].ToString());
        print("BoundaryReward:" + episodeReward["BoundaryReward"].ToString());
        print("PuckVelocityReward:" + episodeReward["PuckVelocityReward"].ToString());
        print("ScoreReward:" + episodeReward["ScoreReward"].ToString());
        */

        episodeReward = new Dictionary<string, float>();
        episodeReward["StepReward"] = 0f;
        episodeReward["DirectionReward"] = 0f;
        episodeReward["BoundaryReward"] = 0f;
        episodeReward["PuckVelocityReward"] = 0f;
        episodeReward["ScoreReward"] = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(puck.transform.position);

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
        // X - Axis
        if (Input.GetKey(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            continuousActionsOut[0] = -1f;
        }
        else if (Input.GetKey(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Keypad6))
        {
            continuousActionsOut[0] = 1f;
        }
        else
        {
            continuousActionsOut[0] = 0f;
        }

        // Y - Axis
        if (Input.GetKey(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Keypad8))
        {
            continuousActionsOut[1] = 1f;
        }
        else if (Input.GetKey(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            continuousActionsOut[1] = -1f;
        }
        else
        {
            continuousActionsOut[1] = 0f;
        }
    }


    public override void OnActionReceived(ActionBuffers actionsIn) 
    {
        #region Action Calculations
        var continouosActions = actionsIn.ContinuousActions;

        // MOVEMENT CALCULATIONS
        float x = continouosActions[0];
        float y = continouosActions[1];

        // Action Dead Zone to avoid unneccessary movement
        if (Mathf.Abs(x) < 0.03f)
        {
            x = 0f;
        }
        if (Mathf.Abs(y) < 0.03f)
        {
            y = 0f;
        }

        // Normalize so going diagonally doesn't speed things up
        Vector2 direction = new Vector2(x, y);
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
            EndEpisode();
            return;
        }
        // HUMAN SCORED
        else if (puck.gameState == GameState.playerScored)
        {
            episodeReward["ScoreReward"] += humanScoredReward;
            SetReward(humanScoredReward);
            EndEpisode();
            return;
        }
        // PUCK REACHED OPPONENT'S BACKWALL
        else if (puck.gameState == GameState.backWallReached && backWallReward > 0)
        {
            episodeReward["ScoreReward"] += backWallReward;
            SetReward(backWallReward);
            EndEpisode();
            return;
        }
        // PUCK HAS BEEN DEFLECTED INTO THE OPPONENT'S FIELD
        else if (puck.transform.position.y < 0 && puck.AgentContact == true && deflectOnlyReward > 0f)
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
            if (agentRB.position.x < agentBoundary.Left || agentRB.position.x > agentBoundary.Right || agentRB.position.y > agentBoundary.Up || agentRB.position.y < agentBoundary.Down)
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
        if (taskType == TaskType.Reaching)
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
                EndEpisode();
                return;
            }
        }
        else if(taskType == TaskType.FullGame)
        {
            if (StepCount == MaxStep)
            {
                SetReward(maxStepReward);
                episodeReward["StepReward"] += stepReward;
                EndEpisode();
                return;
            }

        }
        #endregion
        #endregion

        #region Movement and Clipping
        position = new Vector2(Mathf.Clamp(agentRB.position.x, agentBoundary.Left,
                            agentBoundary.Right),
                            Mathf.Clamp(agentRB.position.y, agentBoundary.Down,
                            agentBoundary.Up));
        agentRB.MovePosition(position + direction * maxMovementSpeed * Time.fixedDeltaTime);
        #endregion
    }
}
