using UnityEngine;

public class HumanPlayer : MonoBehaviour
{
    public bool automaticMovement;
    public float maxMovementSpeed;

    private Rigidbody2D humanPlayerRB;
    private Rigidbody2D PuckRB;

    private Vector2 startingPosition;
    private Boundary playerBoundary;
    private Boundary puckBoundary;
    private Vector2 targetPosition;

    private bool isFirstTimeInOpponentsHalf = true;
    private float offsetXFromTarget;
    private Collider2D humanPlayerCollider;
    bool wasJustClicked = true;
    bool canMove;

    private void Start()
    {
        // Find Puck in Scene
        var puckGameObject = GameObject.Find("Puck");
        PuckRB = puckGameObject.GetComponent<Rigidbody2D>();
        // Find Boundary in Scene
        var playerBoundaryHolder = GameObject.Find("PlayerBoundaryHolder").GetComponent<Transform>();
        var puckBoundaryHolder = GameObject.Find("PlayerPuckBoundaryHolder").GetComponent<Transform>();
        // Get Rigidbody and Collider
        humanPlayerRB = GetComponent<Rigidbody2D>();
        humanPlayerCollider = GetComponent<Collider2D>();
        startingPosition = humanPlayerRB.position;

        playerBoundary = new Boundary(playerBoundaryHolder.GetChild(0).position.y,
            playerBoundaryHolder.GetChild(1).position.y,
            playerBoundaryHolder.GetChild(2).position.x,
            playerBoundaryHolder.GetChild(3).position.x);

        puckBoundary = new Boundary(puckBoundaryHolder.GetChild(0).position.y,
            puckBoundaryHolder.GetChild(1).position.y,
            puckBoundaryHolder.GetChild(2).position.x,
            puckBoundaryHolder.GetChild(3).position.x);
    }

    public void ResetPosition()
    {
        humanPlayerRB.position = startingPosition;
    }


    private void FixedUpdate()
    {
        if(automaticMovement)
        {
                float movementSpeed;
                if (PuckRB.position.y < puckBoundary.Down || PuckRB.position.y > puckBoundary.Up)
                {
                    if (isFirstTimeInOpponentsHalf)
                    {
                        isFirstTimeInOpponentsHalf = false;
                        offsetXFromTarget = Random.Range(-0.1f, 0.1f);
                    }

                    movementSpeed = Random.Range(maxMovementSpeed * 0.5f, maxMovementSpeed);
                    targetPosition = new Vector2(Mathf.Clamp(PuckRB.position.x, -0.5f,
                                                            0.5f),
                                                startingPosition.y);
                }
                else
                {
                    isFirstTimeInOpponentsHalf = true;

                    movementSpeed = Random.Range(maxMovementSpeed * 0.5f, maxMovementSpeed);
                    targetPosition = new Vector2(Mathf.Clamp(PuckRB.position.x, playerBoundary.Left,
                                                playerBoundary.Right),
                                                Mathf.Clamp(PuckRB.position.y, playerBoundary.Down,
                                                playerBoundary.Up));
                }
                humanPlayerRB.MovePosition(Vector2.MoveTowards(humanPlayerRB.position, targetPosition,
                        movementSpeed * Time.fixedDeltaTime));
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                if (wasJustClicked)
                {
                    wasJustClicked = false;

                    if (humanPlayerCollider.OverlapPoint(mousePos))
                    {
                        canMove = true;
                    }
                    else
                    {
                        canMove = false;
                    }
                }

                if (canMove)
                {
                    Vector2 clampedMousePos = new Vector2(Mathf.Clamp(mousePos.x, playerBoundary.Left,
                                                                      playerBoundary.Right),
                                                          Mathf.Clamp(mousePos.y, playerBoundary.Down,
                                                                      playerBoundary.Up));
                    humanPlayerRB.MovePosition(clampedMousePos);
                }
            }
            else
            {
                wasJustClicked = true;
            }
        }
        
    }
}
