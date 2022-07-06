using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private Transform agentScoreTextHumanPerspective;
    [SerializeField] private Transform agentScoreTextTopPerspective;
    [SerializeField] private Transform humanScoreTextHumanPerspective;
    [SerializeField] private Transform humanScoreTextTopPerspective;
    [SerializeField] private Transform humanGoalLight;
    [SerializeField] private Transform agentGoalLight;

    public void ResetUI()
    {
        agentScoreTextHumanPerspective.GetComponent<TextMeshPro>().text = "0";
        agentScoreTextTopPerspective.GetComponent<TextMeshPro>().text = "0";
        humanScoreTextHumanPerspective.GetComponent<TextMeshPro>().text = "0";
        humanScoreTextTopPerspective.GetComponent<TextMeshPro>().text = "0";

        humanGoalLight.GetComponent<Animator>().SetTrigger("GoalScored");
        agentGoalLight.GetComponent<Animator>().SetTrigger("GoalScored");
    }

    public void AgentPlayerScored(int score)
    {
        agentScoreTextHumanPerspective.GetComponent<TextMeshPro>().text = score.ToString();
        agentScoreTextHumanPerspective.GetComponent<Animator>().SetTrigger("GoalScored");
        agentScoreTextTopPerspective.GetComponent<TextMeshPro>().text = score.ToString();
        agentScoreTextTopPerspective.GetComponent<Animator>().SetTrigger("GoalScored");
        humanGoalLight.GetComponent<Animator>().SetTrigger("GoalScored");

    }

    public void HumanPlayerScored(int score)
    {
        humanScoreTextHumanPerspective.GetComponent<TextMeshPro>().text = score.ToString();
        humanScoreTextHumanPerspective.GetComponent<Animator>().SetTrigger("GoalScored");
        humanScoreTextTopPerspective.GetComponent<TextMeshPro>().text = score.ToString();
        humanScoreTextHumanPerspective.GetComponent<Animator>().SetTrigger("GoalScored");
        agentGoalLight.GetComponent<Animator>().SetTrigger("GoalScored");

    }
}
