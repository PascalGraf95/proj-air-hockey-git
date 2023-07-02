using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
//using Newtonsoft.Json;
using SimpleJSON;

public class httpRequestAPI : MonoBehaviour
{
	public const string puckPositionX = "positionX";
    public const string puckPositionZ = "positionZ";
    public const string puckVelocityX = "velocityX";
    public const string puckVelocityZ = "velocityZ";
    public const string puckAccelerationX = "accelerationX";
    public const string puckAccelerationZ = "accelerationZ";

    public const string pusherPositionX = "pusherpositionX";
    public const string pusherPositionZ = "pusherpositionZ";
    public const string pusherVelocityX = "pushervelX";
    public const string pusherVelocityZ = "pushervelZ";

    public const string distance = "distance";
    public const string framerate = "framerate";

	private const string address = "127.0.0.1";
	private const string port = "8000";

	void Start()
	{
		StartCoroutine(GetRequest(puckPositionX));
	}

	IEnumerator GetRequest(string uriParam)
	{
		string uri = "http://" + address + ":" + port + "/" + uriParam;

		using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
		{
			// Request and wait for the desired page.
			yield return webRequest.SendWebRequest();

			string[] pages = uri.Split('/');
			int page = pages.Length - 1;

			switch (webRequest.result)
			{
				case UnityWebRequest.Result.ConnectionError:
                    Debug.LogError("Connection Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.DataProcessingError:
					Debug.LogError(pages[page] + ": Error: " + webRequest.error);
					break;
				case UnityWebRequest.Result.ProtocolError:
					Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
					break;
				case UnityWebRequest.Result.Success:
					//Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
					JSONNode data = JSON.Parse(webRequest.downloadHandler.text);
					Debug.Log($"Received: {data}");
					Debug.Log($"Value: {data[uriParam]}");
					Debug.Log($"Valuetype: {data[uriParam].GetType()}");
					// Do something with the data! (return)
                    break;
			}
		}
	}
}