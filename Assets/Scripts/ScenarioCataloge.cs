using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Mujoco;
using UnityEngine;

public enum PuckMoveOnStart
{
    move,
    rest
}

public enum State
{
    disabled,
    drivePusherToPosition,
    start,
    isRunnning,
    timeout
}

// enum with all possibility scenarios
public enum Scenario
{
    scenario_0,
    scenario_1,
    scenario_2,
    scenario_3,
    scenario_4,
    scenario_5,
    scenario_6
}

// contains all start parameters and configurations of a scenario
public struct Scenario_t
{
    public State currentState;
    public Boundary spawnPuck;
    public Boundary boundPusherAgent;
    public PuckMoveOnStart puckMoveState;
    public Vector2 targetPusherVelocity;
    public Vector2 targetPusherPosition;
    public Scenario currentScenario;

    public Scenario_t(State state,
                    float puckUp, float puckDown, float puckLeft, float puckRight,
                    float pusherUp, float pusherDown, float pusherLeft, float pusherRight,
                    PuckMoveOnStart moveState,
                    Vector2 targetVelocity,
                    Vector2 targetPosition,
                    Scenario scenario)
    {
        currentState = state;
        spawnPuck = new Boundary(puckUp, puckDown, puckLeft, puckRight);
        boundPusherAgent = new Boundary(pusherUp, pusherDown, pusherLeft, pusherRight);
        puckMoveState = moveState;
        targetPusherVelocity = targetVelocity;
        targetPusherPosition = targetPosition;
        currentScenario = scenario;
    }
}

public class ScenarioCataloge : MonoBehaviour
{
    #region privates
    public Scenario_t currentScenarioParams  = new Scenario_t(State.disabled, 
                                                            0f, 0f, 0f, 0f, 
                                                            0f, 0f, 0f, 0f, 
                                                            PuckMoveOnStart.rest,
                                                            new Vector2(0, 0),
                                                            new Vector2(0, 0),
                                                            Scenario.scenario_0);

    private SceneController sceneController;
    private PusherController pusherAgentController;
    private GameObject PusherAgentPosition;
    private PuckController puckController;
    private MjScene mjScene;

    private readonly int TimeoutTimeMS = 1000;   // scenario timeout in milliseconds
    private Timer t;
    #endregion

    void Start()
    {
        // find game objects
        sceneController = GameObject.Find("3DAirHockeyTable").GetComponent<SceneController>();
        pusherAgentController = GameObject.Find("PusherAgent").GetComponent<PusherController>();
        PusherAgentPosition = GameObject.Find("PusherAgent");
        puckController = GameObject.Find("Puck").GetComponent<PuckController>();
    }

    private void Update()
    {
        switch(currentScenarioParams.currentState)
        {
            case State.disabled:
                // do nothing
                break;
            case State.drivePusherToPosition:
                Vector3 position = PusherAgentPosition.transform.localPosition;

                Int32 x = 5;
                Int32 z = -5;

                if (position.x < currentScenarioParams.boundPusherAgent.Right || 
                    position.z > currentScenarioParams.boundPusherAgent.Down)
                {
                    // drive pusher as long as the scenario pusher zone is not reached
                    if(position.x > currentScenarioParams.boundPusherAgent.Right)
                    {
                        x = 0;
                    }
                    if(position.z < currentScenarioParams.boundPusherAgent.Down)
                    {
                        z = 0;
                    }
                    pusherAgentController.Act(new Vector2(x, z));
                }
                else
                {
                    // go into next step
                    currentScenarioParams.currentState = State.start;
                }
                break;
            case State.start:
                // start scenario timer to create timeout if agent failed task
                t = new Timer();
                t.Interval = TimeoutTimeMS;
                t.Elapsed += OnTimedEvent;
                t.Start();

                // set reset puck state correspond to the moving state
                //puckController.transform.GetComponent<MeshRenderer>().enabled = false;
                if (currentScenarioParams.puckMoveState == PuckMoveOnStart.move)
                {
                    puckController.resetPuckState = ResetPuckState.scenarioCatalogeMove;
                }
                else
                {
                    puckController.resetPuckState = ResetPuckState.scenarioCataloge;
                }
                puckController.Reset();

                // Mujoco Scene Reset
                if (mjScene == null)
                {
                    mjScene = GameObject.Find("MjScene").GetComponent<MjScene>();
                }
                mjScene.DestroyScene();
                mjScene.CreateScene();
                //puckController.transform.GetComponent<MeshRenderer>().enabled = true;

                // TODO: disable selfplay and start agentClone

                // go into running state
                currentScenarioParams.currentState = State.isRunnning;
                break;
            case State.isRunnning:
                // TODO: if scenario is succeed go also into timeout or end state                
                break;
            case State.timeout:
                // TODO: start next scenario
                Debug.Log("State: Timeout");
                t.Stop();
                currentScenarioParams.currentState = State.disabled;
                break;
        }
    }

