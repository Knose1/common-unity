using UnityEngine;

namespace Com.GitHub.Knose1
{
	public abstract class ScriptableBetterEditor : ScriptableObject { }
	public abstract class MonoBetterEditor : MonoBehaviour { }

	[System.Obsolete]
	public abstract class BetterEditor : MonoBetterEditor { }
}
