using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackwallColliderScript : MonoBehaviour
{
    public delegate void OnBackwallHitDetected();
    public event OnBackwallHitDetected onBackwallHitDetected;

    private void OnTriggerEnter(Collider other)
    {
        onBackwallHitDetected();
    }
}
