using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Example : MonoBehaviour
{
    public int dataPoints = 100;
    public SimplestPlot SimplestPlotScript;
    public SimplestPlot.PlotType plotType;
    public AirHockeyAgent airHockeyAgent;
    private float counter = 0;
    private Color[] MyColors = new Color[2];

    private System.Random MyRandom;
    private float[] xValues;
    private float[] y1Values;
    private float[] y2Values;

    private Vector2 Resolution;
    // Use this for initialization
    void Start()
    {
        MyRandom = new System.Random();
        xValues = new float[dataPoints];
        y1Values = new float[dataPoints];
        y2Values = new float[dataPoints];
        MyColors[0] = Color.white;
        MyColors[1] = Color.blue;

        var res = new Vector2(400, 400);
        SimplestPlotScript.SetResolution(res);
        SimplestPlotScript.BackGroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        SimplestPlotScript.TextColor = Color.yellow;

        for (int Cnt = 0; Cnt < 2; Cnt++)
        {
            SimplestPlotScript.SeriesPlotY.Add(new SimplestPlot.SeriesClass());
            SimplestPlotScript.SeriesPlotY[Cnt].MyColor = MyColors[Cnt];
        }
        Resolution = SimplestPlotScript.GetResolution();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        counter++;
        PrepareArrays();
        SimplestPlotScript.MyPlotType = plotType;
        SimplestPlotScript.SeriesPlotY[0].YValues = y1Values;
        SimplestPlotScript.SeriesPlotY[1].YValues = y2Values;
        SimplestPlotScript.SeriesPlotX = xValues;
        SimplestPlotScript.UpdatePlot();
    }
    private void PrepareArrays()
    {
        for (int Cnt = 0; Cnt < dataPoints-1; Cnt++)
        {
            xValues[Cnt] = Cnt;
            y1Values[Cnt] = y1Values[Cnt + 1];
            y2Values[Cnt] = y2Values[Cnt + 1];
        }
        y1Values[dataPoints - 1] = airHockeyAgent.currentAccMag;
        y2Values[dataPoints - 1] = airHockeyAgent.currentJerkMag;
    }
}
