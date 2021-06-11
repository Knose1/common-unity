#define TEST_SETTING

using UnityEngine;
using UnityEngine.UI;

namespace Com.GitHub.Knose1.Common.Save
{
	namespace Test
	{
		public class SettingsTest : MonoBehaviour
		{
			private int _value;
			public int Value
			{
				get => _value;
				set => inputField.SetTextWithoutNotify((_value = value).ToString());
			}

			public Button saveButton;
			public Button loadButton;
			public InputField inputField;

			private void Awake()
			{
				SettingsManager.LoadSettings();

				Value = SettingsManager.settings.testSettingsValue;

				saveButton.onClick.AddListener(SaveButton_OnClick);
				loadButton.onClick.AddListener(LoadButton_OnClick);
				inputField.onValueChanged.AddListener(Input_ValueChanged);
			}

			private void Input_ValueChanged(string arg0)
			{
				if (arg0 == string.Empty)
					Value = 0;
				else
					_value = int.Parse(arg0);
			}

			private void SaveButton_OnClick()
			{
				SettingsManager.settings.testSettingsValue = _value;
				SettingsManager.SaveSettings();
			}

			private void LoadButton_OnClick()
			{
				SettingsManager.LoadSettings();
				Value = SettingsManager.settings.testSettingsValue;
			}
		}
	}

	public partial struct Settings
	{
#if TEST_SETTING
#warning Test settings value is enabled
#endif
#if !TEST_SETTING
		[NonSerialized]
#endif
		internal int testSettingsValue;
	}

}


