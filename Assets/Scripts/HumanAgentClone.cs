using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class HumanAgentClone : Agent
{
    private PuckController puckController;
    private PusherController pusherAgentController;
    private PusherController pusherHumanController;
    [SerializeField] private ActionType actionType = ActionType.Continuous;

    /*
     *  Rotationsmatrix 
     *  (-1  0
     *    0 -1)
    */

    private void Start()
    {
        pusherAgentController = GameObject.Find("PusherAgent").GetComponent<PusherController>();
        puckController = GameObject.Find("Puck").GetComponent<PuckController>();
        pusherHumanController = GameObject.Find("PusherHuman").GetComponent<PusherController>();
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

        if (actionType == ActionType.Continuous)
        {
            var continouosActions = actionsIn.ContinuousActions;
            // MOVEMENT CALCULATIONS
            x = continouosActions[0];
            z = continouosActions[1];
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

        // Action Dead Zone to avoid unneccessary movement
        if (Mathf.Abs(x) < 0.03f)
        {
            x = 0f;
        }
        if (Mathf.Abs(z) < 0.03f)
        {
            z = 0f;
        }
        #endregion
        #region Movement and Clipping
        pusherHumanController.Act(new Vector2(-x, -z));
        #endregion
        
    }

}
