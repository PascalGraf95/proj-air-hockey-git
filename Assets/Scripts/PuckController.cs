using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using System;
using Assets.Scripts;

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
    public Vector2 startPos = Vector2.zero;    
    public Boundary agentPusherBoundary = new Boundary(68.8f, 0f, -30f, 30f);
    public Boundary humanPusherBoundary = new Boundary(0, -68.8f, -30f, 30f);
    public Boundary puckBoundary = new Boundary(50f, -50f, -30f, 30f);
    private PusherController pusherAgentController;
    private PusherController pusherHumanController;

    // Domain Randomization
    private DomainRandomizationController domainRandomizationController;
    private static DomainRandomizationObservations domainRandomizationObservations;

    // Accelaration calculation
    Vector2 lastVelocity;
    Vector2 currentAccelaration;

    // Start is called before the first frame update
    void Start()
    {
        SetupPuckController();
        // setup observation randomization
        domainRandomizationController = FindObjectOfType<DomainRandomizationController>();
    }

    public void SetupPuckController()
    {
        pusherAgentController = GameObject.Find("PusherAgent").GetComponent<PusherController>();
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

    private void FixedUpdate()
    {
        var currentVelocity = new Vector2(actuatorX.Velocity, actuatorZ.Velocity);
        currentAccelaration = currentVelocity - lastVelocity;
        lastVelocity = currentVelocity;
    }

    public Vector2 GetCurrentPosition()
    {
        if (domainRandomizationController.ApplyObservationRandomization is true)
        {
            float x = domainRandomizationObservations.RandomizeParameter(transform.position.x);
            float z = domainRandomizationObservations.RandomizeParameter(transform.position.z);
            return new Vector2(x, z);
        }
        else
        {
            return new Vector2(transform.position.x, transform.position.z);
        }        
    }

    public Vector2 GetCurrentVelocity()
    {
        if (domainRandomizationController.ApplyObservationRandomization is true)
        {
            float z = domainRandomizationObservations.RandomizeParameter(actuatorZ.Velocity);
            float x = domainRandomizationObservations.RandomizeParameter(actuatorX.Velocity);
            return new Vector2(x, z);
        }
        else
        {
            return new Vector2(actuatorX.Velocity, actuatorZ.Velocity);
        }
    }

    public Vector2 GetCurrentAccelaration()
    {
        if (domainRandomizationController.ApplyObservationRandomization is true)
        {
            float x = domainRandomizationObservations.RandomizeParameter(currentAccelaration.x);
            float z = domainRandomizationObservations.RandomizeParameter(currentAccelaration.y);
            return new Vector2(x, z);
        }
        else
        {
            return currentAccelaration;
        }
    }

    public void Reset()
    {
        slideJointX.Velocity = 0;
        slideJointZ.Velocity = 0;
        slideJointX.Configuration = 0;
        slideJointZ.Configuration = 0;

        if (resetPuckState == ResetPuckState.normalPosition)
        {
            transform.position = new Vector3(0f, 0.1f, 0f);
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
            transform.position = new Vector3(UnityEngine.Random.Range(puckBoundary.Left, puckBoundary.Right) * 0.9f, 0.5f, UnityEngine.Random.Range(agentPusherBoundary.Down, agentPusherBoundary.Up) * 0.9f);
        }
        else if(resetPuckState == ResetPuckState.ColliderTest)
        {
            transform.position = new Vector3(startPos.x, 0.1f, startPos.y);
            slideJointX.Velocity = Mathf.Sin(ANG * Mathf.Deg2Rad) * VEL;
            slideJointZ.Velocity = Mathf.Cos(ANG * Mathf.Deg2Rad) * VEL;
        }

    }
}
