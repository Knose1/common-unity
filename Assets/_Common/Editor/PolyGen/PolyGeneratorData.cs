using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Com.GitHub.Knose1.Common.Utils;
using System.Linq;

namespace Com.GitHub.Knose1.Editor.PolyGenerator
{
	[Serializable]
	public class PolyGeneratorData : ScriptableObject
	{
		private static PolyGeneratorData _dataInstance;
		public static PolyGeneratorData DataInstance { get => _dataInstance != null ? _dataInstance : _dataInstance = LoadInstance(); protected set => _dataInstance = value; }

		private const string PATH = "Assets/Editor";
		private static PolyGeneratorData LoadInstance()
		{
			
			if (!AssetDatabase.IsValidFolder(PATH))
				AssetDatabase.CreateFolder("Assets", "Editor");
			else 
			{
				try
				{
					return Resources.Load<PolyGeneratorData>(PATH + "/" + nameof(PolyGeneratorData) + ".asset");
				}
				catch (Exception)
				{

					throw;
				}
			}
	
			var settings = CreateInstance<PolyGeneratorData>();
			AssetDatabase.CreateAsset(settings, PATH + "/" + nameof(PolyGeneratorData)+".asset");
			AssetDatabase.SaveAssets();
			return settings;
		}

		private void OnEnable()
		{
			if (_dataInstance != null && _dataInstance != this) Destroy(_dataInstance);
			_dataInstance = this;

			hideFlags |= HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			name = nameof(PolyGeneratorData);
		}

		private void OnDestroy()
		{
			if (_dataInstance == this)
			{
				EditorUtility.SetDirty(this);
				AssetDatabase.SaveAssets();
				_dataInstance = null;
			}
		}

		public void UpdatePointCount()
		{
			_pointsCount = points.Count;
		}

		//--------------------------------//
		//           CONSTANTES           //
		//--------------------------------//
		private const string OVERLAP_EXCEPTION = "Polygon is overlapping itself";
		private const int UPSCALE_FOR_TRIANGLE_GENERATION = 100;

		//--------------------------------//
		//        SERIALIZE FIELDS        //
		//--------------------------------//
		[SerializeField, HideInInspector] private int _pointsCount;
		public int PointsCount => _pointsCount;

		[SerializeField, HideInInspector] public PolyToolSceneVisibility guizmos = FlagEnumUtils.ToEnum<PolyToolSceneVisibility>(-1);
		[SerializeField, Mesh] public Mesh mesh = null;
		[SerializeField]       public  List<Vector2> points = new List<Vector2>();
		[SerializeField]       public  List<Triangle> triangles = new List<Triangle>();

