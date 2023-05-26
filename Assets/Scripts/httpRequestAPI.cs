using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

public class httpRequestAPI : MonoBehaviour
{
	public const string puckPositionX = "positionX";
    public const string puckPositionZ = "positionX";
    public const string puckVelocityX = "velocityX";
    public const string puckVelocityZ = "velocityZ";
    public const string puckAccelerationX = "accelerationZ";
    public const string puckAccelerationZ = "accelerationZ";

    public const string pusherPositionX = "pusherpositionX";
    public const string pusherPositionZ = "pusherpositionZ";
    public const string pusherVelocityX = "pushervelX";
    public const string pusherVelocityZ = "pushervelX";

    public const string distance = "distance";
    public const string framerate = "framerate";

	private const string address = "127.0.0.1";
	private const string port = "8000";

    public class httpImgData
	{
		public string label { get; set; }

		public int value { get; set; }
	}

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
					// Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
					httpImgData imgData = JsonConvert.DeserializeObject<httpImgData>(webRequest.downloadHandler.text);
					Debug.Log(imgData.value);
					// Do something with the data!
                    break;
			}
		}
	}
}