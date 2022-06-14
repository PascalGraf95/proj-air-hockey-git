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
            /*
            if (gameState == GameState.agentScored || gameState == GameState.backWallReached)
            {
                puckRB.position = new Vector3(0, 0, -1);
            }
            else if (gameState == GameState.playerScored)
            {
                puckRB.position = new Vector3(0, 0, 1);
            }
            */
        }
        else if (resetPuckState == ResetPuckState.randomPosition)
        {
            var posX = UnityEngine.Random.Range(agentBoundary.xMin, agentBoundary.xMax) * 0.9f;
            var posZ = UnityEngine.Random.Range(agentBoundary.zMin, agentBoundary.zMax) * 0.9f;
            slideJointX.Configuration = posX;
            slideJointZ.Configuration = posZ;
            transform.position = new Vector3(posX, 0.1f, posZ); 
        }
        else if (resetPuckState == ResetPuckState.randomPositionGlobal || resetPuckState == ResetPuckState.randomVelocity)
        {
            var posX = UnityEngine.Random.Range(puckBoundary.xMin, puckBoundary.xMax) * 0.9f;
            var posZ = UnityEngine.Random.Range(puckBoundary.zMin, puckBoundary.zMax) * 0.9f;
            slideJointX.Configuration = posX;
            slideJointZ.Configuration = posZ;
            transform.position = new Vector3(posX, 0.1f, posZ);

            if (resetPuckState == ResetPuckState.randomVelocity)
            {
                var ang = UnityEngine.Random.Range(-180f, 180f);
                var vel = UnityEngine.Random.Range(30f, 150f);

                slideJointX.Velocity = Mathf.Sin(ang * Mathf.Deg2Rad) * vel;
                slideJointZ.Velocity = Mathf.Cos(ang * Mathf.Deg2Rad) * vel;
                //actuatorX.Control = Mathf.Sin(ang * Mathf.Deg2Rad) * vel * 100;
                //actuatorZ.Control = Mathf.Cos(ang * Mathf.Deg2Rad) * vel * 100;
            }
        }
        else if (resetPuckState == ResetPuckState.randomMiddlePosition)
        {
            //puckRB.position = new Vector3(Random.Range(puckBoundary.xMin, puckBoundary.xMax) * 0.9f, 0f, Random.Range(agentBoundary.zMin, agentBoundary.zMax) * 0.9f);
        }

        else if (resetPuckState == ResetPuckState.shotOnGoal)
        {
            /*
            foreach (Transform m in markerContainer)
            {
                Destroy(m.gameObject);
            }

            var currentPoint = new Vector3(0f, 0f, 75f);
            //Instantiate(marker, new Vector3(currentPoint.x, 0, currentPoint.z), Quaternion.identity, markerContainer);
            var angle = Random.Range(-60f, 60f);
            var spawnLine = Random.Range(puckBoundary.zMin, puckBoundary.zMax);

            Vector3 nextPoint = Vector3.zero;
            Vector3 startingVelocity = Vector3.zero;
            while (true)
            {
                if (angle > 0)
                {
                    nextPoint = new Vector3(puckBoundary.xMax, 0f, currentPoint.z - (puckBoundary.xMax - currentPoint.x) / Mathf.Tan(angle * Mathf.Deg2Rad));
                }
                else
                {
                    nextPoint = new Vector3(puckBoundary.xMin, 0f, currentPoint.z - (puckBoundary.xMin - currentPoint.x) / Mathf.Tan(angle * Mathf.Deg2Rad));
                }
                if (nextPoint.z < spawnLine)
                {
                    nextPoint = new Vector3(currentPoint.x - (spawnLine - currentPoint.z) * Mathf.Tan(angle * Mathf.Deg2Rad), 0f, spawnLine);
                    //Debug.DrawLine(currentPoint, nextPoint, Color.green, 3f);
                    //Instantiate(marker, new Vector3(nextPoint.x, 0, nextPoint.z), Quaternion.identity, markerContainer);
                    angle = -angle;
                    startingVelocity = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0f, Mathf.Cos(angle * Mathf.Deg2Rad)) * Random.Range(80f, 400f);
                    break;
                }
                else
                {
                    angle = -angle;
                    //Debug.DrawLine(currentPoint, nextPoint, Color.green, 3f, false);
                    currentPoint = nextPoint;
                }
                //Instantiate(marker, new Vector3(nextPoint.x, 0, nextPoint.z), Quaternion.identity, markerContainer);
            }
            puckRB.position = nextPoint;
            puckRB.velocity = startingVelocity;
            */
        }
    }
}
