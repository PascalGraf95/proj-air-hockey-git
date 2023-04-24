using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RewardComposition", menuName = "ScriptableObjects/RewardComposition", order = 1)]
public class RewardComposition : ScriptableObject
{
    [Space(5)]
    [Header("Reward Composition")]
    [Range(0f, 10f)]
    public float agentScoredReward;
    [Range(-10f, 0f)]
    public float humanScoredReward;
    [Range(-1f, 0f)]
    public float avoidBoundariesReward;
    [Range(-1f, 0f)]
    public float avoidDirectionChangesReward;
    [Range(0f, 1f)]
    public float encouragePuckMovementReward;
    [Range(-1f, 0f)]
    public float stayInCenterReward;
    [Range(0f, 5f)]
    public float backWallReward;
    public bool endOnBackWall;
    [Range(-1f, 0f)]
    public float puckInAgentsHalfReward;
    [Range(-5f, 0f)]
    public float maxStepReward;
    [Range(-1f, 0f)]
    public float stepReward;
    [Range(-10f, 0f)]
    public float outOfBoundsReward;

}
