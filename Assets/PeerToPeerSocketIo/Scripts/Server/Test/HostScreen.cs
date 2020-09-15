using Com.GitHub.Knose1.Common.UI;
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
		[SerializeField] Text codeTxt = null;
		[SerializeField] BtnWithText userPrefab = null;

		private Dictionary<BtnWithText, Player> playersVisual = new Dictionary<BtnWithText, Player>();

		private void Awake()
		{
			host.OnCode += Host_OnCode; ;
			host.OnUserJoin += Host_OnUserJoin;
			host.OnUserLeave += Host_OnUserLeave;
		}

		private void Host_OnCode(string code)
		{
			codeTxt.text = code;
		}

		private void Host_OnUserJoin(Player p, ServerHost host)
		{
			BtnWithText userUI = Instantiate(userPrefab);
			userUI.Text = p.UserName;
			userUI.OnClick += UserUI_OnClick;
			playersVisual.Add(userUI, p);
			userUI.transform.SetParent(userNameContainer);
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
