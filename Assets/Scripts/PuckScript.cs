using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public enum GameState
{
    normal,
    playerScored,
    agentScored,
    backWallReached,
    puckStopped
}

public class PuckScript : MonoBehaviour
{
    public bool AgentContact { get { return agentContact; } }
    public Rigidbody PuckRB { get { return puckRB; } }

    private Rigidbody puckRB;
    [HideInInspector]
    public GameState gameState;
    private float maxPuckVelocity;
    private bool agentContact;
    public GameObject marker;
    private Transform markerContainer;
    ResetPuckState resetPuckState;
    FieldBoundary puckBoundary;
    FieldBoundary agentBoundary;

    public void Init(ResetPuckState resetPuckState, float maxPuckVelocity, FieldBoundary agentBoundary, GameObject puckMarkerPrefab)
    {
        puckRB = GetComponent<Rigidbody>();
        markerContainer = GameObject.Find("MarkerContainer").transform;
        puckBoundary = GameObject.Find("PuckSpawnBoundaries").GetComponent<FieldBoundary>();
        this.resetPuckState = resetPuckState;
        this.agentBoundary = agentBoundary;
        this.maxPuckVelocity = maxPuckVelocity;
        marker = puckMarkerPrefab;
    }

    public void Reset()
    {
        puckRB.velocity = puckRB.position = Vector3.zero;
        puckRB.angularVelocity = Vector3.zero;

        if (resetPuckState == ResetPuckState.normalPosition)
        {
            if (gameState == GameState.agentScored || gameState == GameState.backWallReached)
            {
                puckRB.position = new Vector3(0, 0, -1);
            }
            else if(gameState == GameState.playerScored)
            {
                puckRB.position = new Vector3(0, 0, 1);
            }
        }
        else if(resetPuckState == ResetPuckState.randomPosition)
        {
            puckRB.position = new Vector3(Random.Range(agentBoundary.xMin, agentBoundary.xMax) * 0.9f, 0f, Random.Range(agentBoundary.zMin, agentBoundary.zMax) * 0.9f);
        }
        else if(resetPuckState == ResetPuckState.randomPositionGlobal || resetPuckState == ResetPuckState.randomVelocity)
        {
            puckRB.position = new Vector3(Random.Range(puckBoundary.xMin, puckBoundary.xMax) * 0.9f, 0f, Random.Range(-agentBoundary.zMax, agentBoundary.zMax) * 0.9f);
            if(resetPuckState == ResetPuckState.randomVelocity)
            {
                puckRB.velocity = new Vector3(Mathf.Sin(Random.Range(-180f, 180f) * Mathf.Deg2Rad), 0f, Mathf.Cos(Random.Range(-180f, 180f) * Mathf.Deg2Rad)) * Random.Range(30f, 150f);
            }
        }
        else if(resetPuckState == ResetPuckState.randomMiddlePosition)
        {
            puckRB.position = new Vector3(Random.Range(puckBoundary.xMin, puckBoundary.xMax) * 0.9f, 0f, Random.Range(agentBoundary.zMin, agentBoundary.zMax) * 0.9f);
        }
        else if(resetPuckState == ResetPuckState.shotOnGoal)
        {
            foreach(Transform m in markerContainer)
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
                if(angle > 0)
                {
                    nextPoint = new Vector3(puckBoundary.xMax, 0f, currentPoint.z - (puckBoundary.xMax - currentPoint.x) / Mathf.Tan(angle * Mathf.Deg2Rad));
                }
                else
                {
                    nextPoint = new Vector3(puckBoundary.xMin, 0f, currentPoint.z - (puckBoundary.xMin - currentPoint.x) / Mathf.Tan(angle * Mathf.Deg2Rad));
                }
                if(nextPoint.z < spawnLine)
                {
                    nextPoint = new Vector3(currentPoint.x - (spawnLine - currentPoint.z) * Mathf.Tan(angle * Mathf.Deg2Rad), 0f, spawnLine);
                    //Debug.DrawLine(currentPoint, nextPoint, Color.green, 3f);
                    //Instantiate(marker, new Vector3(nextPoint.x, 0, nextPoint.z), Quaternion.identity, markerContainer);
                    angle = -angle;
                    startingVelocity = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0f, Mathf.Cos(angle * Mathf.Deg2Rad))*Random.Range(80f, 400f);
                    break;
                }
                else { 
                    angle = -angle;
                    //Debug.DrawLine(currentPoint, nextPoint, Color.green, 3f, false);
                    currentPoint = nextPoint;
                }
                //Instantiate(marker, new Vector3(nextPoint.x, 0, nextPoint.z), Quaternion.identity, markerContainer);
            }                  
            puckRB.position = nextPoint;
            puckRB.velocity = startingVelocity;
        }
        agentContact = false;
        gameState = GameState.normal;
    }

    private void OnTriggerEnter(Collider other)
    {
        gameState = GameState.normal;

        if (other.tag == "AgentGoal")
        {
            gameState = GameState.playerScored;
        }
        else if (other.tag == "HumanGoal")
        {
            gameState = GameState.agentScored;
        }
        else if (other.tag == "HumanBackWall")
        {
            gameState = GameState.backWallReached;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Agent")
        {
            agentContact = true;
        }
        //if (collision.gameObject.tag == "Player" && agentContact == true)
        //{
        //    agentContact = false;
        //}
    }

    private void FixedUpdate()
    {
        if(puckRB.velocity.magnitude > maxPuckVelocity)
        {
            puckRB.ResetInertiaTensor();
        }
        puckRB.velocity = Vector3.ClampMagnitude(puckRB.velocity, maxPuckVelocity);
        if(puckRB.velocity.magnitude == 0)
        {
            gameState = GameState.puckStopped;
        }
    }
}