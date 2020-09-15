using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Com.GitHub.Knose1.Common.UI
{
	public class BtnWithText : MonoBehaviour
	{
		[SerializeField] protected Button _btn = null;
		[SerializeField] protected Text _txt = null;

		public event Action<BtnWithText> OnClick;

		public Button Btn => _btn;
		public Text Txt => _txt;

		public string Text
		{
			get => _txt.text;
			set => _txt.text = value;
		}

	}
}
