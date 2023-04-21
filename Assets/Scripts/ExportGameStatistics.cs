using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyButtons;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Text;

public class ExportGameStatistics : MonoBehaviour
{
    [SerializeField] private SceneController sceneController;
    [SerializeField] private AirHockeyAgent airHockeyAgentScript;
    [SerializeField] private PusherController pusherController;
    [SerializeField] private PuckController puckController;
    [SerializeField] private string imagePath = "";
    [SerializeField] private string csvPath = "";
    [SerializeField] private bool stopCapturingOnEpisodeEnd = true;
    [SerializeField] private int screenshotSuperSize = 1;
    [SerializeField] private List<GameObject> GameObjectsToTrack;
    [SerializeField] private int Step;
    private int stepCounter;
    private Dictionary<GameObject, StringBuilder> csvData;
    private NumberFormatInfo numberFormat;
    private bool captureEpisode = false;

    // Start is called before the first frame update
    void Start()
    {
        sceneController.onEpisodeEnded += EpisodeEnded;
        SetupPositionLogger();
    }

    private void SetupPositionLogger()
    {
        stepCounter = 0;
        csvData = new Dictionary<GameObject, StringBuilder>();
        numberFormat = new NumberFormatInfo();
        numberFormat.NumberDecimalSeparator = ".";

        foreach (GameObject go in GameObjectsToTrack)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Step,PosX,PosY,PosZ");
            csvData.Add(go, sb);
        }
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

    public void ExportScreenshotData()
    {
        if (captureEpisode)
        {
            string filename = string.Format(imagePath + "/img_{0}.png", System.DateTime.Now.ToString("yy-MM-dd_HH-mm-ss-fff"));
            ScreenCapture.CaptureScreenshot(filename, screenshotSuperSize);
        }
    }

    private void ExportPositionData()
    {
        foreach (GameObject go in GameObjectsToTrack)
        {
            string fileName = $"{go.name}_position_data.csv";
            string filePath = Path.Combine(csvPath, fileName);
            string csvContent = csvData[go].ToString();

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, csvContent);
        }
    }

    private void LogPositions()
    {
        stepCounter++;
        if (stepCounter % Step == 0)
        {
            foreach (GameObject go in GameObjectsToTrack)
            {
                Vector3 position = go.transform.position;
                StringBuilder sb = csvData[go];
                sb.AppendLine($"{stepCounter},{position.x.ToString(numberFormat)},{position.y.ToString(numberFormat)},{position.z.ToString(numberFormat)}");
            }
        }
    }

    private void FixedUpdate()
    {
        ExportScreenshotData();
        LogPositions();
    }

    private void OnDestroy()
    {
        ExportPositionData();
    }
}
