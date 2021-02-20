using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Com.GitHub.Knose1.Common.UI
{
	[RequireComponent(typeof(Text)), ExecuteInEditMode]
	public class SetTextByGameObjectInformation : MonoBehaviour
	{
		public enum Information
		{
			Name,
			Index
		}

		[SerializeField] Information info;
		[SerializeField, FormerlySerializedAs("gameObject")] GameObject go;
		Text text;

		// Start is called before the first frame update
		void Awake()
		{
			text = GetComponent<Text>();
		}

		// Update is called once per frame
		void Update()
		{
			if (!Application.isPlaying)
				Awake();

			if (!go) return;
			switch (info)
			{
				case Information.Name:
					text.text = go.name;
					break;
				case Information.Index:
					text.text = go.transform.GetSiblingIndex().ToString();
					break;
				default:
					break;
			}
		}
	}
}
