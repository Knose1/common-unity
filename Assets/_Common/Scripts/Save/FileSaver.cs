using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;

using IOFile = System.IO.File;
using System.Text;

namespace Com.GitHub.Knose1.Common.Save
{
	public static class FileSaver
	{
		public const string JSON_PATH = ".json";
		public const string BIN_PATH = ".bin";
		public const string TXT_PATH = ".txt";
		
		//-------------------------------------------------------//
		// OPEN FOLDER                                           //
		//-------------------------------------------------------//
		public static void OpenPersistentDataPath()
		{
			Application.OpenURL(Application.persistentDataPath);
		}

		/// <summary>
		/// Save an object as a TEXT file
		/// </summary>
		/// <param name="fileName">The file name without the extension</param>
		/// <param name="obj">The object to save</param>
		public static void SaveString(string fileName, string obj)
		{
			string path = GetPath(fileName, FileSaver.TXT_PATH);

			FileStream stream = new FileStream(path, FileMode.Create);

			byte[] bytes = Encoding.UTF8.GetBytes(obj);

			stream.Write(bytes, 0, bytes.Length);

			stream.Close();
		}

		/// <summary>
		/// Read an object from a TEXT file
		/// </summary>
		/// <param name="fileName">The file name without the extension</param>
		/// <param name="obj">The object to save</param>
		public static string ReadString(string fileName)
		{
			string path = FileSaver.GetPath(fileName, FileSaver.TXT_PATH);
			if (!IOFile.Exists(path))
			{
				SaveString(fileName, string.Empty);
				return string.Empty;
			}

			FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
			var sr = new StreamReader(stream, Encoding.UTF8);

			string obj = sr.ReadToEnd();
			
			stream.Close();
			sr.Close();

			return obj;
		}

		internal static string GetPath(string fileName, string extension) => Path.Combine(Application.persistentDataPath, fileName + extension);
	}
	
	public static class FileSaver<TSerializable> where TSerializable : new()
	{
		

		//-------------------------------------------------------//
		// FILE SAVEING                                          //
		//-------------------------------------------------------//

		/// <summary>
		/// Save an object as a JSON file
		/// </summary>
		/// <param name="fileName">The file name without the extension</param>
		/// <param name="obj">The object to save</param>
		public static void SaveJSON(string fileName, TSerializable obj)
		{
			string path = FileSaver.GetPath(fileName, FileSaver.JSON_PATH);

			DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(TSerializable));
			FileStream stream = new FileStream(path, FileMode.Create);

			dataContractJsonSerializer.WriteObject(stream, obj);

			stream.Close();
		}

		/// <summary>
		/// Save an object as a BINARY file
		/// </summary>
		/// <param name="fileName">The file name without the extension</param>
		/// <param name="obj">The object to save</param>
		public static void SaveBin(string fileName, TSerializable obj)
		{
			string path = FileSaver.GetPath(fileName, FileSaver.BIN_PATH);

			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(path, FileMode.Create);

			formatter.Serialize(stream, obj);

			stream.Close();
		}

		//-------------------------------------------------------//
		// FILE READING                                          //
		//-------------------------------------------------------//

		/// <summary>
		/// Read an object from a JSON file
		/// </summary>
		/// <param name="fileName">The file name without the extension</param>
		public static TSerializable ReadJSON(string fileName)
		{
			string path = FileSaver.GetPath(fileName, FileSaver.JSON_PATH);
			if (!IOFile.Exists(path))
			{
				TSerializable toReturn = new TSerializable();
				SaveJSON(fileName, toReturn);
				return toReturn;
			}

			DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(TSerializable));
			FileStream stream = new FileStream(path, FileMode.OpenOrCreate);

			TSerializable obj;
			try
			{
				obj = (TSerializable)dataContractJsonSerializer.ReadObject(stream);
			}
			catch (System.Exception e)
			{
				Debug.LogError(e);
				obj = new TSerializable();
			}

			stream.Close();

			return obj;
		}

		/// <summary>
		/// Read an object from a BINARY file
		/// </summary>
		/// <param name="fileName">The file name without the extension</param>
		public static TSerializable ReadBin(string fileName)
		{
			string path = FileSaver.GetPath(fileName, FileSaver.BIN_PATH);
			if (!IOFile.Exists(path))
			{
				TSerializable toReturn = new TSerializable();
				SaveBin(fileName, toReturn);
				return toReturn;
			}

			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
			
			TSerializable obj;
			try
			{
				obj = (TSerializable)formatter.Deserialize(stream);
			}
			catch (System.Exception e)
			{
				Debug.LogError(e);
				obj = new TSerializable();
			}

			stream.Close();

			return obj;
		}
	}
}