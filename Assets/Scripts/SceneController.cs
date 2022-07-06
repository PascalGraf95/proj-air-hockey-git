using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;

public class SceneController : MonoBehaviour
{
    [SerializeField] private PuckController puckController;
    [SerializeField] private PusherController pusherAgentController;
    [SerializeField] private PusherController pusherHumanController;
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

    public void ResetScene()
    {
        pusherAgentController.Reset();
        puckController.Reset();
        pusherHumanController.Reset();


        if (mjScene == null)
        {
            mjScene = GameObject.Find("MjScene").GetComponent<MjScene>();
        }
        mjScene.DestroyScene();
        mjScene.CreateScene();
    }
}
