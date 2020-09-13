using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.GitHub.Knose1.Common
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshRenderer))]
	public class ScaleMaterialSpriteWithObjectSize : MonoBehaviour
	{
		public enum TransformScale
		{
			Local,
			Lossy
		}

		[SerializeField] protected List<string> m_textures = new List<string>();
		[SerializeField] protected Material m_material = null;
		[SerializeField] protected float m_scale = 1;
		[SerializeField] protected TransformScale m_transformScale;

		protected Material materialClone = null;
		new protected MeshRenderer renderer = null;

		private void OnValidate()
		{
			Start();
		}

		private void Start()
		{
			if (m_material != null)
				materialClone = new Material(m_material);

			renderer = GetComponent<MeshRenderer>();
		}

		private void Update()
		{
			if (materialClone == null) return;

			materialClone.CopyPropertiesFromMaterial(m_material);

			Vector2 transformScale = default;
			switch (m_transformScale)
			{
				case TransformScale.Local:
					transformScale = transform.localScale;
					break;
				case TransformScale.Lossy:
					transformScale = transform.lossyScale;
					break;
			}

			Vector2 scale = new Vector2(transformScale.x * m_scale, transformScale.y * m_scale);
			for (int i = 0; i < m_textures.Count; i++)
			{
				materialClone.SetTextureScale(m_textures[i], scale);
			}

			renderer.material = materialClone;
		}
	}
}