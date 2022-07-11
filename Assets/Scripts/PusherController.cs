using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;

enum ControlMode
{
    Keyboard,
    Click,
    Mouse
}


public class PusherController : MonoBehaviour
{
    [SerializeField] private MjActuator pusherActuatorX;
    [SerializeField] private MjActuator pusherActuatorZ;

    [SerializeField] private MjSlideJoint slideJointX;
    [SerializeField] private MjSlideJoint slideJointZ;

    [SerializeField] private float maxVelocity;
    [SerializeField] private ControlMode controlMode;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float xInput;
        float zInput;
        /*
        switch (controlMode)
        {
            case ControlMode.Keyboard:
                xInput = Input.GetAxis("Horizontal");
                zInput = Input.GetAxis("Vertical");

                pusherActuatorX.Control = xInput * Time.deltaTime * maxVelocity * 100;
                pusherActuatorZ.Control = -zInput * Time.deltaTime * maxVelocity * 100;
                break;
            case ControlMode.Click:
                break;
            case ControlMode.Mouse:
                xInput = Input.GetAxis("Mouse X");
                zInput = Input.GetAxis("Mouse Y");
                print(xInput + " " + zInput);

                pusherActuatorX.Control = xInput * Time.deltaTime * maxVelocity * 100;
                pusherActuatorZ.Control = -zInput * Time.deltaTime * maxVelocity * 100;
                break;
        }
        */

    }

    public void Reset(string pusherType)
    {
        if(pusherType == "Agent")
        {
            transform.position = new Vector3(0, 0, 45.75f);
        }
        else if(pusherType == "Human")
        {
            transform.position = new Vector3(0, 0, -45.75f);
        }
        slideJointX.Configuration = 0f;
        slideJointZ.Configuration = 0f;
        slideJointX.Velocity = 0f;
        slideJointZ.Velocity = 0f;
    }

    public void Act(Vector2 targetVelocity)
    {
        pusherActuatorX.Control = targetVelocity.x * maxVelocity;
        pusherActuatorZ.Control = targetVelocity.y * maxVelocity;
    }

    public Vector2 GetCurrentPosition()
    {
        return new Vector2(transform.position.x, transform.position.z);
    }

    public Vector2 GetCurrentVelocity()
    {
        return new Vector2(pusherActuatorX.Velocity, pusherActuatorZ.Velocity);
    }
}