		//--------------------------------//
		//            GENERATE            //
		//--------------------------------//
		public void GenerateTriangles()
		{
			double startTime = EditorApplication.timeSinceStartup;

			#region ProgessBar
			bool isProgessCanceled = false;
			void LUpdateProgressBar(string content, float progress)
			{
				isProgessCanceled = EditorUtility.DisplayCancelableProgressBar("Generate Triangles", content, progress);
			}
			#endregion ProgessBar

			LUpdateProgressBar("Check if clockwise", 0);

			//Is the polygone clockwise or counter-Clockwise ?
			float order = 0;
			for (int i = 0; i < DataInstance.PointsCount; i++)
			{
				LUpdateProgressBar("Check if clockwise", 1 / DataInstance.PointsCount);
				Vector2 position = DataInstance.points[i];
				Vector2 nextPosition = DataInstance.points[DataInstance.WrapIndex(i+1)];

				order += (nextPosition.x - position.x) * (nextPosition.y + position.y);
			}

			//Overlap special case
			if (order == 0)
			{
				EditorUtility.ClearProgressBar();
				throw new PolygonGeneratorException(OVERLAP_EXCEPTION);
			}
			order = Mathf.Sign(order);

			//Upscale points for more precision
			var oldPoints = DataInstance.points;
			DataInstance.points = DataInstance.points.Map((v) => v * UPSCALE_FOR_TRIANGLE_GENERATION).ToList();

			//Init lists
			List<Triangle> notGoodTriangles = new List<Triangle>();
			List<Line> inOneTriangle = new List<Line>();
			List<int> cantUse = new List<int>();
			List<int> notAdded;
			DataInstance.triangles = new List<Triangle>();

			//First generation
			string whileLoopProgress = $"Create triangles 0/{DataInstance.PointsCount}";
			string forLoopMessage = "";
			string collisionMessage = "";
			_GenerateLocalTriangles(Enumerable.Range(0, DataInstance.PointsCount).ToList());

			//Other generations
			int previousCount = 0;
			int count = cantUse.Count;
			int maxIterate = 10000;
			while (maxIterate > 0 && inOneTriangle.Count > 0)
			{
				whileLoopProgress = $"Create triangles {count}/{DataInstance.PointsCount}";
				LUpdateProgressBar(whileLoopProgress + ";" + forLoopMessage + ";" + collisionMessage, 0);
				
				if (previousCount == count)
				{
					EditorUtility.ClearProgressBar();
					DataInstance.points = oldPoints;
					Debug.Log($"{count}/{DataInstance.PointsCount} points have their triangles computed");
					throw new PolygonGeneratorException(OVERLAP_EXCEPTION);
				}
				previousCount = count;

				List<int> points = Line.LineListToIndex(inOneTriangle, notAdded);

				_GenerateLocalTriangles(points);
				count = cantUse.Count;

				--maxIterate;

				if (isProgessCanceled)
					break;

				if (count == DataInstance.PointsCount)
					break;
			}

			EditorUtility.ClearProgressBar();


			//GENERATOR FUNCTION //////////////////////////////
			void _GenerateLocalTriangles(List<int> indexes)
			{
				notAdded = new List<int>(indexes);
				//inOneTriangle = new List<Line>();

				int indexesCount = indexes.Count;
				int iterationCollision = Mathf.Max(1, indexesCount - 3);
				int progressDiv = indexesCount * iterationCollision;
				for (int i = 0; i < indexesCount; i++)
				{
					if (isProgessCanceled)
						break;

					//Update progress bar
					forLoopMessage = $"Point {i}/{indexesCount}";
					LUpdateProgressBar(whileLoopProgress + "; " + forLoopMessage + "; " + collisionMessage, ((float)i * iterationCollision) / progressDiv);

					//Get Point indexes from the indexes list
					int i_1 = WrapIndex(i+1, indexesCount);
					int i_2 = WrapIndex(i_1+1, indexesCount);

					//Get indexes
					int current = indexes[i];
					int nextIndex = indexes[i_1];
					int nextNextIndex = indexes[i_2];

					//RULE 1 : Certain points reach their 
					if (cantUse.Contains(current)) continue;
					if (cantUse.Contains(nextIndex)) continue;
					if (cantUse.Contains(nextNextIndex)) continue;

					//Get triangle
					Triangle triangle = new Triangle(current, nextIndex, nextNextIndex);

					//RULE 2 : If a triangle was wrong, it'll be always wrong
					if (notGoodTriangles.Contains(triangle))
						continue;

					//Get position
					Vector2 position = DataInstance.points[current];
					Vector2 nextPosition = DataInstance.points[nextIndex];
					Vector2 nextNextPosition = DataInstance.points[nextNextIndex];

					//Get from to
					Vector2 fromStartToNext = nextPosition - position;
					Vector2 fromNextToNextNext = nextNextPosition - nextPosition;
					Vector2 fromNextNextToStart = position - nextNextPosition;

					//Get Perpendiculars
					Vector2 pFromStartToNext = Vector2.Perpendicular(fromStartToNext);
					Vector2 pFromNextToNextNext = Vector2.Perpendicular(fromNextToNextNext);
					Vector2 pFromNextNextToStart = Vector2.Perpendicular(fromNextNextToStart);

					//RULE 3 : You can only create a triangle if its surface is part of the polygone's surface
					float dot = order*Vector2.Dot(fromStartToNext, -pFromNextNextToStart);
					if (dot < 0)
					{
						notGoodTriangles.Add(triangle);
						continue;
					}

					//RULE 4 : You can only create a triangle if there is no other point inside it
					if (indexesCount > 3)
					{
						bool isCollision = false;
						int loopIndex = 0;
						for (int j = WrapIndex(i_2 + 1, indexesCount); j != i; j = WrapIndex(j + 1, indexesCount))
						{
							//Update progress
							collisionMessage = $"Collision checking {loopIndex}/{iterationCollision}";
							LUpdateProgressBar(whileLoopProgress + "; " + forLoopMessage + "; " + collisionMessage, ((float)i * iterationCollision + loopIndex) / progressDiv);

							//Get point to test collision with
							int collidionIndex = indexes[j];
							Vector2 posCheckCollision = DataInstance.points[collidionIndex];

							Vector2 startToPos = posCheckCollision - position;
							Vector2 nextToPos = posCheckCollision - nextPosition;
							Vector2 nextNextToPos = posCheckCollision - nextNextPosition;

							//If collision
							float dotStart = -order*Vector2.Dot(startToPos, pFromStartToNext);
							float dotNext = -order*Vector2.Dot(nextToPos, pFromNextToNextNext);
							float dotNextNext = -order*Vector2.Dot(nextNextToPos,  pFromNextNextToStart);

							float dotStartSign = Mathf.Sign(dotStart);
							float dotNextSign = Mathf.Sign(dotNext);
							float dotNextNextSign = Mathf.Sign(dotNextNext);

							if (dotStartSign == 1 && dotNextSign == 1 && dotNextNextSign == 1)
							{
								isCollision = true;
								break;
							}

							if (isProgessCanceled)
								break;


							loopIndex += 1;
						}

						LUpdateProgressBar(whileLoopProgress + "; " + forLoopMessage + "; " + collisionMessage, ((float)i * iterationCollision + loopIndex) / progressDiv);

						if (isCollision)
						{
							notGoodTriangles.Add(triangle);
							continue;
						}

						if (isProgessCanceled)
							break;
					}

					// CREATE TRIANGLE //////////////////////////

					//Remove points from "Not Added" list
					if (notAdded.Contains(current)) notAdded.Remove(current);
					if (notAdded.Contains(nextIndex)) notAdded.Remove(nextIndex);
					if (notAdded.Contains(nextNextIndex)) notAdded.Remove(nextNextIndex);

					//These lines are on the border of the shape. Let's remove them if we find them in the "inOneTriangle" list
					Line cantUseLine1 = new Line(current, nextIndex);
					Line cantUseLine2 = new Line(nextIndex, nextNextIndex);
					if (inOneTriangle.Contains(cantUseLine1)) inOneTriangle.Remove(cantUseLine1);
					if (inOneTriangle.Contains(cantUseLine2)) inOneTriangle.Remove(cantUseLine2);


					//Add nextIndex
					cantUse.Add(nextIndex);
					
					//Add current and nextNextIndex
					Line line = new Line(current, nextNextIndex);

					if (inOneTriangle.Contains(line))
					{
						inOneTriangle.Remove(line);
						cantUse.Add(line.p1);
						cantUse.Add(line.p2);
					}
					else inOneTriangle.Add(line);

					//If points are not aligned, declare a triangle
					if (dot != 0)
						DataInstance.triangles.Add(triangle);
				}

			}

			//Descale points
			DataInstance.points = oldPoints;

			Debug.Log("Triangles generated in " + (EditorApplication.timeSinceStartup - startTime).ToString("0.000") + "seconds");
		}

