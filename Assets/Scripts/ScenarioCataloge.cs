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
public struct scenario_t
{
    public Boundary spawnPuck;
    public puckMoveOnStart puckMoveState;

    public scenario_t(float up,
                    float down,
                    float left,
                    float right,
                    puckMoveOnStart moveState)
    {
        spawnPuck = new Boundary(up, down, left, right);
        puckMoveState = moveState;
    }
}

public class ScenarioCataloge : MonoBehaviour
{
    // enum with all possibility scenarios
    public enum scenario
    {
        scenario_1
    }

    public scenario_t currentScenarioParams;

    // generate unity objects
    //private GameObject puck;

    //private PusherController pusherAgentController;
    //private PusherController pusherHumanController;

    private PuckController puckController;
    private SceneController sceneController;

    void Start()
    {
        setupSzenarioCataloge();
    }

    private void setupSzenarioCataloge()
    {
        // find game objects
        //puck = GameObject.Find("Puck");
        //pusherAgentController = GameObject.Find("PusherAgent").GetComponent<PusherController>();
        //pusherHumanController = GameObject.Find("PusherHumanSelfplay").GetComponent<PusherController>();
        //puckController = GameObject.Find("Puck").GetComponent<PuckController>();
        sceneController = GameObject.Find("3DAirHockeyTable").GetComponent<SceneController>();
    }

    public void startScenario(scenario scen)
    {
        // setup the scenario correspong to the scenario case
        switch (scen)
        {
            case scenario.scenario_1:
                currentScenarioParams = new scenario_t(-35f, 0f, 33f, -33f, puckMoveOnStart.rest);
                break;
            default:
                break;
        }

        // set reset puck state correspond to the moving state
        if (currentScenarioParams.puckMoveState == puckMoveOnStart.move)
        {
            gameObject.GetComponent<AirHockeyAgent>().resetPuckState = ResetPuckState.scenarioCatalogeMove;
        }
        else
        {
            gameObject.GetComponent<AirHockeyAgent>().resetPuckState = ResetPuckState.scenarioCataloge;
        }

        // set pusherHumanSelfplay  TODO add later (it is always aas selfplay game)
        //gameObject.transform.Find("PusherHuman").GetComponent<MeshRenderer>().enabled = false;
        //gameObject.transform.Find("PusherHumanSelfplay").GetComponent<MeshRenderer>().enabled = true;

        // reset game and start scenario
        //puckController.resetPuckState = gameObject.GetComponent<AirHockeyAgent>().resetPuckState;
        //Debug.Log(puckController.resetPuckState);
        sceneController.ResetSceneHumanPlaying();   // TODO must be Agent selfplay after select agentselfplay above
        // TODO start scenario
    }
}
