using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Mujoco;
using UnityEngine;

public enum ResetPuckState
{
    normalPosition,
    randomPosition,
    randomPositionGlobal,
    randomMiddlePosition
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
    public MjBody PuckBody;
    public MjSlideJoint JointX;
    public MjSlideJoint JointZ;
    public Character Character;

    [HideInInspector]
    public GameState gameState;
    private MjScene _scene;
    private float maxPuckVelocity;
    private bool agentContact;
    public GameObject marker;
    private Transform markerContainer;
    ResetPuckState resetPuckState;
    FieldBoundary puckBoundary;
    FieldBoundary agentBoundary;
    private const float PuckOffsetY = 0.5f;

    public void Init(ResetPuckState resetPuckState, float maxPuckVelocity, FieldBoundary agentBoundary, GameObject puckMarkerPrefab)
    {
        //puckBody = GetComponent<Rigidbody>();
        markerContainer = GameObject.Find("MarkerContainer").transform;
        puckBoundary = GameObject.Find("PuckSpawnBoundaries").GetComponent<FieldBoundary>();
        this.resetPuckState = resetPuckState;
        this.agentBoundary = agentBoundary;
        this.maxPuckVelocity = maxPuckVelocity;
        marker = puckMarkerPrefab;
    }

    public void Reset()
    {
        JointX.Velocity = 0;
        JointZ.Velocity = 0;
        
        PuckBody.transform.position = Vector3.zero;

        if (resetPuckState == ResetPuckState.normalPosition)
        {
            if (gameState == GameState.agentScored || gameState == GameState.backWallReached)
            {
                PuckBody.transform.position = new Vector3(0, PuckOffsetY, -1);
            }
            else if(gameState == GameState.playerScored)
            {
                PuckBody.transform.position = new Vector3(0, PuckOffsetY, 1);
            }
        }
        else if(resetPuckState == ResetPuckState.randomPosition)
        {
            PuckBody.transform.position = new Vector3(Random.Range(agentBoundary.xMin, agentBoundary.xMax) * 0.9f, PuckOffsetY, Random.Range(agentBoundary.zMin, agentBoundary.zMax) * 0.9f);
        }        
        else if(resetPuckState == ResetPuckState.randomMiddlePosition)
        {
            PuckBody.transform.position = new Vector3(Random.Range(puckBoundary.xMin, puckBoundary.xMax) * 0.9f, PuckOffsetY, Random.Range(agentBoundary.zMin, agentBoundary.zMax) * 0.9f);
        }

        agentContact = false;
        gameState = GameState.normal;

        

        if (_scene == null)
        {
            _scene = GameObject.Find("MjScene").GetComponent<MjScene>();
        }
        _scene.DestroyScene();
        _scene.CreateScene();
        
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
        Character.Velocity.x = JointX.Velocity;
        Character.Velocity.z = JointZ.Velocity;
        Character.Position = PuckBody.transform.position;
        
        if (Character.Velocity.magnitude == 0)
        {
            gameState = GameState.puckStopped;
        }
    }


}