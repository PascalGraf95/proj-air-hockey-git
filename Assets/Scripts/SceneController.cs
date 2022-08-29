using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;

public class SceneController : MonoBehaviour
{
    private PuckController puckController;
    private PusherController pusherHumanController;
    private PusherController pusherAgentController;
    [SerializeField] private GoalColliderScript agentGoalColliderScript;
    [SerializeField] private GoalColliderScript humanGoalColliderScript;
    [SerializeField] private BackwallColliderScript backwallColliderScriptLeft;
    [SerializeField] private BackwallColliderScript backwallColliderScriptRight;
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

    // Start is called before the first frame update
    void Start()
    {
        pusherAgentController = GameObject.Find("PusherAgent").GetComponent<PusherController>();
        puckController = GameObject.Find("Puck").GetComponent<PuckController>();
        pusherHumanController = GameObject.Find("PusherHuman").GetComponent<PusherController>();


        // Subscribe to Goal Events
        agentGoalColliderScript.onGoalDetected += HumanPlayerScored;
        humanGoalColliderScript.onGoalDetected += AgentPlayerScored;
        backwallColliderScriptLeft.onBackwallHitDetected += BackwallReached;
        backwallColliderScriptRight.onBackwallHitDetected += BackwallReached;


        // Initialize UI Controller
        uiController = GetComponent<UIController>();
        if(uiController != null)
        {
            uiController.ResetUI();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScene();
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
        currentGameState = GameState.backWallReached;
    }

    public void ResetScene()
    {
        currentGameState = GameState.normal;
        puckController.transform.GetComponent<MeshRenderer>().enabled = false;

        // Reset Human and Agent Pusher
        pusherAgentController.Reset("Agent");
        pusherHumanController.Reset("Human");

        // Reset Puck
        puckController.Reset();

        // Reset Game Score
        if(humanPlayerScore >= 7 || agentPlayerScore >= 7)
        {
            uiController.ResetUI();
            humanPlayerScore = 0;
            agentPlayerScore = 0;
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
}
