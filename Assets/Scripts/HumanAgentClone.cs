using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class HumanAgentClone : Agent
{
    private Transform agent;
    private Rigidbody humanRB;
    private Rigidbody puckBody;
    private ObservationType observationType;
    private ActionType actionType;
    private FieldBoundary humanBoundary;
    private float maxHumanPusherVelocity;
    private float maxHumanPusherAcceleration;

    /*
     *  Rotationsmatrix 
     *  (-1  0
     *    0 -1)
    */


    public void Init(Transform agent, Rigidbody humanRB, Rigidbody puckBody, 
        ObservationType observationType, 
        ActionType actionType, 
        FieldBoundary humanBoundary, 
        float maxHumanPusherVelocity, 
        float maxHumanPusherAcceleration)
    {
        this.agent = agent;
        this.humanRB = humanRB;
        this.puckBody = puckBody;
        this.observationType = observationType;
        this.actionType = actionType;
        this.humanBoundary = humanBoundary;
        this.maxHumanPusherVelocity = maxHumanPusherVelocity;
        this.maxHumanPusherAcceleration = maxHumanPusherAcceleration;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(-transform.position);
        sensor.AddObservation(-humanRB.velocity);
        sensor.AddObservation(-puckBody.position);

        //if (observationType == ObservationType.AgentPuckHuman || observationType == ObservationType.AgentPuckHumanVelocity)
        //{
        //    sensor.AddObservation(-agent.position);
        //}
        //if (observationType == ObservationType.AgentPuckVelocity || observationType == ObservationType.AgentPuckHumanVelocity)
        //{
        //    sensor.AddObservation(-puckBody.velocity);
        //}
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

        // Normalize so going diagonally doesn't speed things up
        Vector3 direction = new Vector3(-x, 0f, -z);
        if (direction.magnitude > 1f)
        {
            direction.Normalize();
        }
        #endregion
        #region Movement and Clipping

        // Apply Force
        humanRB.AddForce(direction * maxHumanPusherAcceleration * humanRB.mass * Time.fixedDeltaTime);
        
        // Limit Velocity
        if (humanRB.velocity.magnitude > maxHumanPusherVelocity)
        {
            humanRB.velocity = humanRB.velocity.normalized * maxHumanPusherVelocity;
        }
        // Limit Position
        if (humanRB.position.x < humanBoundary.xMin)
        {
            humanRB.velocity = new Vector3(0, 0, humanRB.velocity.z);
            humanRB.position = new Vector3(humanBoundary.xMin, 0, humanRB.position.z);
        }
        else if (humanRB.position.x > humanBoundary.xMax)
        {
            humanRB.velocity = new Vector3(0, 0, humanRB.velocity.z);
            humanRB.position = new Vector3(humanBoundary.xMax, 0, humanRB.position.z);
        }
        if (humanRB.position.z < humanBoundary.zMin)
        {
            humanRB.velocity = new Vector3(humanRB.velocity.x, 0, 0);
            humanRB.position = new Vector3(humanRB.position.x, 0, humanBoundary.zMin);
        }
        else if (humanRB.position.z > humanBoundary.zMax)
        {
            humanRB.velocity = new Vector3(humanRB.velocity.x, 0, 0);
            humanRB.position = new Vector3(humanRB.position.x, 0, humanBoundary.zMax);
        }
        #endregion
        
    }

}
