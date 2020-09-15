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
		[SerializeField] InputField inpCode = null;
		[SerializeField] InputField inpUserName = null;

		private void Awake()
		{
			inpCode.onValueChanged.AddListener(InpCode_OnValueChanged);
			inpUserName.onValueChanged.AddListener(InpUserName_OnValueChanged);
			btnJoin.onClick.AddListener(BtnJoin_OnClick);
		}

		private void InpCode_OnValueChanged(string arg0)
		{
			client.Room = arg0;
		}

		private void InpUserName_OnValueChanged(string arg0)
		{
			client.Username = arg0;
		}

		private void BtnJoin_OnClick()
		{
			client.Connect();
		}
	}
}
