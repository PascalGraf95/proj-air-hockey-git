using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using SimpleJSON;

namespace requestAPI
{
	public class httpRequestAPI
	{
        private string valPuckPosX = "0";
        private string valPuckPosZ = "0";
        private string valPuckVelX = "0";
        private string valPuckVelZ = "0";
        private string valPuckAccX = "0";
        private string valPuckAccZ = "0";
        private string valPusherPosX = "0";
        private string valPusherPosZ = "0";
        private string valPusherVelX = "0";
        private string valPusherVelZ = "0";
        private string valDistance = "0";
        private string valFramrate = "0";

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

		private string value = null;

        public IEnumerator GetRequest(string uriParam)
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
						JSONNode data = JSON.Parse(webRequest.downloadHandler.text);
                        saveValue(data[uriParam], uriParam);
						break;
					default:
						Debug.LogError("Undefined error in GetRequest!");
                        saveValue(null, uriParam);
						break;
				}
			}
			yield return null;
		}

		public string getValue(string msgType)
		{
			switch (msgType)
			{
				case puckPositionX:
					return valPuckPosX;
				case puckPositionZ:
					return valPuckPosZ;
				case puckVelocityX:
					return valPuckVelX;
				case puckVelocityZ:
					return valPuckVelZ;
				case puckAccelerationX:
					return valPuckAccX;
				case puckAccelerationZ:
					return valPuckAccZ;
				case pusherPositionX:
					return valPusherPosX;
				case pusherPositionZ:
					return valPusherPosZ;
				case pusherVelocityX:
					return valPusherVelX;
				case pusherVelocityZ:
					return valPusherVelZ;
				case distance:
					return valDistance;
				case framerate:
					return valFramrate;
				default:
					Debug.LogError("Undefined value type in getValue() GetRequest: " + msgType);
					return null;
			}
		}

		private void saveValue(string val, string msgType)
		{
			switch (msgType) 
			{
				case puckPositionX:
					valPuckPosX = val;
					break;
                case puckPositionZ:
					valPuckPosZ = val;
                    break;
                case puckVelocityX:
					valPuckVelX = val;
                    break;
                case puckVelocityZ:
					valPuckVelZ = val;
                    break;
                case puckAccelerationX:
					valPuckAccX = val;
                    break;
                case puckAccelerationZ:
					valPuckAccZ = val;
                    break;
                case pusherPositionX:
					valPusherPosX = val;
                    break;
                case pusherPositionZ:
					valPusherPosZ = val;
                    break;
                case pusherVelocityX:
					valPusherVelX = val;
                    break;
                case pusherVelocityZ:
					valPusherVelZ = val;
                    break;
				case distance:
					valDistance = val;
					break;
				case framerate:
					valFramrate = val;
					break;
                default :
                    Debug.LogError("Undefined value type in saveValue() GetRequest: " + msgType);
                    break;
            }
		}
    }
}