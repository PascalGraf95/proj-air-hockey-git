using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using requestAPI;

public class PuckControllerAPI : MonoBehaviour
{
    private CharacterController controller;
    private httpRequestAPI reqAPI = new httpRequestAPI();

    private void Start()
    {
        //StartCoroutine(reqAPI.GetRequest(httpRequestAPI.puckPositionX));
        //_coroutine = Coroutine();
        //transform.position = new Vector3();
    }

    private void FixedUpdate()
    {
        StartCoroutine(reqAPI.GetRequest(httpRequestAPI.puckPositionX));
        Debug.Log($"PuckAPI value: {reqAPI.getValue()}");
    }
}