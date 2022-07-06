using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;

public class MeasureVelocity : MonoBehaviour
{
    private float xVel;
    private float lastXPos;
    public MjActuator mjActuatorX;
    // Update is called once per frame
    void FixedUpdate()
    {
        xVel = transform.position.x - lastXPos;
        lastXPos = transform.position.x;

        print("X VEL:" + xVel + ", " + mjActuatorX.Velocity);
    }
}
