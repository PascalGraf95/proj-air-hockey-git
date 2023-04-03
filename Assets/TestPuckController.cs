using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;

public class TestPuckController : MonoBehaviour
{
    private MjScene mjScene;
    public float startAngle;
    public float startVelocity;
    public Vector2 startPosition;

    private MjSlideJoint mjSlideJointX;
    private MjSlideJoint mjSlideJointZ;
    

    // Start is called before the first frame update
    void Start()
    {
        mjSlideJointX = transform.Find("JointX").GetComponent<MjSlideJoint>();
        mjSlideJointZ = transform.Find("JointZ").GetComponent<MjSlideJoint>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            ResetScene();
        }
    }

    private void ResetScene()
    {
        transform.position = new Vector3(startPosition.x, 1.76f, startPosition.y);
        mjSlideJointX.Velocity = Mathf.Sin(startAngle * Mathf.Deg2Rad) * startVelocity;
        mjSlideJointZ.Velocity = Mathf.Cos(startAngle * Mathf.Deg2Rad) * startVelocity;


        // Mujoco Scene Reset
        if (mjScene == null)
        {
            mjScene = GameObject.Find("MjScene").GetComponent<MjScene>();
        }
        mjScene.DestroyScene();
        mjScene.CreateScene();
    }
}
