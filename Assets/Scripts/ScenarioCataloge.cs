using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Mujoco;
using UnityEngine;
using UnityEngine.UIElements;
using static Mujoco.MujocoLib;

public enum PuckMoveOnStart
{
    moveSlow,
    moveFast,
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
    scenario_00,
    scenario_01,
    scenario_02,
    scenario_03,
    scenario_04,
    scenario_05,
    scenario_06,
    scenario_07,
    scenario_08,
    scenario_09,
    scenario_10,
    scenario_11,
    scenario_D1
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
                                                            Scenario.scenario_00);

    private SceneController sceneController;
    private PusherController pusherOpponentController;
    private GameObject pusherOpponentPosition;
    private PuckController puckController;
    private MjScene mjScene;
    private string[] csvMsgScen = new string[Enum.GetValues(typeof(Scenario)).Length];

    private readonly int TimeoutTimeMS = 2000;   // scenario timeout in milliseconds
    private string path = "csvFiles/";
    private string filePath = "";
    
    private uint numberOfRounds = 1;
    private Timer t;
    private uint scenarioCnt = 0;
    private uint roundsCnt = 0;
    
    #endregion

    void Start()
    {
        // find game objects
        sceneController = GameObject.Find("3DAirHockeyTable").GetComponent<SceneController>();
        pusherOpponentPosition = GameObject.Find("PusherHumanSelfplay");
        puckController = GameObject.Find("Puck").GetComponent<PuckController>();

        if (GameObject.Find("PusherHuman") != null)
        {
            pusherOpponentController = GameObject.Find("PusherHuman").GetComponent<PusherController>();
        }
        else if (GameObject.Find("PusherHumanSelfplay") != null)
        {
            pusherOpponentController = GameObject.Find("PusherHumanSelfplay").GetComponent<PusherController>();
        }
        else
        {
            Debug.LogError("Pusher Human GameObject not found.");
        }

        // initial csv scenario message
        resetCSVmsgState();
    }

    private void Update()
    {
        switch(currentScenarioParams.currentState)
        {
            case State.disabled:
                // do nothing
                break;
            case State.drivePusherToPosition:
                Vector3 position = pusherOpponentPosition.transform.localPosition;

                // drive to position velocity
                Int32 x = 5;
                Int32 z = 5;

                if (position.x < currentScenarioParams.boundPusherAgent.right || 
                    position.z < currentScenarioParams.boundPusherAgent.up)
                {
                    //Debug.Log("drive to in IF");
                    // drive pusher as long as the scenario pusher zone is not reached
                    if(position.x > currentScenarioParams.boundPusherAgent.right)
                    {
                        x = 0;
                    }
                    if(position.z > currentScenarioParams.boundPusherAgent.up)
                    {
                        z = 0;
                    }
                    pusherOpponentController.Act(new Vector2(x, z));
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
                if (currentScenarioParams.puckMoveState == PuckMoveOnStart.moveSlow)
                {
                    puckController.resetPuckState = ResetPuckState.scenarioCatalogeMoveSlow;
                }
                else if (currentScenarioParams.puckMoveState == PuckMoveOnStart.moveFast)
                {
                    puckController.resetPuckState = ResetPuckState.scenarioCatalogeMoveFast;
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
                // keep running as long as a goal is detected or the timeout event is triggered                
                break;
            case State.timeout:
                Debug.Log("timeout");   // TODO: delte line
                t.Stop();
                currentScenarioParams.currentState = State.disabled;
                scenarioCnt++;
                selectScenario((Scenario)scenarioCnt);
                break;
        }
    }

    private void resetCSVmsgState()
    {
        // initial csv scenario message
        for (int i = 0; i < Enum.GetValues(typeof(Scenario)).Length; i++)
        {
            csvMsgScen[i] = "notPlayed";
        }
    }

    private void OnTimedEvent(object sender,ElapsedEventArgs e)
    {
        csvMsgScen[scenarioCnt] = "timeout";
        currentScenarioParams.currentState = State.timeout;
    }

    private string toCSV()
    {
        /*
         * create a csv string to write it into the csv file
         */
        string str = "";
        for(int i = 0; i < Enum.GetValues(typeof(Scenario)).Length; i++)
        {
            if(i != 0)
            {
                str += ";";
            }
            str += csvMsgScen[i];
        }

        return str;
    }

    public int startScenario(uint rounds)
    {
        // start scenario, if it is not already running
        if (currentScenarioParams.currentState == State.disabled)
        {
            numberOfRounds = rounds;    // set scenario raounds

            selectScenario(Scenario.scenario_00); // start with first scenario

            return 1;
        }
        // retrun 0, if scenario is already running
        else
        {
            return 0;
        }
    }

    public void goalDetectedAgent()
    {
        csvMsgScen[scenarioCnt] = "Goal";   // goal for the evaluated agent
        currentScenarioParams.currentState = State.timeout;
    }

    public void goalDetectedHuman()
    {
        csvMsgScen[scenarioCnt] = "failed"; // gaol for the non evaluated oppenet agent
        currentScenarioParams.currentState = State.timeout;
    }

    public void selectScenario(Scenario scen)
    {
        // setup the scenario correspong to the scenario case
        switch (scen)
        {
            case Scenario.scenario_00:
                currentScenarioParams = new Scenario_t(State.drivePusherToPosition,
                                                        -35f, 0f, 33f, -33f,    // up down left right
                                                        50f, 68f, 30f, 20f,     // up down left right
                                                        PuckMoveOnStart.moveSlow,
                                                        new Vector2(0,0),
                                                        new Vector2(0,0),
                                                        Scenario.scenario_00);
                break;
            case Scenario.scenario_01:
                currentScenarioParams.currentState = State.drivePusherToPosition;
                currentScenarioParams.puckMoveState = PuckMoveOnStart.moveFast;
                currentScenarioParams.currentScenario = Scenario.scenario_01;
                break;
            case Scenario.scenario_02:
                currentScenarioParams = new Scenario_t(State.drivePusherToPosition,
                                                        -65f, -35f, 33f, -33f,  // puck: up down left right
                                                        50f, 68f, 30f, 20f,      // pusher: up down left right
                                                        PuckMoveOnStart.moveSlow,
                                                        new Vector2(0, 0),
                                                        new Vector2(0, 0),
                                                        Scenario.scenario_02);
                break;
            case Scenario.scenario_03:
                currentScenarioParams.currentState = State.drivePusherToPosition;
                currentScenarioParams.puckMoveState = PuckMoveOnStart.moveFast;
                currentScenarioParams.currentScenario = Scenario.scenario_03;
                break;
            case Scenario.scenario_04:
                currentScenarioParams = new Scenario_t(State.drivePusherToPosition,
                                                        -35f, 0f, 33f, -33f,    // puck: up down left right
                                                        53f, 20f, 8f, -8f,      // pusher: up down left right
                                                        PuckMoveOnStart.moveSlow,
                                                        new Vector2(0, 0),
                                                        new Vector2(0, 0),
                                                        Scenario.scenario_04);
                break;
            case Scenario.scenario_05:
                currentScenarioParams.currentState = State.drivePusherToPosition;
                currentScenarioParams.puckMoveState = PuckMoveOnStart.moveFast;
                currentScenarioParams.currentScenario = Scenario.scenario_05;
                break;
            case Scenario.scenario_06:
                currentScenarioParams = new Scenario_t(State.drivePusherToPosition,
                                                        -35f, 0f, 33f, -33f,    // puck: up down left right
                                                        53f, 20f, 15f, -15f,     // pusher: up down left right
                                                        PuckMoveOnStart.moveSlow,
                                                        new Vector2(0, 0),
                                                        new Vector2(0, 0),
                                                        Scenario.scenario_06);
                break;
            case Scenario.scenario_07:
                currentScenarioParams.currentState = State.drivePusherToPosition;
                currentScenarioParams.puckMoveState = PuckMoveOnStart.moveFast;
                currentScenarioParams.currentScenario = Scenario.scenario_07;
                break;
            case Scenario.scenario_08:
                currentScenarioParams = new Scenario_t(State.drivePusherToPosition,
                                                        -35f, 0f, 33f, -33f,    // puck: up down left right
                                                        5f, 20f, 15f, -15f,     // pusher: up down left right
                                                        PuckMoveOnStart.moveSlow,
                                                        new Vector2(0, 0),
                                                        new Vector2(0, 0),
                                                        Scenario.scenario_08);
                break;
            case Scenario.scenario_09:
                currentScenarioParams.currentState = State.drivePusherToPosition;
                currentScenarioParams.puckMoveState = PuckMoveOnStart.moveFast;
                currentScenarioParams.currentScenario = Scenario.scenario_09;
                break;
            case Scenario.scenario_10:
                currentScenarioParams = new Scenario_t(State.drivePusherToPosition,
                                                        -35f, 0f, 33f, -33f,    // puck: up down left right
                                                        5f, 20f, 33f, -33f,     // pusher: up down left right
                                                        PuckMoveOnStart.moveSlow,
                                                        new Vector2(0, 0),
                                                        new Vector2(0, 0),
                                                        Scenario.scenario_10);
                break;
            case Scenario.scenario_11:
                currentScenarioParams.currentState = State.drivePusherToPosition;
                currentScenarioParams.puckMoveState = PuckMoveOnStart.moveFast;
                currentScenarioParams.currentScenario = Scenario.scenario_11;
                break;
            default:
                scenarioCnt = 0;    // reset scenario counter

                // write CSV file
                if(roundsCnt == 0)
                {
                    // get the current date and time
                    DateTime currentDateTime = DateTime.Now;
                    filePath = path + currentDateTime.ToString("yyMMddHHmmss") + "scenarioResult.csv";

                    // add file header
                    string header = "";
                    for(int i = 0; i < Enum.GetValues(typeof(Scenario)).Length; i++)
                    {
                        if(i != 0)
                        {
                            header += ";";
                        }
                        header += "Scenario_" + i.ToString();
                    }

                    // create and write to file
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(filePath))
                        {
                            writer.WriteLine(header);
                            writer.WriteLine(toCSV());
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                    resetCSVmsgState();

                    roundsCnt++;

                    selectScenario(Scenario.scenario_00);
                    break;
                }
                // start scenario again, if not all rounds are played
                else if (roundsCnt >= (numberOfRounds - 1)) 
                {
                    currentScenarioParams.currentState = State.disabled;
                    roundsCnt = 0;
                    sceneController.ResetScene(false);
                }
                // if scenario is going on in the next round
                else
                {
                    // start new round
                    selectScenario(Scenario.scenario_00);

                    roundsCnt++;
                }

                //write to existing file
                try
                {
                    using (StreamWriter writer = File.AppendText(filePath))
                    {
                        writer.WriteLine(toCSV());
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
                resetCSVmsgState();
                break;
        }
    }
}