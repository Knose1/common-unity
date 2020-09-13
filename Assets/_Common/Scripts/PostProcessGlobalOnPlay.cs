using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Com.GitHub.Knose1.Common {
	[RequireComponent(typeof(PostProcessVolume))]
	public class PostProcessGlobalOnPlay : MonoBehaviour {
	
		private void Start () {
			GetComponent<PostProcessVolume>().isGlobal = true;	
		}
	}
}