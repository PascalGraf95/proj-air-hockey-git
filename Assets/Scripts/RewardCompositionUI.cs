using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class RewardCompositionUI : MonoBehaviour
{
    [SerializeField] private AirHockeyAgent airHockeyAgent;
    private TextMeshProUGUI scoreRewardEpisodeText;
    private TextMeshProUGUI scoreRewardShiftText;
    private TextMeshProUGUI directionRewardEpisodeText;
    private TextMeshProUGUI directionRewardShiftText;
    private TextMeshProUGUI boundaryRewardEpisodeText;
    private TextMeshProUGUI boundaryRewardShiftText;
    private TextMeshProUGUI puckVelRewardEpisodeText;
    private TextMeshProUGUI puckVelRewardShiftText;
    private TextMeshProUGUI puckHalfRewardEpisodeText;
    private TextMeshProUGUI puckHalfRewardShiftText;
    private TextMeshProUGUI backwallRewardEpisodeText;
    private TextMeshProUGUI backwallRewardShiftText;
    private TextMeshProUGUI stayInCenterRewardEpisodeText;
    private TextMeshProUGUI stayInCenterRewardShiftftText;
    private TextMeshProUGUI totalRewardEpisodeText;
    private TextMeshProUGUI totalRewardShiftText;

    // Start is called before the first frame update
    void Start()
    {
        scoreRewardEpisodeText = transform.Find("ScoreRewardText/ScoreRewardEpisodeText").GetComponent<TextMeshProUGUI>();
        scoreRewardShiftText = transform.Find("ScoreRewardText/ScoreRewardShiftText").GetComponent<TextMeshProUGUI>();
        directionRewardEpisodeText = transform.Find("DirectionRewardText/DirectionRewardEpisodeText").GetComponent<TextMeshProUGUI>();
        directionRewardShiftText = transform.Find("DirectionRewardText/DirectionRewardShiftText").GetComponent<TextMeshProUGUI>();
        boundaryRewardEpisodeText = transform.Find("BoundaryRewardText/BoundaryRewardEpisodeText").GetComponent<TextMeshProUGUI>();
        boundaryRewardShiftText = transform.Find("BoundaryRewardText/BoundaryRewardShiftText").GetComponent<TextMeshProUGUI>();
        puckVelRewardEpisodeText = transform.Find("PuckVelRewardText/PuckVelRewardEpisodeText").GetComponent<TextMeshProUGUI>();
        puckVelRewardShiftText = transform.Find("PuckVelRewardText/PuckVelRewardShiftText").GetComponent<TextMeshProUGUI>();
        puckHalfRewardEpisodeText = transform.Find("PuckHalfRewardText/PuckHalfRewardEpisodeText").GetComponent<TextMeshProUGUI>();
        puckHalfRewardShiftText = transform.Find("PuckHalfRewardText/PuckHalfRewardShiftText").GetComponent<TextMeshProUGUI>();
        backwallRewardEpisodeText = transform.Find("BackwallRewardText/BackwallRewardEpisodeText").GetComponent<TextMeshProUGUI>();
        backwallRewardShiftText = transform.Find("BackwallRewardText/BackwallRewardShiftText").GetComponent<TextMeshProUGUI>();
        stayInCenterRewardEpisodeText = transform.Find("StayInCenterRewardText/StayInCenterRewardEpisodeText").GetComponent<TextMeshProUGUI>();
        stayInCenterRewardShiftftText = transform.Find("StayInCenterRewardText/StayInCenterRewardShiftText").GetComponent<TextMeshProUGUI>();
        totalRewardEpisodeText = transform.Find("TotalRewardText/TotalRewardEpisodeText").GetComponent<TextMeshProUGUI>();
        totalRewardShiftText = transform.Find("TotalRewardText/TotalRewardShiftText").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        Dictionary<string, float> episodeReward;
        Dictionary<string, float[]> episodeRewardShift;
        airHockeyAgent.GetRewardComposition(out episodeReward, out episodeRewardShift);

        scoreRewardEpisodeText.text = episodeReward["ScoreReward"].ToString("0.00");
        scoreRewardShiftText.text = episodeRewardShift["ScoreRewardShift"].Sum().ToString("0.00");
        directionRewardEpisodeText.text = episodeReward["DirectionReward"].ToString("0.00");
        directionRewardShiftText.text = episodeRewardShift["DirectionRewardShift"].Sum().ToString("0.00");
        boundaryRewardEpisodeText.text = episodeReward["BoundaryReward"].ToString("0.00");
        boundaryRewardShiftText.text = episodeRewardShift["BoundaryRewardShift"].Sum().ToString("0.00");
        puckVelRewardEpisodeText.text = episodeReward["PuckVelocityReward"].ToString("0.00");
        puckVelRewardShiftText.text = episodeRewardShift["PuckVelocityRewardShift"].Sum().ToString("0.00");
        puckHalfRewardEpisodeText.text = episodeReward["PuckInAgentsHalfReward"].ToString("0.00");
        puckHalfRewardShiftText.text = episodeRewardShift["PuckInAgentsHalfRewardShift"].Sum().ToString("0.00");
        backwallRewardEpisodeText.text = episodeReward["BackwallReward"].ToString("0.00");
        backwallRewardShiftText.text = episodeRewardShift["BackwallRewardShift"].Sum().ToString("0.00");
        stayInCenterRewardEpisodeText.text = episodeReward["StayInCenterReward"].ToString("0.00");
        stayInCenterRewardShiftftText.text = episodeRewardShift["StayInCenterRewardShift"].Sum().ToString("0.00");

        // calculate total reward
        float totalRewardShift = 0;
        float totalRewardEpisode = 0;
        foreach (var item in episodeReward)
        {
            totalRewardEpisode += item.Value;
        }
        foreach (var item in episodeRewardShift)
        {
            totalRewardShift += item.Value.Sum();
        }

        totalRewardEpisodeText.text = totalRewardEpisode.ToString("0.00");
        totalRewardShiftText.text = totalRewardShift.ToString("0.00");

    }
}

