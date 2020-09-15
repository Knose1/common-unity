using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Com.GitHub.Knose1.PeerToPeerSocketIo.Server.Test
{
	internal sealed class HostScreen : MonoBehaviour
	{
		[SerializeField] ServerHost host = null;
		[SerializeField] RectTransform userNameContainer = null;
		[SerializeField] Text textPrefab = null;

		private void Awake()
		{
			host.OnUserJoin += Host_OnUserJoin;
			host.OnUserLeave += Host_OnUserLeave;
		}

		private void Host_OnUserJoin(Player p, ServerHost host)
		{
			
		}

		private void Host_OnUserLeave(Player p, ServerHost host)
		{

		}
	}
}
