using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;
using Mujoco;

public class HumanAgentClone : Agent
{
    private PuckController puckController;
    private PusherController pusherAgentController;
    private PusherController pusherHumanController;
    [SerializeField] private ActionType actionType = ActionType.ContinuousVelocity;

    public MjActuator pusherActuatorZ;
    public MjActuator pusherActuatorX;

    /*
     *  Rotationsmatrix 
     *  (-1  0
     *    0 -1)
    */

    private void Start()
    {
        pusherAgentController = GameObject.Find("PusherAgent").GetComponent<PusherController>();
        puckController = GameObject.Find("Puck").GetComponent<PuckController>();
        if (GameObject.Find("PusherHuman") != null)
        {
            pusherHumanController = GameObject.Find("PusherHuman").GetComponent<PusherController>();
        }
        else if (GameObject.Find("PusherHumanSelfplay") != null)
        {
            pusherHumanController = GameObject.Find("PusherHumanSelfplay").GetComponent<PusherController>();
        }
        else
        {
            Debug.LogError("Pusher Human GameObject not found.");
        }

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(-pusherHumanController.GetCurrentPosition());
        sensor.AddObservation(-pusherHumanController.GetCurrentVelocity());
        sensor.AddObservation(-pusherAgentController.GetCurrentPosition());
        sensor.AddObservation(-pusherAgentController.GetCurrentVelocity());
        sensor.AddObservation(-puckController.GetCurrentPosition());
        sensor.AddObservation(-puckController.GetCurrentVelocity());
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = 0f;
        continuousActionsOut[1] = 0f;
    }

    public override void OnActionReceived(ActionBuffers actionsIn)
    {
        #region Action Calculations
        float x = 0f;
        float z = 0f;
        bool setNewTarget = false;

        if (actionType == ActionType.ContinuousVelocity)
        {
            var continouosActions = actionsIn.ContinuousActions;
            // MOVEMENT CALCULATIONS
            x = continouosActions[0];
            z = continouosActions[1];

            // Action Dead Zone to avoid unneccessary movement
            if (Mathf.Abs(x) < 0.03f)
            {
                x = 0f;
            }
            if (Mathf.Abs(z) < 0.03f)
            {
                z = 0f;
            }
        }
        else if (actionType == ActionType.ContinuousPosition)
        {
            var continouosActions = actionsIn.ContinuousActions;
            // MOVEMENT CALCULATIONS
            x = continouosActions[0];
            z = continouosActions[1];
            setNewTarget = (continouosActions[2] > 0.5);
        }
        else
        {
            var discreteActions = actionsIn.DiscreteActions;
            switch (discreteActions[0])
            {
                case 0:
                    x = 0;
                    z = 0;
                    break;
                case 1:
                    x = 1;
                    z = 0;
                    break;
                case 2:
                    x = -1;
                    z = 0;
                    break;
                case 3:
                    x = 0;
                    z = 1;
                    break;
                case 4:
                    x = 0;
                    z = -1;
                    break;
            }
        }
        #endregion
        #region Movement and Clipping
        if (actionType == ActionType.ContinuousPosition && !setNewTarget)
        {
            return;
        }
        else
        {
            pusherHumanController.Act(new Vector2(-x, -z));
        }
        #endregion

    }
}
