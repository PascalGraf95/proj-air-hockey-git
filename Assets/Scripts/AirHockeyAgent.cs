using UnityEngine;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public enum ActionType { Discrete, Continuous };
public enum TaskType
{
    Reaching,
    Scoring,
    Defending,
    FullGame
}

public class AirHockeyAgent : Agent
{
    public ActionType actionType;
    public float maxMovementSpeed;
    public bool randomAgentPosition;
    
    private Boundary agentBoundary;

    private Rigidbody2D agentRB;
    private PuckScript puck;
    private HumanPlayer humanPlayer;

    private Vector2 startingPosition;
    private Vector2 position;
    public TaskType taskType;
    public ResetPuckState resetPuckState;

    // Start is called before the first frame update
    void Start()
    {
        agentRB = GetComponent<Rigidbody2D>();

        var puckGameObject = GameObject.Find("Puck");
        puck = puckGameObject.GetComponent<PuckScript>();


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

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(agentRB.position);
        sensor.AddObservation(puck.PuckRB.position);
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
        var continouosActions = actionsIn.ContinuousActions;
        if(taskType == TaskType.Reaching)
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
            SetReward(-0.01f);
        }
        else if(taskType == TaskType.Defending)
        {
            if (puck.AgentScored)
            {
                SetReward(1f);
                EndEpisode();
                return;
            }
            else if (puck.HumanScored)
            {
                SetReward(-1f);
                EndEpisode();
                return;
            }
            else if (StepCount == MaxStep)
            {
                EndEpisode();
                return;
            }
            SetReward(-0.001f);
        }
        else if(taskType == TaskType.FullGame)
        {
            if (puck.AgentScored)
            {
                SetReward(1f);
                EndEpisode();
                return;
            }
            else if (puck.HumanScored)
            {
                SetReward(-1f);
                EndEpisode();
                return;
            }
        }



        float x = 0;
        float y = 0;

        x = continouosActions[0];
        y = continouosActions[1];
        

        // normalize so going diagonally doesn't speed things up
        Vector2 direction = new Vector2(x, y).normalized;

        // move Position
        position = new Vector2(Mathf.Clamp(agentRB.position.x, agentBoundary.Left,
                            agentBoundary.Right),
                            Mathf.Clamp(agentRB.position.y, agentBoundary.Down,
                            agentBoundary.Up));
        agentRB.MovePosition(position + direction * maxMovementSpeed * Time.fixedDeltaTime);
    }
}