		public void SaveMesh()
		{
			if (!EditorUtility.DisplayDialog("Save mesh ?", "Do you really wanna save the mesh ?\nThis action can't be reverted", "Save the mesh", "Cancel")) return;

			MeshData data = new MeshData(DataInstance.points.Select((Vector2 v) => (Vector3)v).ToList(), Triangle.TriangleListToIndex(DataInstance.triangles));

			data.SetNormals(Vector3.back);

			float minX = DataInstance.points.Min((v) => v.x);
			float maxX = DataInstance.points.Max((v) => v.x);
			float minY = DataInstance.points.Min((v) => v.y);
			float maxY = DataInstance.points.Max((v) => v.y);

			DataInstance.mesh.vertices = data.vertices.ToArray();
			DataInstance.mesh.triangles = data.triangles.ToArray();
			DataInstance.mesh.uv = DataInstance.points.Select((v) => new Vector2(Mathf.InverseLerp(minX, maxX, v.x), Mathf.InverseLerp(minY, maxY, v.y))).ToArray();

			AssetDatabase.SaveAssets();
		}

		//--------------------------------//
		//           UTILITIES            //
		//--------------------------------//
		private static Vector2 Sum(List<Vector2> points)
		{
			Vector2 sum = Vector2.zero;
			foreach (var item in points)
			{
				sum += item;
			}
			return sum;
		}

		public static Vector2 GetCenter(params Vector2[] points) => GetCenter(points.ToList());
		public static Vector2 GetCenter(List<Vector2> points)
		{
			Vector2 sum = Sum(points);
			return sum / points.Count;
		}

		public Vector2 GetCenter(Vector2 remove = default)
		{
			Vector2 sum = Sum(points);
			sum -= remove;
			return sum / _pointsCount;
		}

		public static int WrapIndex(int index, int count)
		{
			int toReturn = index % count;
			return toReturn >= 0 ? toReturn : count + toReturn;
		}

		public int WrapIndex(int index) => WrapIndex(index, _pointsCount);
	}
}
