using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Com.GitHub.Knose1.PeerToPeerSocketIo.Server.Test
{
	internal sealed class HostScreen : MonoBehaviour
	{
		[SerializeField] ServerHost host;


		private void Awake()
		{
			host.OnUserJoin += Host_OnUserJoin;
			host.OnUserLeave += Host_OnUserLeave;
		}

		private void Host_OnUserJoin(Player p, ServerHost host) => throw new NotImplementedException();
		private void Host_OnUserLeave(Player p, ServerHost host) => throw new NotImplementedException();

	}
}
