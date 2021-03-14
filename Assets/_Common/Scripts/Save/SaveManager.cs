using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Com.GitHub.Knose1.Common.Utils;
using UnityEngine;


namespace Com.GitHub.Knose1.Common.Save
{
	/// <summary>
	/// Please use <see cref="ISaveData{TStruct}"/>
	/// </summary>
	public interface ISaveData {};

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TStruct">I'm a struct and it's my type</typeparam>
	public interface ISaveData<TStruct> : ISaveData where TStruct : struct, ISaveData<TStruct> { }
	
	/// <summary>
	/// A class for creating save datas.
	/// You must register a save type before 
	/// </summary>
	/// <example>
	/// SaveManager.Init();
	/// SaveManager.Load(0);
	/// 
	/// TestSave save = SaveManager.save.GetData<TestSave>();
	/// TestSave2 save2 = SaveManager.save.GetData<TestSave2>();
	/// 
	/// Debug.Log(save.hello);
	/// Debug.Log(save2.hi);
	/// 
	/// save.hello = 10;
	/// save2.hi = "hello";
	/// 
	/// SaveManager.save.SetData<TestSave>(save);
	/// SaveManager.save.SetData<TestSave2>(save2);
	/// 
	/// SaveManager.Save(0);
	/// </example>
	public static class SaveManager
	{
		public const string SAVE_FILE_NAME = "save";

		/// <summary>
		/// Save object. Represent the loaded datas from the save.
		/// </summary>
		public static SaveObject save = new SaveObject();

		/// <summary>
		/// Initialise the SaveManager. Gets all the <see cref="ISaveData{TStruct}"/> and mark them as recorded types in the SaveObject.
		/// </summary>
		public static void Init()
		{
			var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(p => p.IsValueType && p.GetInterface(nameof(ISaveData)+"`1") != null);

			Debug.Log(
				//Found {0} save types \n
				// - {Type0}\n
				// - {Type1}\n
				// etc....
				"Found " + types.Count()+ " save types\n"+types.Map( (t) => " - "+t.FullName ).ToJoinString("\n")
			);

			SaveObject.recordedTypes = types.ToList();
		}

		/// <summary>
		/// Save the current save in a certain slot
		/// </summary>
		/// <param name="index"></param>
		public static void Save(int index)
		{
			FileSaver<SaveObject>.SaveBin(SAVE_FILE_NAME+index, save);
		}

		/// <summary>
		/// Load a save and set as the current save
		/// </summary>
		/// <param name="index"></param>
		public static void Load(int index)
		{
			save = FileSaver<SaveObject>.ReadBin(SAVE_FILE_NAME + index);
			save.CheckTypes();
		}

		/// <summary>
		/// Load a save without setting <see cref="save"/>. Usefull to make a show save system
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static SaveObject TempLoad(int index)
		{
			var toReturn = FileSaver<SaveObject>.ReadBin(SAVE_FILE_NAME + index);
			toReturn.CheckTypes();

			return toReturn;
		}


		/// <summary>
		/// The file object that will be serialized
		/// </summary>
		[System.Serializable, KnownType(nameof(GetTypes))]
		public class SaveObject
		{
			/// <summary>
			/// Types recorded in <see cref="saves"/>
			/// </summary>
			internal static List<Type> recordedTypes = new List<Type>();

			/// <summary>
			/// The data saved
			/// </summary>
			protected ISaveData[] saves = new ISaveData[0];

			/// <summary>
			/// Get the types in <see cref="saves"/>
			/// </summary>
			/// <returns></returns>
			private static IEnumerable<Type> GetTypes() => recordedTypes;

			/// <summary>
			/// Check if everytype has an instance in <see cref="saves"/>
			/// </summary>
			internal void CheckTypes()
			{
				foreach (Type types in recordedTypes)
				{
					bool isBreak = false;
					foreach (ISaveData saves in saves)
					{
						Type saveType = saves.GetType();
						Type saveTypeInterface = saveType.GetInterface(nameof(ISaveData)+"`1");
						Type[] genericArgs = saveTypeInterface.GetGenericArguments();

						if (genericArgs.Length > 0)
						{
							if (genericArgs[0].IsEquivalentTo(types))
							{
								isBreak = true;
								break;
							}
						}
					}

					if (isBreak)
						continue;

					object obj = Activator.CreateInstance(types);
					saves = saves.Append((ISaveData)obj).ToArray();
				}
			}

			/// <summary>
			/// Get the structure data
			/// </summary>
			/// <typeparam name="TStruct"></typeparam>
			/// <returns></returns>
			public TStruct GetData<TStruct>() where TStruct : struct, ISaveData<TStruct>
			{
				foreach (var item in saves)
				{
					if (item is ISaveData<TStruct>)
						return (TStruct)item;
				}

				Debug.LogWarning(typeof(TStruct).Name+" not found in "+nameof(SaveObject));

				return default;
			}

			/// <summary>
			/// Set structure datas
			/// </summary>
			/// <typeparam name="TStruct"></typeparam>
			/// <param name="value"></param>
			public void SetData<TStruct>(TStruct value) where TStruct : struct, ISaveData<TStruct>
			{
				int index = GetIndexOfType<TStruct>();

				if (index == -1)
					Debug.LogWarning(typeof(TStruct).Name + " not found in " + nameof(SaveObject));

				saves[index] = value;
			}

			/// <summary>
			/// Get the index where the type is in the array
			/// </summary>
			/// <typeparam name="TStruct"></typeparam>
			/// <returns></returns>
			public int GetIndexOfType<TStruct>() where TStruct : struct, ISaveData<TStruct>
			{
				int index = 0;
				foreach (var item in saves)
				{
					if (item is ISaveData<TStruct>)
						return index;

					++index;
				}

				return -1;
			}
		}
	}
}