using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Com.GitHub.Knose1.Common.Utils
{
	abstract public class MeshCreator : MonoBetterEditor
	{
		protected const int NUMBER_OF_VERTICE_BY_TRIANGLE = 3;
		protected const float THRESHOLD = 0.001f;

		[Header("Allow", order=1001), SerializeField] protected bool m_updateCompile = false;
		[SerializeField] protected bool m_updateBuild = false;
		[Header("Debug", order=1000), SerializeField] protected bool m_drawGizmos;

		private bool isUpdate = false;

		protected MeshFilter meshFilter;
		protected MeshCollider meshCollider;

		/// <summary>
		/// The computed vertices
		/// </summary>
		protected List<Vector3> vertices = new List<Vector3>();

		/// <summary>
		/// The dirty vertices (plase use <see cref="DirtyVertices"/>
		/// </summary>
		private List<Vector3> _dirtyVertices = new List<Vector3>();
		/// <summary>
		/// The dirty vertices
		/// </summary>
		protected List<Vector3> DirtyVertices
		{
			get => _dirtyVertices;
			set
			{
				_dirtyVertices = value;
				vertices = new List<Vector3>();
			}
		}

		/// <summary>
		/// The computed triangles
		/// </summary>
		protected List<int> triangles = new List<int>();

		/// <summary>
		/// The dirty triangles (please use <see cref="DirtyTriangles"/>
		/// </summary>
		private List<int> _dirtyTriangles = new List<int>();

		/// <summary>
		/// The dirty triangles
		/// </summary>
		protected List<int> DirtyTriangles
		{
			get => _dirtyTriangles;
			set
			{
				_dirtyTriangles = value;
				triangles = new List<int>();
			}
		}

		protected virtual void Awake()
		{
			meshFilter = GetComponent<MeshFilter>();
			meshCollider = GetComponent<MeshCollider>();
			if (!meshFilter) meshFilter = gameObject.AddComponent<MeshFilter>();
			if (!meshCollider) meshCollider = gameObject.AddComponent<MeshCollider>();
		}

		private void Start()
		{
			isUpdate = false;
		}

		private void Update()
		{
			isUpdate = true;
		}
		protected List<int> ToIntList(params int[] i) => i.ToList();

		/// <summary>
		/// Apply meshData to mesh's vertices and triangles
		/// </summary>
		/// <param name="meshData"></param>
		protected void Compile(MeshData meshData) => Compile(meshData.vertices, meshData.triangles);
		
		/// <summary>
		/// Apply vertices to mesh's vertices
		/// Apply triangles to mesh's triangles
		/// </summary>
		protected void Compile(List<Vector3> vertices, List<int> triangles)
		{
			DirtyVertices = vertices;
			DirtyTriangles = triangles;

			Compile();
		}

		/// <summary>
		/// Apply DirtyVertices to mesh's vertices
		/// Apply DirtyTriangles to mesh's triangles
		/// </summary>
		protected void Compile()
		{
			if (!m_updateCompile && isUpdate) return;

			vertices = DirtyVertices.ToList();
			triangles = DirtyTriangles.ToList();
		}

		/// <summary>
		/// Create new mesh
		/// </summary>
		/// <returns></returns>
		protected Mesh Build()
		{
			if (!m_updateBuild && isUpdate) return meshFilter.mesh;

			Mesh mesh = new Mesh();

			mesh.vertices = vertices.ToArray();
			mesh.triangles = triangles.ToArray();

			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			meshFilter.mesh = mesh;

			meshCollider.sharedMesh = mesh;

			return mesh;
		}

#if UNITY_EDITOR

		protected virtual void OnDrawGizmos()
		{
			if (Application.isPlaying && vertices != null && m_drawGizmos)
			{
				int count = vertices.Count;
				if (count > 30)
					return;

				for (int i = 0; i < count; i++)
				{
					UnityEditor.Handles.Label(vertices[i], i.ToString(), new GUIStyle() { fontSize = 12 });
				}
			}
		}
#endif

		protected int AddDirtyVertice(bool checkForDouble, Vector3 item)
		{
			int index = DirtyVertices.IndexOf(item, THRESHOLD);
			if (!checkForDouble || index == -1)
			{
				index = DirtyVertices.Count;
				DirtyVertices.Add(item);
			}
			return index;
		}

		protected void NewShape()
		{
			DirtyVertices = new List<Vector3>();
			DirtyTriangles = new List<int>();
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
		protected List<int> getTriangleFromNormalAndVertices(Vector3 normal, List<Vector3> vertices, int startIndex = 0) => getTriangleFromNormalAndVertices(normal, vertices, new List<int>(NUMBER_OF_VERTICE_BY_TRIANGLE) { startIndex + 0, startIndex + 1, startIndex + 2 });

		/// <summary>
		/// Reorder the list of indexes depending on the normal
		/// </summary>
		/// <param name="normal">The expected normal</param>
		/// <param name="vertices">A list composed with 3 vertices</param>
		/// <param name="indexes">A list composed with the indexes of the 3 vertices</param>
		/// <returns>The reordered indexes</returns>
		protected List<int> getTriangleFromNormalAndVertices(Vector3 normal, List<Vector3> vertices, List<int> indexes)
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
		
		protected List<int> GenerateQuad(Vector3 min, Vector2 size, Vector3 xDirector, Vector3 yDirector, Vector3 expectedNormal, bool checkForDouble = true)
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

				_dirtyTriangles.AddRange(getTriangleFromNormalAndVertices(expectedNormal, v, t));
			}

			return indexes;
		}

		protected List<int> GenerateCircle(Vector3 normal, int nDivision, float radius, bool hard = false, bool checkForDouble = true)
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
			int startPosIndex = DirtyVertices.Count;
			Vector3 firstCirclePos = default;
			int firstCircleIndex = 0;

			Vector3 lastPos = default;
			int lastPosIndex = 0;

			DirtyVertices.Add(startPos);

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
						startPosIndex = DirtyVertices.Count;
						DirtyVertices.Add(startPos);

						lastPosIndex = startPosIndex + 1;
						DirtyVertices.Add(lastPos);
					}


					List<int> triangle = getTriangleFromNormalAndVertices(normal,
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

					DirtyTriangles.AddRange(triangle);
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

		protected List<int> GeneratePlane(Vector3 origine, Vector2 planeSize, Vector2 smalRecSize, Vector3 xDirector, Vector3 yDirector, Vector3 expectedNormal, bool checkForDouble = true)
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

		protected List<int> GenerateCube(Vector3 origine, Vector3 quadSize, Vector3Int divisions, Vector3 xDirector, Vector3 yDirector, Vector3 zDirector, bool checkForDouble = true)
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


		/*----------------------------------------------*/
		/*-          Translate, Rotate, Scale          -*/
		/*----------------------------------------------*/
		protected void Translate(Vector3 value, int from = 0, int to = -1)
		{
			if (to == -1) to = DirtyVertices.Count - 1;

			//- Deplacez le triangle sur les trois axes
			for (int i = to; i >= from; i--)
			{
				DirtyVertices[i] = DirtyVertices[i] + value;
			}
		}
		protected void Translate(Vector3 value, IEnumerable<int> indexes)
		{
			IEnumerator<int> enumerator = indexes.GetEnumerator();

			while (enumerator.MoveNext())
			{
				DirtyVertices[enumerator.Current] = DirtyVertices[enumerator.Current] + value;
			}
			enumerator.Dispose();
		}
		
		protected void Scale(Vector3 value, int from = 0, int to = -1)
		{
			if (to == -1) to = DirtyVertices.Count - 1;

			//- Redimensionner le triangle sur les trois axes
			Matrix4x4 scale = Matrix4x4.Scale(value);
			for (int i = to; i >= from; i--)
			{
				//vertices[i] = scale.MultiplyPoint(vertices[i]);

				Vector3 vertice = DirtyVertices[i];
				DirtyVertices[i] = new Vector3(vertice.x * value.x, vertice.y * value.y, vertice.z * value.z);
			}

		}
		protected void Scale(Vector3 value, Vector3 scaleOrigine, int from = 0, int to = -1)
		{
			Translate(-scaleOrigine, from, to);
			Scale(value, from, to);
			Translate(scaleOrigine, from, to);
		}
		protected void Scale(Vector3 value, IEnumerable<int> indexes)
		{
			IEnumerator<int> enumerator = indexes.GetEnumerator();

			while (enumerator.MoveNext())
			{
				Vector3 vertice = DirtyVertices[enumerator.Current];
				DirtyVertices[enumerator.Current] = new Vector3(vertice.x * value.x, vertice.y * value.y, vertice.z * value.z);
			}
			enumerator.Dispose();
		}
		protected void Scale(Vector3 value, Vector3 scaleOrigine, IEnumerable<int> indexes)
		{
			Translate(-scaleOrigine, indexes);
			Scale(value, indexes);
			Translate(scaleOrigine, indexes);
		}

		protected void Rotate(Quaternion value, int from = 0, int to = -1)
		{
			if (to == -1) to = DirtyVertices.Count - 1;

			//- Redimensionner le triangle sur les trois axes
			Matrix4x4 rot = Matrix4x4.Rotate(value);
			for (int i = to; i >= from; i--)
			{
				//vertices[i] = rot.MultiplyPoint(vertices[i]);

				Vector3 vertice = DirtyVertices[i];
				DirtyVertices[i] = value * vertice;
			}

		}
		
		protected void Rotate(Quaternion value, Vector3 rotationOrigine, int from = 0, int to = -1)
		{
			Translate(-rotationOrigine, from, to);
			Rotate(value, from, to);
			Translate(rotationOrigine, from, to);
		}

		protected void Rotate(Quaternion value, IEnumerable<int> indexes)
		{
			IEnumerator<int> enumerator = indexes.GetEnumerator();
			
			while (enumerator.MoveNext())
			{
				Vector3 vertice = DirtyVertices[enumerator.Current];
				DirtyVertices[enumerator.Current] = value * vertice;
			}
			enumerator.Dispose();
		}
		protected void Rotate(Quaternion value, Vector3 rotationOrigine, IEnumerable<int> indexes)
		{
			Translate(-rotationOrigine, indexes);
			Rotate(value, indexes);
			Translate(rotationOrigine, indexes);
		}
	}
}