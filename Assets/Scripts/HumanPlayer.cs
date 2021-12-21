using UnityEngine;

public class HumanPlayer : MonoBehaviour
{
    private float maxHumanAcceleration;
    private float maxHumanVelocity;

    HumanBehavior humanBehavior;

    private Rigidbody humanPlayerRB;
    private Rigidbody PuckRB;
    public GameObject targetPositionObject;

    private Vector3 startingPosition;
    private FieldBoundary humanBoundary;
    private Vector3 targetPosition;

    private bool randomSetFlag = false;
    private float offsetXFromTarget;
    private Collider humanPlayerCollider;
    private Collider airHockeyTableCollider;

    public void Init(HumanBehavior humanBehavior, float maxHumanVelocity, float maxHumanAcceleration)
    {
        airHockeyTableCollider = GameObject.Find("AirHockeyTableTop").GetComponent<Collider>();

        this.humanBehavior = humanBehavior;
        this.maxHumanVelocity = maxHumanVelocity;
        this.maxHumanAcceleration = maxHumanAcceleration;
        // Find Puck in Scene
        var puckGameObject = GameObject.Find("Puck");
        PuckRB = puckGameObject.GetComponent<Rigidbody>();
        // Get Rigidbody and Collider
        humanPlayerRB = GetComponent<Rigidbody>();
        humanPlayerCollider = GetComponent<Collider>();
        startingPosition = humanPlayerRB.position;

        humanBoundary = GameObject.Find("HumanBoundaries").GetComponent<FieldBoundary>();
        targetPositionObject = GameObject.Find("TargetPositionObject");
    }

    public void ResetPosition()
    {
        if(humanBehavior == HumanBehavior.None)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(true);
        }

        if (humanBehavior == HumanBehavior.RandomPosition || humanBehavior == HumanBehavior.Heuristic)
        {
            humanPlayerRB.position = new Vector3(Random.Range(humanBoundary.xMin, humanBoundary.xMax), 0f,
                Random.Range(humanBoundary.zMin, humanBoundary.zMax));
        }
        else if (humanBehavior == HumanBehavior.StartingPosition)
        {
            humanPlayerRB.position = startingPosition;
        }
    }


    private void FixedUpdate()
    {
        if (humanBehavior == HumanBehavior.Heuristic)
        {
            if (PuckRB.position.z > 0) // Puck in Agent Half
            {
                if (!randomSetFlag)
                {
                    offsetXFromTarget = Random.Range(humanBoundary.xMin * 0.5f, humanBoundary.xMax * 0.5f);
                    randomSetFlag = true;
                }
                targetPosition = new Vector3(offsetXFromTarget, 0f, startingPosition.z);
            }
            else // Puck in Human Half
            {
                randomSetFlag = false;
                if (PuckRB.position.z < humanPlayerRB.position.z)
                {
                    targetPosition = new Vector3(0, 0f, Mathf.Clamp(PuckRB.position.z - 10f, humanBoundary.zMin,
                        humanBoundary.zMax));
                }
                else
                {
                    targetPosition = new Vector3(Mathf.Clamp(PuckRB.position.x, humanBoundary.xMin,
                                                humanBoundary.xMax), 0f,
                                                Mathf.Clamp(PuckRB.position.z, humanBoundary.zMin,
                                                humanBoundary.zMax));
                }
            }
            targetPositionObject.transform.position = targetPosition;
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                RaycastHit hitData;
                Vector3 mousePosWorld = humanPlayerRB.position;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (airHockeyTableCollider.Raycast(ray, out hitData, 1000f))
                {
                    mousePosWorld = hitData.point;
                }
                targetPosition = new Vector3(Mathf.Clamp(mousePosWorld.x, humanBoundary.xMin, humanBoundary.xMax),
                                            0,
                                            Mathf.Clamp(mousePosWorld.z, humanBoundary.zMin, humanBoundary.zMax));
            }
            else
            {
                targetPosition = humanPlayerRB.position;
            }
        }
        // Apply Force
        humanPlayerRB.AddForce((targetPosition - humanPlayerRB.position).normalized * maxHumanAcceleration * humanPlayerRB.mass * Time.deltaTime);
        // Limit Velocity
        if(humanPlayerRB.velocity.magnitude > maxHumanVelocity)
        {
            humanPlayerRB.velocity = humanPlayerRB.velocity.normalized * maxHumanVelocity;
        }
        // Limit Position
        if(humanPlayerRB.position.x < humanBoundary.xMin)
        {
            humanPlayerRB.velocity = new Vector3(0, 0, humanPlayerRB.velocity.z);
            humanPlayerRB.position = new Vector3(humanBoundary.xMin, 0, humanPlayerRB.position.z);
        }
        else if(humanPlayerRB.position.x > humanBoundary.xMax)
        {
            humanPlayerRB.velocity = new Vector3(0, 0, humanPlayerRB.velocity.z);
            humanPlayerRB.position = new Vector3(humanBoundary.xMax, 0, humanPlayerRB.position.z);
        }
        if (humanPlayerRB.position.z < humanBoundary.zMin)
        {
            humanPlayerRB.velocity = new Vector3(humanPlayerRB.velocity.x, 0, 0);
            humanPlayerRB.position = new Vector3(humanPlayerRB.position.x, 0, humanBoundary.zMin);
        }
        else if (humanPlayerRB.position.z > humanBoundary.zMax)
        {
            humanPlayerRB.velocity = new Vector3(humanPlayerRB.velocity.x, 0, 0);
            humanPlayerRB.position = new Vector3(humanPlayerRB.position.x, 0, humanBoundary.zMax);
        }

    }
}
