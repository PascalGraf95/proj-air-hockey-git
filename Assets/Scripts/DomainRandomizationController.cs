using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using Assets.Scripts;

public class DomainRandomizationController : MonoBehaviour
{
    public bool applyRandomization = false;
    

    public void ApplyRandomization()
    {
        if (applyRandomization == true)
        {
            // Get all objects with the DomainRandomization script attached.
            DomainRandomization[] domainRandomizations = FindObjectsOfType<DomainRandomization>();
            foreach (DomainRandomization domainRandomization in domainRandomizations)
            {
                var gameObjects = domainRandomization.GameObjectsToRandomize;
                domainRandomization.RandomizeGameObjectTree(gameObjects);
            }
        }        
    }
}