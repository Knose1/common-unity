using UnityEngine;
using UnityEngine.UI;

namespace Com.GitHub.Knose1.Common.Save.Test
{
	//Unity serialization warning (messy when it comes to struct)
#pragma warning disable CS0649

	[System.Serializable]
	struct TestSave : ISaveData<TestSave>
	{
		private static TestSave GetData() => SaveManager.save.GetData<TestSave>();
		public static int Hello
		{
			get => GetData().hello;
			set
			{
				TestSave testSave = GetData();
				testSave.hello = value;
				SaveManager.save.SetData(testSave);
			}
		}


		private int hello;
	}

	[System.Serializable]
	struct TestSave2 : ISaveData<TestSave2>
	{
		private static TestSave2 GetData() => SaveManager.save.GetData<TestSave2>();
		public static string Hi
		{
			get => GetData().hi;
			set
			{
				TestSave2 testSave = GetData();
				testSave.hi = value;
				SaveManager.save.SetData(testSave);
			}
		}
		private string hi;
	}


//Unity serialization warning (messy when it comes to struct)
#pragma warning restore CS0649

	public class SaveTest : MonoBehaviour
	{
		public int Hello
		{
			set => intInputField.SetTextWithoutNotify(value.ToString());
		}
		
		public string Hi
		{
			set => stringInputField.SetTextWithoutNotify(value);
		}

		public Button saveButton;
		public Button loadButton;
		public InputField intInputField;
		public InputField stringInputField;

		void Start()
		{
			SaveManager.Init();
			SaveManager.Load(0);

			Hello = TestSave.Hello;
			Hi = TestSave2.Hi;

			saveButton.onClick.AddListener(SaveButton_OnClick);
			loadButton.onClick.AddListener(LoadButton_OnClick);
			intInputField.onValueChanged.AddListener(IntField_ValueChanged);
			stringInputField.onValueChanged.AddListener(StringField_ValueChanged);
		}

		private void IntField_ValueChanged(string arg0)
		{
			if (arg0 == string.Empty)
				TestSave.Hello = Hello = 0;
			else
				TestSave.Hello = Hello = int.Parse(arg0);
		}

		private void StringField_ValueChanged(string arg0)
		{
			TestSave2.Hi = Hi = arg0;
		}

		private void SaveButton_OnClick()
		{
			SaveManager.Save(0);
		}

		private void LoadButton_OnClick()
		{
			SaveManager.Load(0);

			Hello = TestSave.Hello;
			Hi = TestSave2.Hi;
		}
	}
}
