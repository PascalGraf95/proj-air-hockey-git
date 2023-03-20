using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyButtons;
using System.IO;
using System.Diagnostics;

public class ExportGameStatistics : MonoBehaviour
{
    [SerializeField] private SceneController sceneController;
    [SerializeField] private AirHockeyAgent airHockeyAgentScript;
    [SerializeField] private PusherController pusherController;
    [SerializeField] private PuckController puckController;
    [SerializeField] private string imagePath = "";
    [SerializeField] private string csvPath = "";
    [SerializeField] private bool stopCapturingOnEpisodeEnd = true;
    private bool captureEpisode = false;
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        sceneController.onEpisodeEnded += EpisodeEnded;
        cam = GameObject.Find("TopViewCameraRendering").GetComponent<Camera>();
        //airHockeyAgentScript.onNewActionReceived += ExportData;
    }

    [Button]
    public void CaptureNextEpisode()
    {
        captureEpisode = true;
    }

    public void EpisodeEnded()
    {
        if(stopCapturingOnEpisodeEnd)
        {
            captureEpisode = false;
        }
    }

    public void ExportData()
    {
        if (captureEpisode)
        {

            string filename = string.Format(imagePath + "/img_{0}.png", System.DateTime.Now.ToString("yy-MM-dd_HH-mm-ss-fff"));
            ScreenCapture.CaptureScreenshot(filename, 1);
        }
    }
    private void FixedUpdate()
    {
        ExportData();
    } 
}
