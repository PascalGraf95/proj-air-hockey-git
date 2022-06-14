using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;

public class SceneController : MonoBehaviour
{
    [SerializeField] private PuckController puckController;
    [SerializeField] private PusherController pusherController;
    private MjScene mjScene;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScene();
        }
    }

    private void ResetScene()
    {
        pusherController.Reset();
        puckController.Reset();

        if (mjScene == null)
        {
            mjScene = GameObject.Find("MjScene").GetComponent<MjScene>();
        }
        mjScene.DestroyScene();
        mjScene.CreateScene();
    }
}
