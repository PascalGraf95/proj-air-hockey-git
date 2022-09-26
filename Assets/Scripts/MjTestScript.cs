using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using System;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MjTestScript : Agent
{
    private MjScene mjScene;
    private DateTime lastReset;
    private DateTime now;

    // Start is called before the first frame update
    void Start()
    {
        lastReset = DateTime.Now;
    }
    public override void OnEpisodeBegin()
    {
        Reset();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
    }


    public void Reset()
    {
        GameObject.Find("SphereBody").transform.position = new Vector3(0f, 15f, 0f);
        // Mujoco Scene Reset
        if (mjScene == null)
        {
            mjScene = GameObject.Find("MjScene").GetComponent<MjScene>();
        }
        mjScene.DestroyScene();
        mjScene.CreateScene();
    }
}
