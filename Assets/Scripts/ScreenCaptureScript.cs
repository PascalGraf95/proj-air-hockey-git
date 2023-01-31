using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyButtons;

public class ScreenCaptureScript : MonoBehaviour
{
    public string ExportPath = "Screencapture/";
    public string FileName = "Airhockey_Gameview";
    public string FileType = ".png";
    public int IncreaseResolutionFactor = 1;

    [Button]
    public void CaptureScreen()
    {
        ScreenCapture.CaptureScreenshot(ExportPath + FileName + FileType, IncreaseResolutionFactor);
    }
}
