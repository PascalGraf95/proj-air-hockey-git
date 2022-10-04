using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GoalColliderScript : MonoBehaviour
{
    public delegate void OnGoalDetected();
    public event OnGoalDetected onGoalDetected;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Puck")
        {
            onGoalDetected();
            audioSource.Play();
        }
    }
}
