using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Text.RegularExpressions;

namespace Com.IsartDigital.Common
{
	[RequireComponent(typeof(TMP_InputField))]
	public class TMPInputMaxCharacter : MonoBehaviour
	{
		TMP_InputField inp;
		[SerializeField] public int size = 16;
		[SerializeField] public bool trim = true;
		[SerializeField, Tooltip("Leave empty for none")] public string regexCharacterLimitation = "[A-Za-z0-9]+";
		
		private void OnEnable()
		{
			inp = GetComponent<TMP_InputField>();
			inp.onValueChanged.AddListener( OnValueChanged );
		}
		
		private void OnDisable()
		{
			inp.onValueChanged.RemoveListener( OnValueChanged );
		}

		private void OnValueChanged(string arg0)
		{
			if (trim) inp.text = arg0.Trim();
			if (size > 0 && inp.text.Length > size)
			{
				inp.text = inp.text.Substring(0, size);
			}
			if (regexCharacterLimitation.Length > 0)
			{
				Regex reg = new Regex(".*?(?<good>"+regexCharacterLimitation+").*?");
				inp.text = reg.Match(inp.text).Groups["good"].Value;
			}
		}
	}
}