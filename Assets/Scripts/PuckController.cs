using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using System;

public class PuckController : MonoBehaviour
{
    [SerializeField] private MjSlideJoint slideJointX;
    [SerializeField] private MjSlideJoint slideJointZ;
    [SerializeField] private MjActuator actuatorX;
    [SerializeField] private MjActuator actuatorZ;
    [SerializeField] private ResetPuckState resetPuckState;
    private GameState gameState;
    private FieldBoundary agentBoundary = new FieldBoundary();
    private FieldBoundary puckBoundary = new FieldBoundary();

    private MjScene mjScene;
    private int episodeNumber;
    private float episodeReward;
    private DateTime resetTime;

    // Start is called before the first frame update
    void Start()
    {
        agentBoundary.xMin = -30f;
        agentBoundary.xMax = 30f;
        agentBoundary.zMin = 0f;
        agentBoundary.zMax = 50f;

        puckBoundary.xMin = -30f;
        puckBoundary.xMax = 30f;
        puckBoundary.zMin = -50f;
        puckBoundary.zMax = 50f;

    }
    private void Update()
    {
    }


    public void Reset()
    {
        slideJointX.Velocity = 0;
        slideJointZ.Velocity = 0;
        slideJointX.Configuration = 0;
        slideJointZ.Configuration = 0;

        if (resetPuckState == ResetPuckState.normalPosition)
        {
            transform.position = Vector3.zero;
        }
        else if (resetPuckState == ResetPuckState.randomPosition)
        {
            var posX = UnityEngine.Random.Range(agentBoundary.xMin, agentBoundary.xMax) * 0.9f;
            var posZ = UnityEngine.Random.Range(agentBoundary.zMin, agentBoundary.zMax) * 0.9f;
            slideJointX.Configuration = posX;
            slideJointZ.Configuration = posZ;
            transform.position = new Vector3(posX, 0.1f, posZ); 
        }
        else if (resetPuckState == ResetPuckState.randomMiddlePosition)
        {
            transform.position = new Vector3(UnityEngine.Random.Range(puckBoundary.xMin, puckBoundary.xMax) * 0.9f, 0.5f, UnityEngine.Random.Range(agentBoundary.zMin, agentBoundary.zMax) * 0.9f);
        }

    }
}
