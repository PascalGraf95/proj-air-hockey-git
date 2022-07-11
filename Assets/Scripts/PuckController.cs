using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using System;

public enum ResetPuckState
{
    normalPosition,
    randomPosition,
    randomPositionGlobal,
    shotOnGoal,
    randomVelocity,
    randomMiddlePosition,
    ColliderTest
}

public class PuckController : MonoBehaviour
{
    [SerializeField] private MjSlideJoint slideJointX;
    [SerializeField] private MjSlideJoint slideJointZ;
    [SerializeField] private MjActuator actuatorX;
    [SerializeField] private MjActuator actuatorZ;
    public ResetPuckState resetPuckState;
    public float VEL = 0f;
    public float ANG = 0f;
    public Vector3 startPos = Vector3.zero;
    public Boundary agentPusherBoundary = new Boundary(68.8f, 0f, -30f, 30f);
    public Boundary humanPusherBoundary = new Boundary(0, -68.8f, -30f, 30f);
    public Boundary puckBoundary = new Boundary(50f, -50f, -30f, 30f);
    private PusherController pusherAgentController;
    private PusherController pusherHumanController;

    // Start is called before the first frame update
    void Start()
    {
        pusherAgentController = GameObject.Find("PusherAgent").GetComponent<PusherController>();
        pusherHumanController = GameObject.Find("PusherHuman").GetComponent<PusherController>();
    }

    public Vector2 GetCurrentPosition()
    {
        return new Vector2(transform.position.x, transform.position.z);
    }

    public Vector2 GetCurrentVelocity()
    {
        return new Vector2(actuatorX.Velocity, actuatorZ.Velocity);
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
            Vector2 newPuckPosition;
            while(true)
            {
                var posX = UnityEngine.Random.Range(agentPusherBoundary.Left, agentPusherBoundary.Right) * 0.9f;
                var posZ = UnityEngine.Random.Range(agentPusherBoundary.Down, agentPusherBoundary.Up) * 0.9f;
                newPuckPosition = new Vector2(posX, posZ);
                if (Vector2.Distance(newPuckPosition, pusherAgentController.GetCurrentPosition()) > 5f &&
                    Vector2.Distance(newPuckPosition, pusherHumanController.GetCurrentPosition()) > 5f) break;
            }
            slideJointX.Configuration = newPuckPosition.x;
            slideJointZ.Configuration = newPuckPosition.y;
            transform.position = new Vector3(newPuckPosition.x, 0.1f, newPuckPosition.y); 
        }
        else if (resetPuckState == ResetPuckState.randomPositionGlobal || resetPuckState == ResetPuckState.randomVelocity)
        {
            Vector2 newPuckPosition;
            while (true)
            {
                var posX = UnityEngine.Random.Range(puckBoundary.Left, puckBoundary.Right) * 0.9f;
                var posZ = UnityEngine.Random.Range(puckBoundary.Down, puckBoundary.Up) * 0.9f;
                newPuckPosition = new Vector2(posX, posZ);
                if (Vector2.Distance(newPuckPosition, pusherAgentController.GetCurrentPosition()) > 5f && 
                    Vector2.Distance(newPuckPosition, pusherHumanController.GetCurrentPosition()) > 5f)  break;
            }
            slideJointX.Configuration = newPuckPosition.x;
            slideJointZ.Configuration = newPuckPosition.y;
            transform.position = new Vector3(newPuckPosition.x, 0.1f, newPuckPosition.y);

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
        else if (resetPuckState == ResetPuckState.ColliderTest)
        {
            slideJointX.Configuration = startPos.x;
            slideJointZ.Configuration = startPos.z;
            transform.position = new Vector3(startPos.x, 0.1f, startPos.z);

            slideJointX.Velocity = Mathf.Sin(ANG * Mathf.Deg2Rad) * VEL;
            slideJointZ.Velocity = Mathf.Cos(ANG * Mathf.Deg2Rad) * VEL;

        }
    }
}
