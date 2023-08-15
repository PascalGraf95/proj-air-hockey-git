using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using requestAPI;
using System.Globalization;
using Mujoco;
using System;

public class PuckControllerAPI : MonoBehaviour
{
    private GameObject puck;

    private httpRequestAPI reqAPI = new httpRequestAPI();
    private bool simulate_real_puck_enabled = false;

    public void simulate_real_puck()
    {
        if (!simulate_real_puck_enabled)
        {
            DeactivateMujocoObjects();
            simulate_real_puck_enabled = true;
        }
        else
        {
            activateMujocoObjects();
            simulate_real_puck_enabled = false;
        }
    }

    private void DeactivateMujocoObjects()
    {
        puck = GameObject.Find("Puck");

        // disable all mujoco transforms on puck
        puck.GetComponent<MjBody>().enabled = false;

        foreach (Transform t in puck.transform)
        {
            t.gameObject.SetActive(false);
        }
    }

    private void activateMujocoObjects()
    {
        puck = GameObject.Find("Puck");

        // enable all mujoco transforms on puck
        puck.GetComponent<MjBody>().enabled = true;

        foreach (Transform t in puck.transform)
        {
            t.gameObject.SetActive(true);
        }
    }

    private void FixedUpdate()
    {
        if (simulate_real_puck_enabled)
        {
            Vector2 newPosition;

            StartCoroutine(reqAPI.GetRequest(httpRequestAPI.puckPositionX));
            newPosition.x = Convert.ToSingle(reqAPI.getValue(httpRequestAPI.puckPositionX));

            StartCoroutine(reqAPI.GetRequest(httpRequestAPI.puckPositionZ));
            newPosition.y = Convert.ToSingle(reqAPI.getValue(httpRequestAPI.puckPositionZ));

            puck.transform.position = new Vector3(newPosition.x, 0.1f, newPosition.y);
        }
    }
}