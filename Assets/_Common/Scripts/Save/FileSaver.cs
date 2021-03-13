using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.GitHub.Knose1.Common.Save
{
	public class FileSaver : MonoBehaviour
	{
		private static FileSaver _instance;

		public static FileSaver Instance
		{
			get
			{
				return _instance = _instance ?? FindObjectOfType<FileSaver>();
			}
		}
	}
}