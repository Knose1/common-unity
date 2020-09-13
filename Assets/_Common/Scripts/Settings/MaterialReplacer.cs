using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Com.GitHub.Knose1.Common.Settings
{
	[CreateAssetMenu(fileName = nameof(MaterialReplacer), menuName = nameof(Common) + "/" + nameof(MaterialReplacer))]
	public class MaterialReplacer : ScriptableObject
	{
		[SerializeField] protected List<Replacer> m_replacers = new List<Replacer>();
		public List<Replacer> Replacers => m_replacers;

		/// <summary>
		/// Each meshRenderer in the gameObject (child included) will see their material replaced by the replacer (see <see cref="m_replacers"/>)
		/// </summary>
		/// <param name="gameObject"></param>
		public void ReplaceMaterial(GameObject gameObject)
		{
			MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer renderer in meshRenderers)
			{
				if (Replacer.ContainsOriginal(m_replacers, renderer.sharedMaterial, out Replacer replacer))
				{
					renderer.material = replacer.replacer;
				}
			}
		}

		/// <summary>
		/// Each meshRenderer in the gameObject (child included) will see their material replaced by the original (see <see cref="m_replacers"/>)
		/// </summary>
		/// <param name="gameObject"></param>
		public void UnreplaceMaterial(GameObject gameObject)
		{
			MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer renderer in meshRenderers)
			{
				if (Replacer.ContainsReplacer(m_replacers, renderer.sharedMaterial, out Replacer replacer))
				{
					renderer.material = replacer.original;
				}
			}
		}

		[Serializable]
		public struct Replacer
		{
			public Material original;
			public Material replacer;

			public static bool ContainsOriginal(List<Replacer> replacers, Material material, out Replacer replacer)
			{
				replacer = default;

				for (int i = replacers.Count - 1; i >= 0; i--)
				{
					if ((replacer = replacers[i]).original == material) return true;
				}

				return false;
			}

			public static bool ContainsReplacer(List<Replacer> replacers, Material material, out Replacer replacer)
			{
				replacer = default;

				for (int i = replacers.Count - 1; i >= 0; i--)
				{
					if ((replacer = replacers[i]).replacer == material) return true;
				}

				return false;
			}
		}
	}
}
