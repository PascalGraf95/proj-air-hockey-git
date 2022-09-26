using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class RewardCompositionUI : MonoBehaviour
{
    [SerializeField] private AirHockeyAgent airHockeyAgent;
    private TextMeshProUGUI scoreRewardText;
    private TextMeshProUGUI scoreRewardShiftText;
    private TextMeshProUGUI directionRewardText;
    private TextMeshProUGUI directionRewardShiftText;
    private TextMeshProUGUI boundaryRewardText;
    private TextMeshProUGUI boundaryRewardShiftText;
    private TextMeshProUGUI puckVelRewardText;
    private TextMeshProUGUI puckVelRewardShiftText;
    private TextMeshProUGUI puckHalfRewardText;
    private TextMeshProUGUI puckHalfRewardShiftText;
    private TextMeshProUGUI backwallRewardText;
    private TextMeshProUGUI backwallRewardShiftText;
    private TextMeshProUGUI stayInCenterRewardText;
    private TextMeshProUGUI stayInCenterRewardShiftftText;

    // Start is called before the first frame update
    void Start()
    {
        scoreRewardText = transform.Find("ScoreRewardText").GetComponent<TextMeshProUGUI>();
        scoreRewardShiftText = transform.Find("ScoreRewardText/ScoreRewardShiftText").GetComponent<TextMeshProUGUI>();
        directionRewardText = transform.Find("DirectionRewardText").GetComponent<TextMeshProUGUI>();
        directionRewardShiftText = transform.Find("DirectionRewardText/DirectionRewardShiftText").GetComponent<TextMeshProUGUI>();
        boundaryRewardText = transform.Find("BoundaryRewardText").GetComponent<TextMeshProUGUI>();
        boundaryRewardShiftText = transform.Find("BoundaryRewardText/BoundaryRewardShiftText").GetComponent<TextMeshProUGUI>();
        puckVelRewardText = transform.Find("PuckVelRewardText").GetComponent<TextMeshProUGUI>();
        puckVelRewardShiftText = transform.Find("PuckVelRewardText/PuckVelRewardShiftText").GetComponent<TextMeshProUGUI>();
        puckHalfRewardText = transform.Find("PuckHalfRewardText").GetComponent<TextMeshProUGUI>();
        puckHalfRewardShiftText = transform.Find("PuckHalfRewardText/PuckHalfRewardShiftText").GetComponent<TextMeshProUGUI>();
        backwallRewardText = transform.Find("BackwallRewardText").GetComponent<TextMeshProUGUI>();
        backwallRewardShiftText = transform.Find("BackwallRewardText/BackwallRewardShiftText").GetComponent<TextMeshProUGUI>();
        stayInCenterRewardText = transform.Find("StayInCenterRewardText").GetComponent<TextMeshProUGUI>();
        stayInCenterRewardShiftftText = transform.Find("StayInCenterRewardText/StayInCenterRewardShiftText").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {

        Dictionary<string, float> episodeReward;
        Dictionary<string, float[]> episodeRewardShift;
        airHockeyAgent.GetRewardComposition(out episodeReward, out episodeRewardShift);

        scoreRewardText.text = "Score: " + episodeReward["ScoreReward"].ToString("0.00");
        scoreRewardShiftText.text = episodeRewardShift["ScoreRewardShift"].Sum().ToString("0.00");
        directionRewardText.text = "Direction: " + episodeReward["DirectionReward"].ToString("0.00");
        directionRewardShiftText.text = episodeRewardShift["DirectionRewardShift"].Sum().ToString("0.00");
        boundaryRewardText.text = "Boundary: " + episodeReward["BoundaryReward"].ToString("0.00");
        boundaryRewardShiftText.text = episodeRewardShift["BoundaryRewardShift"].Sum().ToString("0.00");
        puckVelRewardText.text = "PuckVel: " + episodeReward["PuckVelocityReward"].ToString("0.00");
        puckVelRewardShiftText.text = episodeRewardShift["PuckVelocityRewardShift"].Sum().ToString("0.00");
        puckHalfRewardText.text = "PuckHalf: " + episodeReward["PuckInAgentsHalfReward"].ToString("0.00");
        puckHalfRewardShiftText.text = episodeRewardShift["PuckInAgentsHalfRewardShift"].Sum().ToString("0.00");
        backwallRewardText.text = "Backwall: " + episodeReward["BackwallReward"].ToString("0.00");
        backwallRewardShiftText.text = episodeRewardShift["BackwallRewardShift"].Sum().ToString("0.00");
        stayInCenterRewardText.text = "Center: " + episodeReward["StayInCenterReward"].ToString("0.00");
        stayInCenterRewardShiftftText.text = episodeRewardShift["StayInCenterRewardShift"].Sum().ToString("0.00");

    }
}

