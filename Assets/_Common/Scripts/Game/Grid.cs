using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.GitHub.Knose1.Common.Game
{
	public class Grid : MonoBehaviour
	{
		private static readonly Vector3 BASE_GRID_OFFSET = new Vector3(0.5f, 0.5f, 0.5f);

		[Header("Grid")]
		[SerializeField] protected Transform    m_board;
		[SerializeField] protected Transform    m_boardVisual;
		[SerializeField] protected Vector3      m_boardVisualScaleDependency = new Vector3(1, 1, 1);
		[SerializeField] protected Vector3      m_boardVisualScaleOffset = new Vector3(0, 0, 0);
		[SerializeField] protected Vector3      m_boardOffset = new Vector3(0, 0.5f, 0);
		[SerializeField] protected Vector3      m_boardPivot = new Vector2(0.5f,0.5f);
		[SerializeField] protected Vector2      m_size;
		public Vector2 Size
		{
			get => m_size;
			set
			{
				Vector2 oldSize = m_size;

				m_size = value;

				Resize();

				if (oldSize != value && oldSize != Vector2.zero)
				{
					int count = m_elements.Count;
					Vector2 end = (value - oldSize) / 2f;
					for (int i = 0; i < count; i++)
					{
						m_elements[i].transform.position += new Vector3(end.x, 0, end.y);
					}
				}
			}
		}

		[Header("Element")]
		[SerializeField] protected Transform    m_elementContainer;
		[SerializeField] protected Vector2Int   m_elementOffset = new Vector2Int(0, 0);
		[SerializeField] protected Vector2      m_elementPivot = new Vector2(0.5f,0.5f);

		[SerializeField] protected List<GameObject>  m_elements = new List<GameObject>();
		public List<GameObject> Elements => m_elements;
		public int ElementCount => m_elements.Count;

		[Header("Test")]
		[SerializeField] protected Vector3 m_toGridInput;
		[SerializeField] protected Vector2Int m_toGridOutput;
		[SerializeField] protected Vector2Int m_toWorldInput;
		[SerializeField] protected Vector3 m_toWorldOutput;


		private void OnValidate()
		{
			if (!m_board) return;
			Resize();

			if (!Application.isPlaying)
			{
				m_toGridOutput = WorldToGrid(m_toGridInput);
				m_toWorldOutput = GridToWorld(m_toWorldInput);

				Debug.DrawLine(m_toGridInput, new Vector3(m_toGridInput.x, -m_toGridInput.y, m_toGridInput.z), Color.red, 5);
			}
		}

		private void Resize()
		{
			Vector3 size = new Vector3(m_size.x, (m_size.x + m_size.y) / 2f, m_size.y);
			m_board.localScale = size;
			m_board.position = new Vector3(size.x * (m_boardPivot.x - 0.5f) * 2 * BASE_GRID_OFFSET.x,
										   size.y * (m_boardPivot.y - 0.5f) * 2 * BASE_GRID_OFFSET.y,
										   size.z * (m_boardPivot.z - 0.5f) * 2 * BASE_GRID_OFFSET.z
								)
								+ m_boardOffset;

			Vector3 visualSize = new Vector3(lReturnReplacerIfNaN(m_boardVisualScaleOffset.x / size.x, 0),
											 lReturnReplacerIfNaN(m_boardVisualScaleOffset.y / size.y, 0),
											 lReturnReplacerIfNaN(m_boardVisualScaleOffset.z / size.z, 0)
								 ) + m_boardVisualScaleDependency;

			m_boardVisual.localScale = visualSize;
			m_boardVisual.localPosition = new Vector3(0, -visualSize.y / 2, 0);

			float lReturnReplacerIfNaN(float value, float replacer)
			{
				return float.IsNaN(value) ? replacer : value;
			};
		}

		/// <summary>
		/// Place something on the grid without adding it in m_elements
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="pos"></param>
		public void PlaceObj(Transform obj, Vector2Int pos)
		{
			obj.SetParent(m_elementContainer);
			obj.transform.position = GridToWorld(pos);
		}

		/// <summary>
		/// Place something on the grid and add it in m_elements
		/// </summary>
		/// <param name="elm"></param>
		/// <param name="pos"></param>
		public void PlaceElement(GameObject elm, Vector2Int pos)
		{
			m_elements.Add(elm);
			PlaceObj(elm.transform, pos);
		}

		public void RemoveElement(GameObject elm)
		{
			m_elements.Remove(elm);
			elm.transform.SetParent(null);
		}
		
		public void DestroyAllElements()
		{
			for (int i = ElementCount - 1; i >= 0; i--)
			{
				Destroy(m_elements[i]);
			}
			m_elements = new List<GameObject>();
		}

		public void RemoveObj(Transform obj)
		{
			obj.SetParent(null, true);
		}

		public bool IsPosInsideGrid(Vector2Int pos)
		{
			return
				pos.x >= 0 && pos.x < m_size.x
				&&
				pos.y >= 0 && pos.y < m_size.y
			;
		}

		public Vector2Int WorldToGrid(Transform transform)
		{
			return WorldToGrid(transform.position);
		}

		public Vector2Int WorldToGrid(Vector3 pos)
		{
			pos = m_elementContainer.InverseTransformPoint(pos);
			Vector2 vec2Pos = new Vector2(pos.x - m_elementPivot.x + 0.5f, pos.z - m_elementPivot.y + 0.5f);

			return Vector2Int.RoundToInt(vec2Pos) + m_elementOffset;
		}

		public Vector3 GridToWorld(Vector2Int pos)
		{
			return m_elementContainer.TransformPoint(new Vector3(pos.x - m_elementOffset.x + m_elementPivot.x - 0.5f, 0, pos.y - m_elementOffset.y + m_elementPivot.y - 0.5f));
		}

		public GameObject GetElementAt(Vector2Int pos)
		{
			for (int i = m_elements.Count - 1; i >= 0; i--)
			{
				GameObject p = m_elements[i];
				Vector2Int vector2Int = WorldToGrid(p.transform);
				if (vector2Int == pos) return p;
			}

			return null;
		}
	}
}
