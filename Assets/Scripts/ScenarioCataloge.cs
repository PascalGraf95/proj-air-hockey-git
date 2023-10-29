using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mujoco;
using UnityEngine;

public enum puckMoveOnStart
{
    move,
    rest
}

// contains all start parameters and configurations of a scenario
public struct Scenario_t
{
    public bool isEnabled;
    public Boundary spawnPuck;
    public Boundary boundPusherAgent;
    public puckMoveOnStart puckMoveState;

    public Scenario_t(bool enableFlag,
                    float puckUp, float puckDown, float puckLeft, float puckRight,
                    float pusherUp, float pusherDown, float pusherLeft, float pusherRight,
                    puckMoveOnStart moveState)
    {
        isEnabled = enableFlag;
        spawnPuck = new Boundary(puckUp, puckDown, puckLeft, puckRight);
        boundPusherAgent = new Boundary(pusherUp, pusherDown, pusherLeft, pusherRight);
        puckMoveState = moveState;
    }
}

public class ScenarioCataloge : MonoBehaviour
{
    // enum with all possibility scenarios
    public enum scenario
    {
        scenario_0,
        scenario_1,
        scenario_2,
        scenario_3,
        scenario_4,
        scenario_5,
        scenario_6
    }

    public Scenario_t currentScenarioParams  = new Scenario_t(false, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, puckMoveOnStart.rest);

    private SceneController sceneController;

    void Start()
    {
        setupSzenarioCataloge();
    }

    private void setupSzenarioCataloge()
    {
        // find game objects
        sceneController = GameObject.Find("3DAirHockeyTable").GetComponent<SceneController>();
    }

    public void startScenario(scenario scen)
    {
        // setup the scenario correspong to the scenario case
        switch (scen)
        {
            case scenario.scenario_0:
                /*currentScenarioParams = new Scenario_t(true,
                                                        -35f, 0f, 33f, -33f, // up down left right
                                                        70f, 30f, -30f, -15f,   // up down left right
                                                        puckMoveOnStart.rest);*/
                break;
            case scenario.scenario_1:
                /*currentScenarioParams = new Scenario_t(true,
                                                        -35f, 0f, 33f, -33f, // up down left right
                                                        70f, 0f, -30f, 15f,   // up down left right
                                                        puckMoveOnStart.rest);*/
                break;
            case scenario.scenario_2:
                currentScenarioParams = new Scenario_t(true,
                                                        -35f, 0f, 33f, -33f, // up down left right
                                                        -55f, -25f, 15f, -15f,   // up down left right
                                                        puckMoveOnStart.rest);
                break;
            case scenario.scenario_3:
                currentScenarioParams = new Scenario_t(true,
                                                        -35f, 0f, 33f, -33f, // up down left right
                                                        -55f, -25f, 15f, -15f,   // up down left right
                                                        puckMoveOnStart.rest);
                break;
            case scenario.scenario_4:
                currentScenarioParams = new Scenario_t(true,
                                                        -35f, 0f, 33f, -33f, // up down left right
                                                        -55f, -25f, 15f, -15f,   // up down left right
                                                        puckMoveOnStart.rest);
                break;
            case scenario.scenario_5:
                currentScenarioParams = new Scenario_t(true,
                                                        -35f, 0f, 33f, -33f, // up down left right
                                                        -55f, -25f, 15f, -15f,   // up down left right
                                                        puckMoveOnStart.rest);
                break;
            case scenario.scenario_6:
                currentScenarioParams = new Scenario_t(true,
                                                        -35f, 0f, 33f, -33f, // up down left right
                                                        -55f, -25f, 15f, -15f,   // up down left right
                                                        puckMoveOnStart.rest);
                break;
            default:
                break;
        }

        // set reset puck state correspond to the moving state
        /*if (currentScenarioParams.puckMoveState == puckMoveOnStart.move)
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
        sceneController.ResetSceneAgentPlaying();
        // TODO start scenario
    }
}
