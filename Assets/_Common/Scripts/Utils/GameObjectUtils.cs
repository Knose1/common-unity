using UnityEngine;

namespace Com.Github.Knose1.Common.Utils
{
	public static class GameObjectUtils
	{
		public static void SetCollidersEnabled(GameObject gameObject, bool enabled)
		{
			Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
			foreach (Collider collider in colliders)
			{
				collider.enabled = enabled;
			}
		}
	}
}
