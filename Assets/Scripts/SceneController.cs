using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using TMPro;

public class SceneController : MonoBehaviour
{
    [SerializeField] private PuckController puckController;
    [SerializeField] private PusherController pusherController;
    [SerializeField] private GoalColliderScript agentGoalColliderScript;
    [SerializeField] private GoalColliderScript humanGoalColliderScript;
    private UIController uiController;

    private MjScene mjScene;
    private int humanPlayerScore = 0;
    private int agentPlayerScore = 0;

    // Start is called before the first frame update
    void Start()
    {
        agentGoalColliderScript.onGoalDetected += HumanPlayerScored;
        humanGoalColliderScript.onGoalDetected += AgentPlayerScored;

        uiController = GetComponent<UIController>();
        uiController.ResetUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScene();
        }
    }

    private void AgentPlayerScored()
    {
        agentPlayerScore++;
        uiController.AgentPlayerScored(agentPlayerScore);
        ResetScene();
    }

    private void HumanPlayerScored()
    {
        humanPlayerScore++;
        uiController.HumanPlayerScored(humanPlayerScore);
        ResetScene();
    }

    private void ResetScene()
    {
        puckController.transform.GetComponent<MeshRenderer>().enabled = false;
        pusherController.Reset();
        puckController.Reset();

        if (mjScene == null)
        {
            mjScene = GameObject.Find("MjScene").GetComponent<MjScene>();
        }
        mjScene.DestroyScene();
        mjScene.CreateScene();
        puckController.transform.GetComponent<MeshRenderer>().enabled = true;
    }
}
