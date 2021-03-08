using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuckScript : MonoBehaviour
{
    public ScoreScript ScoreScriptInstance;
    public float MaxSpeed;
    public bool AgentScored { get { return agentScored; } }
    public bool HumanScored { get { return humanScored; } }

    public bool AgentContact { get { return agentContact; } }
    public Rigidbody2D PuckRB { get { return puckRB; } }

    private Rigidbody2D puckRB;
    private bool agentScored;
    private bool humanScored;
    private bool agentContact;

    void Start()
    {
        puckRB = GetComponent<Rigidbody2D>();
    }

    public void Reset(bool randomPuckPosition, Boundary agentBoundary)
    {
        puckRB.velocity = puckRB.position = new Vector2(0, 0);
        if (randomPuckPosition)
        {
            // Puck velocity and random position
            puckRB.position = new Vector2(Random.Range(agentBoundary.Left, agentBoundary.Right) * 0.9f, Random.Range(agentBoundary.Down, agentBoundary.Up) * 0.9f);
        }
        else
        {
            if (agentScored)
            {
                puckRB.position = new Vector2(0, -1);
            }
            else
            {
                puckRB.position = new Vector2(0, 1);
            }
        }  
        agentScored = humanScored = agentContact = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "AIGoal")
        {
            ScoreScriptInstance.Increment(ScoreScript.Score.PlayerScore);
            humanScored = true;
        }
        else if (other.tag == "PlayerGoal")
        {
            ScoreScriptInstance.Increment(ScoreScript.Score.AIScore);
            agentScored = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
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
        puckRB.velocity = Vector2.ClampMagnitude(puckRB.velocity, MaxSpeed);
    }
}