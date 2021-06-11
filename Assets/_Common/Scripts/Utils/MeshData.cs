//#define DEBUG_BASIC_SHAPE_CS

#if DEBUG_BASIC_SHAPE_CS
#warning DEBUG_BASIC_SHAPE_CS
#endif

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Com.GitHub.Knose1.Common.Utils
{
	public struct MeshData
	{
		public const int NUMBER_OF_VERTICE_BY_TRIANGLE = 3;
		public const float THRESHOLD = 0.001f;

		public List<Vector3> vertices;
		public List<int> triangles;

		public MeshData(List<Vector3> vertices, List<int> triangles)
		{
			this.vertices = vertices;
			this.triangles = triangles;
		}

		/// <summary>
		/// Init with default values, returns itself
		/// </summary>
		/// <returns></returns>
		public MeshData Init()
		{
			this.vertices = new List<Vector3>();
			this.triangles = new List<int>();
			return this;
		}

		public int AddDirtyVertice(bool checkForDouble, Vector3 item)
		{
			int index = vertices.IndexOf(item, THRESHOLD);
			if (!checkForDouble || index == -1)
			{
				index = vertices.Count;
				vertices.Add(item);
			}
			return index;
		}

		public void NewShape()
		{
			vertices = new List<Vector3>();
			triangles = new List<int>();
		}

		/*---------------------------*/
		/*-          UTILS          -*/
		/*---------------------------*/

		/// <summary>
		/// Reorder the list of indexes depending on the normal
		/// </summary>
		/// <param name="normal">The expected normal</param>
		/// <param name="vertices">A list composed with 3 vertices</param>
		/// <param name="startIndex">Will create a List<int>(){i,i+1,i+2}</param>
		/// <returns>The reordered indexes</returns>
		public static List<int> GetTriangleFromNormalAndVertices(Vector3 normal, List<Vector3> vertices, int startIndex = 0) => GetTriangleFromNormalAndVertices(normal, vertices, new List<int>(NUMBER_OF_VERTICE_BY_TRIANGLE) { startIndex + 0, startIndex + 1, startIndex + 2 });

		/// <summary>
		/// Reorder the list of indexes depending on the normal
		/// </summary>
		/// <param name="normal">The expected normal</param>
		/// <param name="vertices">A list composed with 3 vertices</param>
		/// <param name="indexes">A list composed with the indexes of the 3 vertices</param>
		/// <returns>The reordered indexes</returns>
		public static List<int> GetTriangleFromNormalAndVertices(Vector3 normal, List<Vector3> vertices, List<int> indexes)
		{
			if (indexes.Count != NUMBER_OF_VERTICE_BY_TRIANGLE && vertices.Count != NUMBER_OF_VERTICE_BY_TRIANGLE) throw new System.Exception($"Can't calculate triangle : {nameof(indexes)}.Length != 3 AND {nameof(vertices)}.Count != 3");
			if (indexes.Count != NUMBER_OF_VERTICE_BY_TRIANGLE) throw new System.Exception($"Can't calculate triangle : {nameof(indexes)}.Count != 3");
			if (vertices.Count != NUMBER_OF_VERTICE_BY_TRIANGLE) throw new System.Exception($"Can't calculate triangle : {nameof(vertices)}.Count != 3");

			Vector3 origine = vertices[0];
			Vector3 originalNormal = new Plane(origine,vertices[1],vertices[2]).normal; //Get the original normal of the triangle

#if DEBUG_BASIC_SHAPE_CS
			Debug.DrawRay(origine, originalNormal.normalized, Color.yellow, 0);
			Debug.DrawRay(Vector3.zero, normal, new Color(0.75f,0.5f,0f), 0);
#endif

			int middleDirection = System.Math.Sign(Vector3.Dot(originalNormal, normal)); //If middleDirection is negative, normal is on the opposite side
			if (middleDirection == 0) middleDirection = 1;
			originalNormal = originalNormal * middleDirection; //Set originalNormal in the same direction as normal

			float f = Vector3.SignedAngle(vertices[1] - origine, vertices[2] - origine, originalNormal); //Get the signed rotation between vertices[1] and vertices[2] (where origine is their (0,0,0)) 
			int sign = (int)Mathf.Sign(f);
			if (sign != -1) sign = 0;

			int[] outIndexes = new int[NUMBER_OF_VERTICE_BY_TRIANGLE]; //The indexes to return

			for (int i = 0; i < NUMBER_OF_VERTICE_BY_TRIANGLE; i++)
			{
				outIndexes[i] = indexes[Mathf.Abs(i + (NUMBER_OF_VERTICE_BY_TRIANGLE - 1) * sign)];
			}
			return outIndexes.ToList();
		}

		public List<int> GenerateTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
		{
			int startIndex = vertices.Count;
			List<Vector3> vert = ListUtils.ToList(a,b,c);
			vertices.AddRange(vert);

			List<int> indexes = GetTriangleFromNormalAndVertices(normal, vert, startIndex);
			triangles.AddRange(indexes);

			return indexes;
		}

		public List<int> GenerateQuad(Vector3 min, Vector2 size, Vector3 xDirector, Vector3 yDirector, Vector3 expectedNormal, bool checkForDouble = true)
		{
			List<int> indexes = new List<int>();
			List<Vector3> toAdd = new List<Vector3>();

#if DEBUG_BASIC_SHAPE_CS
			Debug.DrawRay(min, xDirector * size.x, Color.red);
			Debug.DrawRay(min, yDirector * size.y, Color.green);
			Debug.DrawRay(min, expectedNormal, Color.blue);			
#endif

			for (int x = 0; x <= 1; x++)
			{
				for (int y = 0; y <= 1; y++)
				{
					Vector3 lX = xDirector * (size.x * x);
					Vector3 lY = yDirector * (size.y * y);
					Vector3 item = min + lX +lY;
					toAdd.Add(item);

					int index = AddDirtyVertice(checkForDouble, item);

					indexes.Add(index);
				}

			}

			for (int j = 0; j <= 1; j++)
			{
				List<Vector3> v = new List<Vector3>();
				List<int> t = new List<int>();
				for (int i = 0; i < 3; i++)
				{
					int currentIndex = i + j;

					Vector3 vertice = toAdd[currentIndex];
					int index = indexes[currentIndex];
					v.Add(vertice);
					t.Add(index);
				}

				triangles.AddRange(GetTriangleFromNormalAndVertices(expectedNormal, v, t));
			}

			return indexes;
		}

		public List<int> GenerateCircle(Vector3 normal, int nDivision, float radius, bool hard = false, bool checkForDouble = true)
		{
			List<int> added = new List<int>();
			Quaternion? quat = null;

			if (normal != Vector3.up)
			{
				Quaternion.FromToRotation(Vector3.up, normal);
			}

			//- Construisez le DataMesh d'un cercle(plus precisement un polygone regulier) de rayon radius et nDivision cotes
			//- Le cercle doit etre centre sur le transform
			Vector3 startPos = Vector3.zero;
			int startPosIndex = vertices.Count;
			Vector3 firstCirclePos = default;
			int firstCircleIndex = 0;

			Vector3 lastPos = default;
			int lastPosIndex = 0;

			vertices.Add(startPos);

			added.Add(startPosIndex);

			float cheesAngle = 360f / nDivision;
			for (int i = 0; i <= nDivision; i++)
			{
				Vector3 pos;
				int index;

				if (i < nDivision)
				{
					float angle = i * cheesAngle;
					pos = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle) * radius, 0, Mathf.Cos(Mathf.Deg2Rad * angle) * radius);
					index = AddDirtyVertice(checkForDouble, pos);
				}
				else
				{
					pos = firstCirclePos;
					index = firstCircleIndex;
				}

				if (i > 0)
				{
					if (hard && i > 1)
					{
						startPosIndex = vertices.Count;
						vertices.Add(startPos);

						lastPosIndex = startPosIndex + 1;
						vertices.Add(lastPos);
					}


					List<int> triangle = GetTriangleFromNormalAndVertices(normal,
					vertices: new List<Vector3>()
					{
						startPos,
						lastPos,
						pos
					},

					indexes:  new List<int>()
					{
						startPosIndex,
						lastPosIndex,
						index
					});

					triangles.AddRange(triangle);
				}
				else
				{
					firstCirclePos = pos;
					firstCircleIndex = index;
				}

				lastPos = pos;
				lastPosIndex = index;

				added.Add(index);
			}

			if (quat.HasValue)
			{
				Rotate(quat.Value, startPosIndex);
			}
			return added;
		}

		public List<int> GeneratePlane(Vector3 origine, Vector2 planeSize, Vector2 smalRecSize, Vector3 xDirector, Vector3 yDirector, Vector3 expectedNormal, bool checkForDouble = true)
		{
			List<int> added = new List<int>();
			xDirector = xDirector.normalized;
			yDirector = yDirector.normalized;

			for (float x = 0; x < planeSize.x; x += 1)
				for (float y = 0; y < planeSize.y; y += 1)
				{


					Vector3 min = origine + xDirector * smalRecSize.x * x + yDirector * smalRecSize.y * y;
					Vector3 size = smalRecSize;
					added.AddRange(GenerateQuad(min, smalRecSize, xDirector, yDirector, expectedNormal, checkForDouble));
				}

			return added;
		}

		public List<int> GenerateCube(Vector3 origine, Vector3 quadSize, Vector3Int divisions, Vector3 xDirector, Vector3 yDirector, Vector3 zDirector, bool checkForDouble = true)
		{
			List<int> added = new List<int>();

			xDirector = xDirector.normalized;
			yDirector = yDirector.normalized;
			zDirector = zDirector.normalized;

			//The diagonal point of the cube
			Vector3 oppositeOrigine =
				origine +
				xDirector * (divisions.x * quadSize.x) +
				yDirector * (divisions.y * quadSize.y) +
				zDirector * (divisions.z * quadSize.z);

			added.AddRange(GeneratePlane(origine, new Vector2(divisions.x, divisions.y), new Vector2(quadSize.x, quadSize.y), xDirector, yDirector, -zDirector, checkForDouble));
			added.AddRange(GeneratePlane(origine, new Vector2(divisions.y, divisions.z), new Vector2(quadSize.y, quadSize.z), yDirector, zDirector, -xDirector, checkForDouble));
			added.AddRange(GeneratePlane(origine, new Vector2(divisions.z, divisions.x), new Vector2(quadSize.z, quadSize.x), zDirector, xDirector, -yDirector, checkForDouble));

			added.AddRange(GeneratePlane(oppositeOrigine, new Vector2(divisions.x, divisions.y), new Vector2(quadSize.x, quadSize.y), -xDirector, -yDirector, zDirector, checkForDouble));
			added.AddRange(GeneratePlane(oppositeOrigine, new Vector2(divisions.y, divisions.z), new Vector2(quadSize.y, quadSize.z), -yDirector, -zDirector, xDirector, checkForDouble));
			added.AddRange(GeneratePlane(oppositeOrigine, new Vector2(divisions.z, divisions.x), new Vector2(quadSize.z, quadSize.x), -zDirector, -xDirector, yDirector, checkForDouble));

			return added;
		}

		public List<int> GenerateTorus(float innerRadius, float tubeRadius, int nTubeDivision, int nTorusDivision, bool doubleSided, bool innerSide)
		{
			int lastTrianglesCount = triangles.Count;
			List<MeshData> meshes = new List<MeshData>();
#if DEBUG_BASIC_SHAPE_CS
			Vector3 lastTorusLine = default; //Debug
#endif
			float torusCircleRatio = 2 * Mathf.PI / nTorusDivision;
			float tubeCircleRatio = 2 * Mathf.PI / nTubeDivision;

			List<Vector3> torusCenters = new List<Vector3>();
			List<List<Vector3>> tubeCircles = new List<List<Vector3>>();
			for (int i = 0; i < nTorusDivision; i++)
			{
				//Torus
				float torusAngle = i * torusCircleRatio;
				Vector3 circleCenterNormalized = new Vector3(Mathf.Sin(torusAngle),0, Mathf.Cos(torusAngle));
				Vector3 circleCenter = circleCenterNormalized * (innerRadius + tubeRadius);

				List<Vector3> tubeCircle = new List<Vector3>();
				tubeCircles.Add(tubeCircle);
				torusCenters.Add(circleCenter);

				//Circle

#if DEBUG_BASIC_SHAPE_CS
				Vector3 lastTubeLine = circleCenter; //Debug
#endif
				for (int j = 0; j < nTubeDivision; j++)
				{
					float circleAngle = j * tubeCircleRatio;
					Vector3 circlePoint = (circleCenterNormalized * Mathf.Sin(circleAngle) + Vector3.up * Mathf.Cos(circleAngle)) * tubeRadius + circleCenter;

					tubeCircle.Add(circlePoint);

#if DEBUG_BASIC_SHAPE_CS
					Debug.DrawLine(lastTubeLine, circlePoint, Color.black); //Debug
					lastTubeLine = circlePoint; //Debug
#endif

				}

#if DEBUG_BASIC_SHAPE_CS
				Debug.DrawLine(lastTorusLine, circleCenter); //Debug
				lastTorusLine = circleCenter; //Debug
#endif
			}

			for (int i = 0; i < nTorusDivision; i++)
			{
				int iNext = (i + 1) % nTorusDivision;

				Vector3 torusCenter = torusCenters[i];
				Vector3 nextTorusCenter = torusCenters[iNext];
				List<Vector3> tubeCircle = tubeCircles[i];
				List<Vector3> nextTubeCircle = tubeCircles[iNext];
				for (int j = 0; j < nTubeDivision; j++)
				{
					int jNext = (j + 1) % nTubeDivision;
					Vector3 currentPoint = tubeCircle[j];
					Vector3 nextPoint = tubeCircle[jNext];
					Vector3 currentPointNextTube = nextTubeCircle[j];
					Vector3 nextPointNextTube = nextTubeCircle[jNext];

					Vector3 currentNormal = currentPoint - torusCenter;
					Vector3 nextNormal = nextPointNextTube - nextTorusCenter;

					MeshData data = new MeshData().Init();

					if (doubleSided || !innerSide)
					{
						data.GenerateTriangle(currentPoint, nextPoint, currentPointNextTube, currentNormal);
						data.GenerateTriangle(currentPointNextTube, nextPointNextTube, nextPoint, nextNormal);
					}

					if (doubleSided || innerSide)
					{
						data.GenerateTriangle(currentPoint, nextPoint, currentPointNextTube, -currentNormal);
						data.GenerateTriangle(currentPointNextTube, nextPointNextTube, nextPoint, -nextNormal);
					}
					
					meshes.Add(data);
				}
			}

			this.Merge(enumMerge : meshes);
			List<int> toReturn = new List<int>(triangles);
			toReturn.RemoveRange(0, lastTrianglesCount);
			return toReturn;
		}

		/*----------------------------------------------*/
		/*-          Translate, Rotate, Scale          -*/
		/*----------------------------------------------*/
		public void Translate(Vector3 value, int from = 0, int to = -1)
		{
			if (to == -1) to = vertices.Count - 1;

			//- Deplacez le triangle sur les trois axes
			for (int i = to; i >= from; i--)
			{
				vertices[i] = vertices[i] + value;
			}
		}
		public void Translate(Vector3 value, IEnumerable<int> indexes)
		{
			IEnumerator<int> enumerator = indexes.GetEnumerator();
			while (enumerator.MoveNext())
			{
				vertices[enumerator.Current] = vertices[enumerator.Current] + value;
			}
			enumerator.Dispose();
		}

		public void Scale(Vector3 value, int from = 0, int to = -1)
		{
			if (to == -1) to = vertices.Count - 1;

			//- Redimensionner le triangle sur les trois axes
			Matrix4x4 scale = Matrix4x4.Scale(value);
			for (int i = to; i >= from; i--)
			{
				//vertices[i] = scale.MultiplyPoint(vertices[i]);

				Vector3 vertice = vertices[i];
				vertices[i] = new Vector3(vertice.x * value.x, vertice.y * value.y, vertice.z * value.z);
			}

		}
		public void Scale(Vector3 value, Vector3 scaleOrigine, int from = 0, int to = -1)
		{
			Translate(-scaleOrigine, from, to);
			Scale(value, from, to);
			Translate(scaleOrigine, from, to);
		}
		public void Scale(Vector3 value, IEnumerable<int> indexes)
		{
			IEnumerator<int> enumerator = indexes.GetEnumerator();

			while (enumerator.MoveNext())
			{
				Vector3 vertice = vertices[enumerator.Current];
				vertices[enumerator.Current] = new Vector3(vertice.x * value.x, vertice.y * value.y, vertice.z * value.z);
			}
			enumerator.Dispose();
		}
		public void Scale(Vector3 value, Vector3 scaleOrigine, IEnumerable<int> indexes)
		{
			Translate(-scaleOrigine, indexes);
			Scale(value, indexes);
			Translate(scaleOrigine, indexes);
		}

		public void Rotate(Quaternion value, int from = 0, int to = -1)
		{
			if (to == -1) to = vertices.Count - 1;

			//- Redimensionner le triangle sur les trois axes
			Matrix4x4 rot = Matrix4x4.Rotate(value);
			for (int i = to; i >= from; i--)
			{
				//vertices[i] = rot.MultiplyPoint(vertices[i]);

				Vector3 vertice = vertices[i];
				vertices[i] = value * vertice;
			}

		}
		public void Rotate(Quaternion value, Vector3 rotationOrigine, int from = 0, int to = -1)
		{
			Translate(-rotationOrigine, from, to);
			Rotate(value, from, to);
			Translate(rotationOrigine, from, to);
		}
		public void Rotate(Quaternion value, IEnumerable<int> indexes)
		{
			IEnumerator<int> enumerator = indexes.GetEnumerator();

			while (enumerator.MoveNext())
			{
				Vector3 vertice = vertices[enumerator.Current];
				vertices[enumerator.Current] = value * vertice;
			}
			enumerator.Dispose();
		}
		public void Rotate(Quaternion value, Vector3 rotationOrigine, IEnumerable<int> indexes)
		{
			Translate(-rotationOrigine, indexes);
			Rotate(value, indexes);
			Translate(rotationOrigine, indexes);
		}

		public void VectorSymetry(Vector3 directorVector, int from = 0, int to = -1)
		{
			Rotate(Quaternion.AngleAxis(180, directorVector), from, to);
		}
		public void VectorSymetry(Vector3 directorVector, Vector3 symetryOrigine, int from = 0, int to = -1)
		{
			Rotate(Quaternion.AngleAxis(180, directorVector), symetryOrigine, from, to);
		}
		public void VectorSymetry(Vector3 directorVector, IEnumerable<int> indexes)
		{
			Rotate(Quaternion.AngleAxis(180, directorVector), indexes);
		}
		public void VectorSymetry(Vector3 directorVector, Vector3 symetryOrigine, IEnumerable<int> indexes)
		{
			Rotate(Quaternion.AngleAxis(180, directorVector), symetryOrigine, indexes);
		}
		/*----------------------------------------------*/
		/*-                   Normal                   -*/
		/*----------------------------------------------*/
		public List<Vector3> GetTriangleNormals()
		{
			List<Vector3> normals = new List<Vector3>();
			int count = triangles.Count;
			for (int i = 0; i < count; i += 3)
			{
				if (count - i < 3) break;
				Vector3 vect_o = vertices[triangles[i]];
				Vector3 vect_i = vertices[triangles[i + 1]];
				Vector3 vect_j = vertices[triangles[i + 2]];
				Vector3 normal = new Plane(vect_o, vect_i, vect_j).normal;
				normals.Add(normal);
			}

			return normals;
		}
		public List<Vector3> GetVerticesNormals()
		{
			List<Vector3> toReturn = new List<Vector3>();
			int count = vertices.Count;
			for (int i = 0; i < count; i++)
			{
				List<Vector3> normals = new List<Vector3>();
				for (int t = 0; t < triangles.Count; t += 3)
				{
					int t_0 = triangles[t];
					int t_1 = triangles[t+1];
					int t_2 = triangles[t+2];
					if (t_0 != i && t_1 != i && t_2 != i) continue;
					
					Vector3 vect_o = vertices[t_0];
					Vector3 vect_i = vertices[t_1];
					Vector3 vect_j = vertices[t_2];
					Vector3 normal = new Plane(vect_o, vect_i, vect_j).normal;
					Debug.DrawRay(vertices[i], normal, Color.blue);
					normals.Add(normal);
				}

				Vector3 n = Vector3Utils.Middle(normals.ToArray()).normalized;
#if DEBUG_BASIC_SHAPE_CS
				Debug.DrawRay(vertices[i], n, Color.green);
#endif
				toReturn.Add(n);
			}

			return toReturn;
		}

		public void SetNormals(Vector3 normal)
		{
			int count = triangles.Count;
			for (int i = 0; i < count; i += 3)
			{
				if (count - i < 3) break;
				SetNormal(normal, i, count);
			}
		}


		public void SetNormal(Vector3 normal, int index) => SetNormal(normal, index, triangles.Count);
		private void SetNormal(Vector3 normal, int index, int count)
		{
			//Set index to a 3 multiple
			index = (index / 3) * 3;

			if (count - index < 3) return;

			int index_1 = index + 1;
			int index_2 = index + 2;

			int i_0 = triangles[index];
			int i_1 = triangles[index_1];
			int i_2 = triangles[index_2];

			Vector3 p_0 = vertices[i_0];
			Vector3 p_1 = vertices[i_1];
			Vector3 p_2 = vertices[i_2];

			List<int> triangle = GetTriangleFromNormalAndVertices(normal, ListUtils.ToList(p_0, p_1, p_2), ListUtils.ToList(i_0, i_1, i_2));

			triangles[index] = triangle[0];
			triangles[index_1] = triangle[1];
			triangles[index_2] = triangle[2];
		}

		/*----------------------------------------------*/
		/*-              Check for Double              -*/
		/*----------------------------------------------*/
		public void CheckForDouble(float threshold) => CheckForDouble(0, vertices.Count, threshold);
		public void CheckForDouble(int verticeIndexMin, int verticeIndexMax, float threshold)
		{
			verticeIndexMax = Mathf.Min(vertices.Count - 1, verticeIndexMax);
			CheckForDouble(vertices.GetRange(verticeIndexMin, verticeIndexMax - verticeIndexMin + 1).Map((_, i) => i), threshold);
		}

		public void CheckForDouble(List<int> verticesIndex, float threshold)
		{
			int verticeCount = vertices.Count;
			for (int i = verticesIndex.Count - 1; i >= 0; i--)
			{
				int verticeIndexI = verticesIndex[i];
				if (verticeIndexI >= 0 && verticeIndexI < verticeCount)
				{
					for (int j = verticesIndex.Count - 1; j >= 0; j--)
					{
						if (i == j) continue;
						
						int verticeIndexJ = verticesIndex[j];
						if (verticeIndexI == verticeIndexJ) 
						{
							verticesIndex.RemoveAt(verticeIndexI);
							break;
						}

					}
				}
				else
				{
					verticesIndex.RemoveAt(verticeIndexI);
				}
			}

			verticesIndex.Sort((a, b) => a - b);

			for (int i = verticesIndex.Count - 1; i >= 0; i--)
			{
				int verticeIndexI = verticesIndex[i];
				for (int j = i; j >= 0; j--)
				{
					int verticeIndexJ = verticesIndex[j];
					if (verticeIndexI == verticeIndexJ) continue;

					if (Vector3Utils.Equals(vertices[verticeIndexI], vertices[verticeIndexJ], threshold) )
					{
						triangles = triangles.Map(LMapTriangles);
						
						int LMapTriangles(int triangle)
						{
							int toReturn = triangle;

							if (triangle == verticeIndexI)
							{
								toReturn = verticeIndexJ;
							}
							else if (triangle > verticeIndexI)
							{
								--toReturn;
							}

							return toReturn;
						}

						vertices.RemoveAt(verticeIndexI);
						verticesIndex.RemoveAt(verticeIndexI);
						break;
					}
				}
			}

			
		}

		public static MeshData Merge(params MeshData[] merge) => Merge(mergeEnum: merge);
		public static MeshData Merge(IEnumerable<MeshData> mergeEnum)
		{
			MeshData lToReturn = new MeshData().Init();
			MeshData[] mergeList = mergeEnum.ToArray();
			int length = mergeList.Length;
			int max = 0;
			for (int i = 0; i < length; i++)
			{
				MeshData currentMesh = mergeList[i];
				List<Vector3> vertices1 = currentMesh.vertices;
				lToReturn.vertices.AddRange(vertices1);
				lToReturn.triangles.AddRange(currentMesh.triangles.Map((v) => v + max));
				max += vertices1.Count;
			}

			return lToReturn;
		}
	}

	public static class MeshDataUtils
	{
		public static void Merge(ref this MeshData t, params MeshData[] merge)
		{
			MeshData mesh = MeshData.Merge(merge.Prepend(t).ToArray());
			t.vertices = mesh.vertices;
			t.triangles = mesh.triangles;
		}
		public static void Merge(ref this MeshData t, IEnumerable<MeshData> enumMerge)
		{
			MeshData mesh = MeshData.Merge(enumMerge.Prepend(t).ToArray());
			t.vertices = mesh.vertices;
			t.triangles = mesh.triangles;
		}
	}
}