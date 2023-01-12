using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using System;
using UnityEngine.SceneManagement;
using Unity.MLAgents;

public class SceneController : MonoBehaviour
{
    private PuckController puckController;
    private PusherController pusherHumanController;
    private PusherController pusherAgentController;
    private GameObject cursor;
    [SerializeField] private GoalColliderScript agentGoalColliderScript;
    [SerializeField] private GoalColliderScript humanGoalColliderScript;
    [SerializeField] private BackwallColliderScript backwallColliderScriptLeft;
    [SerializeField] private BackwallColliderScript backwallColliderScriptRight;
    [SerializeField] private Transform airhockeyTableBends;

    private UIController uiController;
    public GameState CurrentGameState 
    { 
        get { return currentGameState; } 
        set { currentGameState = value; } 
    }

    private MjScene mjScene;
    private int humanPlayerScore = 0;
    private int agentPlayerScore = 0;
    private GameState currentGameState;
    private bool humanPlaying = false;
    private float lastBackwallHitDetected;

    // Start is called before the first frame update
    void Start()
    {
        SetupSceneController();
        cursor = GameObject.Find("HandCursor");
        pusherAgentController = GameObject.Find("PusherAgent").GetComponent<PusherController>();
        puckController = GameObject.Find("Puck").GetComponent<PuckController>();
        pusherHumanController = GameObject.Find("PusherHumanSelfplay").GetComponent<PusherController>();
        
        // Subscribe to Goal Events
        agentGoalColliderScript.onGoalDetected += HumanPlayerScored;
        humanGoalColliderScript.onGoalDetected += AgentPlayerScored;
        backwallColliderScriptLeft.onBackwallHitDetected += BackwallReached;
        backwallColliderScriptRight.onBackwallHitDetected += BackwallReached;
        foreach(Transform bend in airhockeyTableBends)
        {
            foreach(Transform block in bend)
            {
                BackwallColliderScript script = block.GetComponent<BackwallColliderScript>();
                if(script != null)
                {
                    script.onBackwallHitDetected += BackwallReached;
                }
            }
        }

        // Initialize UI Controller
        uiController = GetComponent<UIController>();
        if (uiController != null)
        {
            uiController.ResetUI();
        }
    }

    public void SetupSceneController()
    {
        pusherAgentController = GameObject.Find("PusherAgent").GetComponent<PusherController>();
        puckController = GameObject.Find("Puck").GetComponent<PuckController>();

        if (GameObject.Find("PusherHuman") != null)
        {
            pusherHumanController = GameObject.Find("PusherHuman").GetComponent<PusherController>();
        }
        else if (GameObject.Find("PusherHumanSelfplay") != null)
        {
            pusherHumanController = GameObject.Find("PusherHumanSelfplay").GetComponent<PusherController>();
        }
        else
        {
            Debug.LogError("Pusher Human GameObject not found.");
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScene(false);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ResetSceneAgentPlaying();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ResetSceneHumanPlaying();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            uiController.ToggleUiIsVisible();
        }
    }

    public void AgentPlayerScored()
    {
        agentPlayerScore++;
        currentGameState = GameState.agentScored;
        if (uiController != null)
        {
            uiController.AgentPlayerScored(agentPlayerScore);
        }
    }

    private void HumanPlayerScored()
    {
        humanPlayerScore++;
        currentGameState = GameState.playerScored;
        if (uiController != null)
        {
            uiController.HumanPlayerScored(humanPlayerScore);
        }
    }

    public bool puckIsInGame()
    {
        var puckPos = puckController.GetCurrentPosition();
        if (-50f < puckPos.x  && 50f > puckPos.x && -100f < puckPos.y && 100f > puckPos.y)
        {
            return true;
        }
        return false;
    }

    private void BackwallReached()
    {
        if(Time.time - lastBackwallHitDetected > 1f)
        {
            currentGameState = GameState.backWallReached;
            lastBackwallHitDetected = Time.time;
        }

    }

    public void ResetScene(bool forceScoreReset)
    {
        currentGameState = GameState.normal;
        puckController.transform.GetComponent<MeshRenderer>().enabled = false;

        // Reset Human and Agent Pusher
        pusherAgentController.Reset("Agent", false);
        pusherHumanController.Reset("Human", false);

        // Reset Puck
        puckController.Reset();

        // Reset Game Score
        if(humanPlayerScore >= 7 || agentPlayerScore >= 7 || forceScoreReset)
        {
            if(!humanPlaying)
            {
                uiController.ResetUI();
                humanPlayerScore = 0;
                agentPlayerScore = 0;
            }
            else
            {
                ResetSceneAgentPlaying();
            }

        }

        // Mujoco Scene Reset
        if (mjScene == null)
        {
            mjScene = GameObject.Find("MjScene").GetComponent<MjScene>();
        }
        mjScene.DestroyScene();
        mjScene.CreateScene();

        puckController.transform.GetComponent<MeshRenderer>().enabled = true;
    }

    /// <summary>
    /// Resets the scene in a way so that a human player can play against the artificial intelligence for one game to 10.
    /// This includes setting the PusherHuman active and disabling the mesh renderer of the ai controlled human pusher.
    /// Furthermore, all game objects need new references to the human pusher.
    /// </summary>
    public void ResetSceneHumanPlaying()
    {
        // Set human playing bool to true
        humanPlaying = false;

        if (GameObject.Find("PusherHuman") == null)
        {
            pusherHumanController.Reset("Human", true);
        }
        // Deactivate the Agent Controlled Pusher
        gameObject.transform.Find("PusherHumanSelfplay").GetComponent<MeshRenderer>().enabled = false;

        // Then activate the Human Controlled Pusher
        gameObject.transform.Find("PusherHuman").gameObject.SetActive(true);

        // Furthermore modify the puck reset scenario
        gameObject.transform.Find("Puck").GetComponent<PuckController>().resetPuckState = ResetPuckState.randomVelocity;

        // Trigger der Start Function again for all important GameObjects
        gameObject.transform.Find("Puck").GetComponent<PuckController>().SetupPuckController();
        transform.GetComponent<AirHockeyAgent>().SetupAirHockeyAgent();
        SetupSceneController();

        // Reset whole game score
        ResetScene(true);

        // Set human playing bool to true
        humanPlaying = true;
    }

    public void ResetSceneAgentPlaying()
    {
        // Deactivate the Agent Controlled Pusher
        gameObject.transform.Find("PusherHumanSelfplay").GetComponent<MeshRenderer>().enabled = true;

        // Then activate the Human Controlled Pusher
        gameObject.transform.Find("PusherHuman").gameObject.SetActive(false);

        // Furthermore modify the puck reset scenario
        gameObject.transform.Find("Puck").GetComponent<PuckController>().resetPuckState = ResetPuckState.randomVelocity;

        // Trigger der Start Function again for all important GameObjects
        gameObject.transform.Find("Puck").GetComponent<PuckController>().SetupPuckController();
        transform.GetComponent<AirHockeyAgent>().SetupAirHockeyAgent();
        SetupSceneController();

        // Set human playing bool to false
        humanPlaying = false;

        // Reset whole game score
        ResetScene(true);
    }
}