    private void OnTimedEvent(object sender,ElapsedEventArgs e)
    {
        currentScenarioParams.currentState = State.timeout;
    }

    public void startScenario(Scenario scen)
    {
        // setup the scenario correspong to the scenario case
        switch (scen)
        {
            case Scenario.scenario_0:
                currentScenarioParams = new Scenario_t(State.drivePusherToPosition,
                                                        -35f, 0f, 33f, -33f, // up down left right
                                                        -68f, -50f, 30f, 20f,   // up down left right
                                                        PuckMoveOnStart.move,
                                                        new Vector2(0,0),
                                                        new Vector2(0,0),
                                                        Scenario.scenario_0);
                break;
            case Scenario.scenario_1:
                /*currentScenarioParams = new Scenario_t(true,
                                                        -35f, 0f, 33f, -33f, // up down left right
                                                        70f, 0f, -30f, 15f,   // up down left right
                                                        PuckMoveOnStart.rest);*/
                break;
            /*case scenario.scenario_2:
                currentScenarioParams = new Scenario_t(true,
                                                        -35f, 0f, 33f, -33f, // up down left right
                                                        -55f, -25f, 15f, -15f,   // up down left right
                                                        PuckMoveOnStart.rest);
                break;
            case scenario.scenario_3:
                currentScenarioParams = new Scenario_t(true,
                                                        -35f, 0f, 33f, -33f, // up down left right
                                                        -55f, -25f, 15f, -15f,   // up down left right
                                                        PuckMoveOnStart.rest);
                break;
            case scenario.scenario_4:
                currentScenarioParams = new Scenario_t(true,
                                                        -35f, 0f, 33f, -33f, // up down left right
                                                        -55f, -25f, 15f, -15f,   // up down left right
                                                        PuckMoveOnStart.rest);
                break;
            case scenario.scenario_5:
                currentScenarioParams = new Scenario_t(true,
                                                        -35f, 0f, 33f, -33f, // up down left right
                                                        -55f, -25f, 15f, -15f,   // up down left right
                                                        PuckMoveOnStart.rest);
                break;
            case scenario.scenario_6:
                currentScenarioParams = new Scenario_t(true,
                                                        -35f, 0f, 33f, -33f, // up down left right
                                                        -55f, -25f, 15f, -15f,   // up down left right
                                                        PuckMoveOnStart.rest);
                break;*/
            default:
                break;
        }

        // set reset puck state correspond to the moving state
        /*if (currentScenarioParams.puckMoveState == PuckMoveOnStart.move)
        {
            gameObject.GetComponent<AirHockeyAgent>().resetPuckState = ResetPuckState.scenarioCatalogeMove;
        }
        else
        {
            gameObject.GetComponent<AirHockeyAgent>().resetPuckState = ResetPuckState.scenarioCataloge;
        }*/

        // set pusherHumanSelfplay  TODO add later (it is always aas selfplay game)
        //gameObject.transform.Find("PusherHuman").GetComponent<MeshRenderer>().enabled = false;
        //gameObject.transform.Find("PusherHumanSelfplay").GetComponent<MeshRenderer>().enabled = true;

        // reset game and start scenario
        //sceneController.ResetSceneAgentPlaying();
        // TODO start scenario
    }
}
