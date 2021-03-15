namespace Com.GitHub.Knose1.Common.Save
{
	public static class SettingsManager
	{
		public const string SETTINGS_FILE_NAME = "settings";

		public static Settings settings;

		/// <summary>
		/// Save the settings to file
		/// </summary>
		/// <param name="category">You shall always have a category</param>
		public static void SaveSettings()
		{
			FileSaver<Settings>.SaveJSON(SETTINGS_FILE_NAME, settings);
		}

		/// <summary>
		/// Load the settings from file
		/// </summary>
		/// <param name="category">You shall always have a category</param>
		public static void LoadSettings()
		{
			settings = FileSaver<Settings>.ReadJSON(SETTINGS_FILE_NAME);
		}
	}

	/// <summary>
	/// Settings to be saved.<br/>
	/// Declare a partial struct <see cref="Com"/>.<see cref="GitHub"/>.<see cref="Knose1"/>.<see cref="Common"/>.<see cref="Save"/>.<see cref="Settings"/> to add fields.
	/// </summary>
	/// <example>
	///	namespace Com.GitHub.Knose1.Common.Save {
	///		public partial struct Settings
	///		{
	///			public int testSettingsValue;
	///		}
	///	}
	/// </example>
	[System.Serializable]
	public partial struct Settings { }
}