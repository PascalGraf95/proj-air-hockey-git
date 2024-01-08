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
    timeout,
    newRound
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
    scenario_end_round
}

// contains all start parameters and configurations of a scenario
public struct Scenario_t
{
    public State currentState;
    public Boundary spawnPuck;
    public Boundary boundPusherAgent;
    public PuckMoveOnStart puckMoveState;
    public Vector2 targetPusherVelocity;
    public Scenario currentScenario;

    public Scenario_t(State state,
                    float puckUp, float puckDown, float puckLeft, float puckRight,
                    float pusherUp, float pusherDown, float pusherLeft, float pusherRight,
                    PuckMoveOnStart moveState,
                    Vector2 targetVelocity,
                    Scenario scenario)
    {
        currentState = state;
        spawnPuck = new Boundary(puckUp, puckDown, puckLeft, puckRight);
        boundPusherAgent = new Boundary(pusherUp, pusherDown, pusherLeft, pusherRight);
        puckMoveState = moveState;
        targetPusherVelocity = targetVelocity;
        currentScenario = scenario;
    }
}

public class ScenarioCataloge : MonoBehaviour
{
    #region privates
    public bool scenDebug = false;

    public Scenario_t currentScenarioParams  = new Scenario_t(State.disabled, 
                                                            0f, 0f, 0f, 0f, 
                                                            0f, 0f, 0f, 0f, 
                                                            PuckMoveOnStart.rest,
                                                            new Vector2(0, 0),
                                                            Scenario.scenario_00);

    private SceneController sceneController;
    private PusherController pusherOpponentController;
    private GameObject pusherOpponentPosition;
    private PuckController puckController;
    private MjScene mjScene;
    private string[] csvMsgScen = new string[Enum.GetValues(typeof(Scenario)).Length];

    private readonly int TimeoutTimeMS = 3500;   // scenario timeout in milliseconds
    private string path = "csvFiles/";
    private string filePath = "";
    
    private uint numberOfRounds = 1;
    private Timer t;
    private uint scenarioCnt = 0;
    private uint roundsCnt = 0;
    private uint newCSVfile = 0;    // 0: just create a csv file in the first place and write all triggered scenarios in one file
                                    // 1: create a new file every time the scenario is triggered
    
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

        // init timer
        t = new Timer(TimeoutTimeMS);
        t.Elapsed += OnTimedEvent;
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
                float x = currentScenarioParams.targetPusherVelocity.x;
                float z = currentScenarioParams.targetPusherVelocity.y;

                //if (position.x < currentScenarioParams.boundPusherAgent.right || 
                //    position.z < currentScenarioParams.boundPusherAgent.up)
                if ((((position.x - currentScenarioParams.boundPusherAgent.right) * Math.Sign(x)) < 0) ||
                    (((position.z - currentScenarioParams.boundPusherAgent.up) * Math.Sign(z)) < 0))
                {
                    // drive pusher as long as the scenario pusher zone is not reached
                    //if(position.x > currentScenarioParams.boundPusherAgent.right)
                    if (((position.x - currentScenarioParams.boundPusherAgent.right) * Math.Sign(x)) > 0)
                    {
                        x = 0;
                    }
                    //if(position.z > currentScenarioParams.boundPusherAgent.up)
                    if (((position.z - currentScenarioParams.boundPusherAgent.up) * Math.Sign(z)) > 0)
                    {
                        z = 0;
                    }
                    Debug.Log("vel x: " + x + "; z:" + z);
                    Debug.Log("if: " + position.x + " - " + currentScenarioParams.boundPusherAgent.right + " * " + Math.Sign(x) + " < 0" + " || " + 
                                 position.z + " - " + currentScenarioParams.boundPusherAgent.up + " * " + Math.Sign(z) + " < 0");
                    Debug.Log("pos x: " + position.x + "; z: " + position.z);
                    pusherOpponentController.Act(new Vector2(x, z));
                    //if (scenDebug)
                    //{
                    //    currentScenarioParams.currentState = State.timeout;
                    //}
                }
                else
                {
                    // go into next step
                    currentScenarioParams.currentState = State.start;
                    Debug.Log("Reach scen range! REEEEEEEEEEEEEEEEEEEEEEEE");
                }
                break;
            case State.start:
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

