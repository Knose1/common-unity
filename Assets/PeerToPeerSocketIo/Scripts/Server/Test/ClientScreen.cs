using Com.GitHub.Knose1.Common.UI.Utils;
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
		[SerializeField] ModalBox modalBoxPrefab = null;
		[SerializeField] ServerClient client = null;
		[SerializeField] Button btnJoin = null;
		[SerializeField] InputField inpCode = null;
		[SerializeField] InputField inpUserName = null;

		ModalBox currentErrorBox = null;
		ModalBox currentWarnBox = null;

		private void Awake()
		{
			inpCode.onValueChanged.AddListener(InpCode_OnValueChanged);
			inpUserName.onValueChanged.AddListener(InpUserName_OnValueChanged);
			btnJoin.onClick.AddListener(BtnJoin_OnClick);

			client.OnSocketError += Client_OnSocketError;
			client.OnSocketDisconnect += Client_OnSocketDisconnect;
		}

		private void OnDestroy()
		{
			inpCode.onValueChanged.RemoveListener(InpCode_OnValueChanged);
			inpUserName.onValueChanged.RemoveListener(InpUserName_OnValueChanged);
			btnJoin.onClick.RemoveListener(BtnJoin_OnClick);

			client.OnSocketError -= Client_OnSocketError;
			client.OnSocketDisconnect -= Client_OnSocketDisconnect;
		}

		private void Client_OnSocketError(string error)
		{
			if (currentErrorBox)
			{
				currentErrorBox.SetTitle(currentErrorBox.GetTitle() + "\n" + error);
				return;
			}

			currentErrorBox = ModalBox.CreateSimpleAlert(modalBoxPrefab, transform.parent, error, "Error");
			currentErrorBox.Show((b) => {
				Destroy(b.gameObject);
			});
		}

		private void Client_OnSocketDisconnect()
		{
			ModalBox box = ModalBox.CreateSimpleAlert(modalBoxPrefab, transform.parent, "You've been disconnected");
			box.Show((b) => {
				Destroy(b.gameObject);
			});
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
