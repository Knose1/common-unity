namespace Com.GitHub.Knose1.Common.Save
{
	public static class SettingsManager
	{
		public const string SETTINGS_FILE_NAME = "settings";

		public static Settings settings;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="category">You shall always have a category</param>
		public static void SaveSettings()
		{
			FileSaver<Settings>.SaveJSON(SETTINGS_FILE_NAME, settings);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="category">You shall always have a category</param>
		public static void LoadSettings()
		{
			settings = FileSaver<Settings>.ReadJSON(SETTINGS_FILE_NAME);
		}
	}

	[System.Serializable]
	public partial struct Settings { }
}