                // go into running state
                currentScenarioParams.currentState = State.isRunnning;
                break;
            case State.isRunnning:
                // keep running as long as a goal is detected or the timeout event is triggered
                //if (scenDebug)
                //{
                //    currentScenarioParams.currentState = State.timeout;
                //}
                break;
            case State.timeout:
                //Debug.Log("timeout: t.stop()");
                //if(!scenDebug)
                //{
                //    break;
                //}
                //scenDebug = false;

                ResetAndRestartTimer();

                currentScenarioParams.currentState = State.isRunnning;
                scenarioCnt++;
                selectScenario((Scenario)scenarioCnt);
                break;
            case State.newRound:
                selectScenario(Scenario.scenario_00);
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

    private void ResetAndRestartTimer()
    {
        // Stop the timer
        t.Stop();

        // Reset the timer properties
        t.Interval = TimeoutTimeMS; // Set the interval to the desired value
        t.Enabled = false; // Disable the timer to ensure it doesn't start immediately
        t.Elapsed -= OnTimedEvent; // Remove the existing event handler (optional, if already assigned)

        // Add the new event handler
        t.Elapsed += OnTimedEvent;
    }

    private void OnTimedEvent(object sender,ElapsedEventArgs e)
    {
        try
        {
            csvMsgScen[scenarioCnt] = "timeout";
            currentScenarioParams.currentState = State.timeout;
        }
        catch (Exception ex)
        {
            Debug.Log("Fehler im Timer-Handler: " + ex.Message);
        }
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

    public int startScenario(ushort rounds, uint oneFileFlagCSV)
    {
        // use rounds as ushort to not overload round counter

        // start scenario, if it is not already running
        if (currentScenarioParams.currentState != State.disabled)
        {
            return 0;   // retrun 0, if scenario is already running
        }

        newCSVfile = oneFileFlagCSV;
        if(newCSVfile == 1) // reset round counter, if the new CSV-File flag is set
        {
            roundsCnt = 0;
        }
        else if(roundsCnt + rounds > 4294967295)    // handle overflow
        {
            roundsCnt = rounds;
        }

        numberOfRounds = rounds;    // set scenario raounds

        selectScenario(Scenario.scenario_00); // start with first scenario

        return 1;   // retrun 1, if a new scenario start
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
        Debug.Log("scenario: " + scen);

        pusherOpponentController.Reset("Human", false);

        // Mujoco Scene Reset
        if (mjScene == null)
        {
            mjScene = GameObject.Find("MjScene").GetComponent<MjScene>();
        }
        mjScene.DestroyScene();
        mjScene.CreateScene();

        // setup the scenario correspong to the scenario case
        switch (scen)
        {
            case Scenario.scenario_00:
                currentScenarioParams = new Scenario_t(State.drivePusherToPosition,
                                                        35f, 5f, 30f, -30f,    // up down left right
                                                        50f, 68f, 30f, 20f,     // up down left right
                                                        PuckMoveOnStart.moveSlow,
                                                        new Vector2(1f, 1f),
                                                        Scenario.scenario_00);
                t.Start();
                break;
            case Scenario.scenario_01:
                currentScenarioParams.currentState = State.drivePusherToPosition;
                currentScenarioParams.puckMoveState = PuckMoveOnStart.moveFast;
                currentScenarioParams.currentScenario = Scenario.scenario_01;
                t.Start();
                break;
            case Scenario.scenario_02:
                currentScenarioParams = new Scenario_t(State.drivePusherToPosition,
                                                        35f, 5f, 30f, -30f,  // puck: up down left right
                                                        50f, 68f, 30f, 20f,      // pusher: up down left right
                                                        PuckMoveOnStart.moveSlow,
                                                        new Vector2(1f, 1f),
                                                        Scenario.scenario_02);
                t.Start();
                break;
            case Scenario.scenario_03:
                currentScenarioParams.currentState = State.drivePusherToPosition;
                currentScenarioParams.puckMoveState = PuckMoveOnStart.moveFast;
                currentScenarioParams.currentScenario = Scenario.scenario_03;
                t.Start();
                break;
            case Scenario.scenario_04:
                currentScenarioParams = new Scenario_t(State.drivePusherToPosition,
                                                        35f, 5f, 30f, -30f,    // puck: up down left right
                                                        20f, 53f, 8f, -8f,      // pusher: up down left right
                                                        PuckMoveOnStart.moveSlow,
                                                        new Vector2(0.1f, 0.1f),
                                                        Scenario.scenario_04);
                t.Start();
                break;
            case Scenario.scenario_05:
                currentScenarioParams.currentState = State.drivePusherToPosition;
                currentScenarioParams.puckMoveState = PuckMoveOnStart.moveFast;
                currentScenarioParams.currentScenario = Scenario.scenario_05;
                t.Start();
                break;
            case Scenario.scenario_06:
                currentScenarioParams = new Scenario_t(State.drivePusherToPosition,
                                                        35f, 5f, 30f, -30f,    // puck: up down left right
                                                        20f, 53f, 15f, -15f,     // pusher: up down left right
                                                        PuckMoveOnStart.moveSlow,
                                                        new Vector2(0.1f, 0.1f),
                                                        Scenario.scenario_06);
                t.Start();
                break;
            case Scenario.scenario_07:
                currentScenarioParams.currentState = State.drivePusherToPosition;
                currentScenarioParams.puckMoveState = PuckMoveOnStart.moveFast;
                currentScenarioParams.currentScenario = Scenario.scenario_07;
                t.Start();
                break;
            case Scenario.scenario_08:
                currentScenarioParams = new Scenario_t(State.drivePusherToPosition,
                                                        35f, 5f, 30f, -30f,    // puck: up down left right
                                                        8f, 20f, 15f, -15f,     // pusher: up down left right
                                                        PuckMoveOnStart.moveSlow,
                                                        new Vector2(0.1f, -1f),
                                                        Scenario.scenario_08);
                t.Start();
                break;
            case Scenario.scenario_09:
                currentScenarioParams.currentState = State.drivePusherToPosition;
                currentScenarioParams.puckMoveState = PuckMoveOnStart.moveFast;
                currentScenarioParams.currentScenario = Scenario.scenario_09;
                t.Start();
                break;
            case Scenario.scenario_10:
                currentScenarioParams = new Scenario_t(State.drivePusherToPosition,
                                                        35f, 5f, 30f, -30f,    // puck: up down left right
                                                        8f, 20f, 33f, -33f,     // pusher: up down left right
                                                        PuckMoveOnStart.moveSlow,
                                                        new Vector2(0.1f, -1f),
                                                        Scenario.scenario_10);
                t.Start();
                break;
            case Scenario.scenario_11:
                currentScenarioParams.currentState = State.drivePusherToPosition;
                currentScenarioParams.puckMoveState = PuckMoveOnStart.moveFast;
                currentScenarioParams.currentScenario = Scenario.scenario_11;
                t.Start();
                break;
            default:
                scenarioCnt = 0;    // reset scenario counter
                roundsCnt++;

                // write CSV file
                if (roundsCnt == 1)
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

                    // spacial case: if number of rounds = 1
                    if(numberOfRounds == 1)
                    {
                        if (newCSVfile == 1)
                        {
                            roundsCnt = 0;
                        }
                        currentScenarioParams.currentState = State.disabled;
                        sceneController.ResetScene(false);
                        break;
                    }

                    //selectScenario(Scenario.scenario_00);
                    currentScenarioParams.currentState = State.newRound;
                    break;
                }
                // start scenario again, if not all rounds are played
                else if ((roundsCnt % numberOfRounds) == 0)
                {
                    if(newCSVfile == 1)
                    {
                        roundsCnt = 0;
                    }
                    currentScenarioParams.currentState = State.disabled;
                    sceneController.ResetScene(false);
                }
                // if scenario is going on in the next round
                else
                {
                    // start new round
                    //selectScenario(Scenario.scenario_00);
                    currentScenarioParams.currentState = State.newRound;
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