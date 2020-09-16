using Com.GitHub.Knose1.Common.UI;
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
	internal sealed class HostScreen : MonoBehaviour
	{
		[SerializeField] ModalBox modalBoxPrefab = null;
		[SerializeField] ServerHost host = null;
		[SerializeField] RectTransform userNameContainer = null;
		[SerializeField] Text codeTxt = null;
		[SerializeField] BtnWithText userPrefab = null;

		ModalBox currentErrorBox = null;
		ModalBox currentWarnBox = null;

		private Dictionary<BtnWithText, Player> playersVisual = new Dictionary<BtnWithText, Player>();

		private void Awake()
		{
			host.OnCode += Host_OnCode;
			host.OnUserJoin += Host_OnUserJoin;
			host.OnUserLeave += Host_OnUserLeave;
			host.OnSocketError += Host_OnSocketError;
			host.OnSocketDisconnect += Host_OnSocketDisconnect;
		}

		private void OnDestroy()
		{
			host.OnCode -= Host_OnCode;
			host.OnUserJoin -= Host_OnUserJoin;
			host.OnUserLeave -= Host_OnUserLeave;
			host.OnSocketError -= Host_OnSocketError;
			host.OnSocketDisconnect -= Host_OnSocketDisconnect;
		}

		private void Host_OnSocketError(string error)
		{
			if (currentWarnBox) Destroy(currentWarnBox.gameObject);
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

		private void Host_OnSocketDisconnect()
		{
			if (currentErrorBox != null) return;
			currentWarnBox = ModalBox.CreateSimpleAlert(modalBoxPrefab, transform.parent, "You've been disconnected");
			currentWarnBox.Show((b) => {
				Destroy(b.gameObject);
			});
		}

		private void Host_OnCode(string code)
		{
			codeTxt.text = code;
		}

		private void Host_OnUserJoin(Player p, ServerHost host)
		{
			BtnWithText userUI = Instantiate(userPrefab, userNameContainer.transform);
			userUI.Text = p.UserName;
			userUI.OnClick += UserUI_OnClick;
			playersVisual.Add(userUI, p);
		}

		private void UserUI_OnClick(BtnWithText userUI)
		{
			userUI.OnClick -= UserUI_OnClick;
			host.Kick(playersVisual[userUI]);

			DestroyUserUI(userUI);
		}

		private void Host_OnUserLeave(Player p, ServerHost host)
		{
			if (playersVisual.ContainsValue(p))
			{
				DestroyUserUI(playersVisual.First(x => x.Value == p).Key);
			}
		}

		private void DestroyUserUI(BtnWithText btnTxt)
		{
			btnTxt.OnClick -= UserUI_OnClick;
			playersVisual.Remove(btnTxt);
			Destroy(btnTxt.gameObject);
		}
	}
}
