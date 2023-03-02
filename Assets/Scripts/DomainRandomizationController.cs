using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using Assets.Scripts;

public class DomainRandomizationController : MonoBehaviour
{
    [Tooltip("Randomize the simulation enviromnet. E.g. parameters like friction and damping.")]
    public bool ApplyEnvironmentRandomization = false;
    [Tooltip("Randomize the observations e.g. adds random noise to the perceived puck position.")]
    public bool ApplyObservationRandomization = false;
    [Tooltip("Randomize the actions e.g. adds random delay to the pusher movement.")]
    public bool ApplyActionRandomization = false;


    public void ApplyRandomization()
    {
        if (ApplyEnvironmentRandomization == true)
        {
            // Get all objects with the DomainRandomization script attached.
            DomainRandomizationEnvironment[] domainRandomizations = FindObjectsOfType<DomainRandomizationEnvironment>();
            foreach (DomainRandomizationEnvironment domainRandomization in domainRandomizations)
            {
                var gameObjects = domainRandomization.GameObjectsToRandomize;
                domainRandomization.RandomizeGameObjectTree(gameObjects);
            }
        }        
    }
}