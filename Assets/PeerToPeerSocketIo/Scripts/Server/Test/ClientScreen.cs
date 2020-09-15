using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Com.GitHub.Knose1.PeerToPeerSocketIo.Server.Test
{
	internal sealed class ClientScreen : MonoBehaviour
	{
		[SerializeField] ServerClient client = null;
		[SerializeField] Button btnJoin = null;
		[SerializeField] Input inpCode;
		[SerializeField] Input inpUserName;
	}
}
