using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PusherConfiguration", menuName = "ScriptableObjects/NewPusherConfiguration", order = 1)]
public class PusherConfiguration : ScriptableObject
{
	public float maxVelocity;
	public float velocityControlFactor;
	public float jointDamping;
	public float mass;
}
