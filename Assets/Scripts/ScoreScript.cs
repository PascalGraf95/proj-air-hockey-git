using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    public enum Score
    {
        AIScore, PlayerScore
    }

    public Text AiScoreTxt, PlayerScoreTxt;
    private int aiScore, playerScore;

    public void Increment(Score whichScore)
    {
        if (whichScore == Score.AIScore)
            AiScoreTxt.text = (++aiScore).ToString();
        else
            PlayerScoreTxt.text = (++playerScore).ToString();
    }
}
