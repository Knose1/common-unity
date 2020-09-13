///-----------------------------------------------------------------
/// Author : Knose1
/// Date : 07/05/2020 15:28
///-----------------------------------------------------------------

using UnityEngine;
using UnityEngine.Networking;

namespace Com.GitHub.Knose1.Common.Server {
	public class JsonPostWebRequest : UnityWebRequest
	{
		public JsonPostWebRequest(string url, string json) : base(url, kHttpVerbPOST)
		{
			downloadHandler = new DownloadHandlerBuffer();
			uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
			uploadHandler.contentType = "application/json";
		}
	}
}