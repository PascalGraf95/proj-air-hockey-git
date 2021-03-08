using UnityEngine;

public class GuideRodsScript : MonoBehaviour
{
    public Rigidbody2D rbPusher;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var grPosition = new Vector2(0f, rbPusher.position.y);
        this.transform.position = grPosition;
    }
}
