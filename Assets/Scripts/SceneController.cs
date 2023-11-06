using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using System;
using UnityEngine.SceneManagement;
using Assets.Scripts;
using Unity.MLAgents.SideChannels;
using Unity.MLAgents;

public enum GameState
{
    normal,
    agentScored,
    playerScored,
    backWallReached
}

public class SceneController : MonoBehaviour
{
    #region Variables
    private PuckController puckController;
    private PuckControllerAPI puckControllerAPI;
    private ScenarioCataloge scenarioCataloge;
    private PusherController pusherHumanController;
    private PusherController pusherAgentController;
    private GameObject cursor;
    [SerializeField] private GoalColliderScript agentGoalColliderScript;
    [SerializeField] private GoalColliderScript humanGoalColliderScript;
    [SerializeField] private BackwallColliderScript backwallColliderScriptLeft;
    [SerializeField] private BackwallColliderScript backwallColliderScriptRight;
    [SerializeField] private Transform airhockeyTableBends;
    [SerializeField] private PusherConfiguration pusherConfiguration;

    public delegate void OnEpisodeEnded();
    public event OnEpisodeEnded onEpisodeEnded;

    private UIController uiController;
    public GameState CurrentGameState 
    { 
        get { return currentGameState; } 
        set { currentGameState = value; } 
    }

    private MjScene mjScene;
    private int humanPlayerScore = 0;
    private int agentPlayerScore = 0;
    [SerializeField] private int maxScore = 5;
    [SerializeField] private int maxEpisodesWithoutScore = 5;
    private GameState currentGameState;
    private bool humanPlaying = false;
    private int gamesPlayed = 0;
    private float lastBackwallHitDetected;
    private int episodesWithoutScore = 0;

    AdditionalGameInformationsSideChannel gameResultsSideChannel;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        SetupSceneController();
        
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

        puckControllerAPI = GetComponent<PuckControllerAPI>();
        scenarioCataloge = GetComponent<ScenarioCataloge>();
    }

    public void Awake()
    {
        gameResultsSideChannel = new AdditionalGameInformationsSideChannel();
        SideChannelManager.RegisterSideChannel(gameResultsSideChannel);
    }

    public void OnDestroy()
    {
        if (Academy.IsInitialized)
        {
            SideChannelManager.UnregisterSideChannel(gameResultsSideChannel);
        }
    }
    public void SetupSceneController()
    {
        cursor = GameObject.Find("HandCursor");
        pusherAgentController = GameObject.Find("PusherAgent").GetComponent<PusherController>();
        puckController = GameObject.Find("Puck").GetComponent<PuckController>();
        puckController.resetPuckState = gameObject.GetComponent<AirHockeyAgent>().resetPuckState;


        if (GameObject.Find("PusherHuman") != null)
        {
            pusherHumanController = GameObject.Find("PusherHuman").GetComponent<PusherController>();
        }
        else if (GameObject.Find("PusherHumanSelfplay") != null)
        {
            pusherHumanController = GameObject.Find("PusherHumanSelfplay").GetComponent<PusherController>();
            pusherHumanController.SetPusherConfiguration(pusherConfiguration);
        }
        else
        {
            Debug.LogError("Pusher Human GameObject not found.");
        }
        pusherAgentController.SetPusherConfiguration(pusherConfiguration);
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
        if (Input.GetKeyDown(KeyCode.X))
        {
            //puckControllerAPI.simulate_real_puck();

            // start scenario, if it is disabled
            int err = scenarioCataloge.startScenario(3);
            Debug.Log("Start Scenario State: " + err);
        }
    }

    public void AgentPlayerScored()
    {
        episodesWithoutScore = 0;
        agentPlayerScore++;
        currentGameState = GameState.agentScored;
        if (uiController != null)
        {
            uiController.AgentPlayerScored(agentPlayerScore);
        }
    }

    private void HumanPlayerScored()
    {
        if(scenarioCataloge.currentScenarioParams.currentState == State.isRunnning)
        {
            scenarioCataloge.goalDetected();
        }
        else
        {
            episodesWithoutScore = 0;
            humanPlayerScore++;
            currentGameState = GameState.playerScored;
            if (uiController != null)
            {
                uiController.HumanPlayerScored(humanPlayerScore);
            }
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
        onEpisodeEnded();
        currentGameState = GameState.normal;
        puckController.transform.GetComponent<MeshRenderer>().enabled = false;

        // Reset Human and Agent Pusher
        pusherAgentController.Reset("Agent", false);
        pusherHumanController.Reset("Human", false);

        // Reset Puck
        puckController.Reset();

        // Reset Game Score
        if(humanPlayerScore >= maxScore || agentPlayerScore >= maxScore || forceScoreReset || episodesWithoutScore >= maxEpisodesWithoutScore)
        {
            episodesWithoutScore = 0;
            if (!humanPlaying)
            {
                // Game results to determine Elo-rating should only be send to modular-reinforcement-learning
                // if selfplay is active and a full game has been played
                if (!forceScoreReset)
                {
                    gamesPlayed++;
                    gameResultsSideChannel.SendGameResultToModularRL(agentPlayerScore, humanPlayerScore, gamesPlayed);
                }

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
        episodesWithoutScore++;
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
        gameObject.transform.Find("Puck").GetComponent<PuckController>().resetPuckState = gameObject.GetComponent<AirHockeyAgent>().resetPuckState;

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
        gameObject.transform.Find("Puck").GetComponent<PuckController>().resetPuckState = gameObject.GetComponent<AirHockeyAgent>().resetPuckState;

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
