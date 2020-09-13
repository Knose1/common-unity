using SocketIO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.GitHub.Knose1.PeerToPeerSocketIo.Server
{
	public class ServerHost : ServerBehaviour
	{
		public delegate void OnMessageDelegate(string playerId, Message message);
		public event OnMessageDelegate OnMessage;

		public delegate void OnUserJoinLeaveDelegate(string username, string playerId);
		public event OnUserJoinLeaveDelegate OnUserJoin;
		public event OnUserJoinLeaveDelegate OnUserLeave;
		
		protected const string SEND_MESSAGE_TO = "sendMessageTo";
		public event Action<string> OnId;

		protected override void Start()
		{
			base.Start();
			socket.On("roomIdGenerated", SocketGeneratedRoomId);
			socket.On("userJoin", SocketUserJoin);
			socket.On("userLeave", SocketUserLeave);
		}

		protected override void SocketConnect(SocketIOEvent obj)
		{
			base.SocketConnect(obj);
			socket.Emit("host");
		}

		/// <summary>
		/// Direct message a player
		/// </summary>
		/// <param name="msg"></param>
		/// <remarks>
		/// You can pass the methode to public if you need it
		/// </remarks>
		public void SendMessageTo(Message msg, string playerId)
		{
			JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
			JSONObject msgObj = Message.ToObject(msg);
			Debug.Log("Send To " + msg);
			json.AddField(MESSAGE, msgObj);
			json.AddField("id", playerId);

			SendMessageTo(json);
		}

		/// <summary>
		/// Direct message a player
		/// </summary>
		/// <param name="obj"></param>
		/// <remarks>
		/// You can pass the methode to public if you need it
		/// </remarks>
		protected void SendMessageTo(JSONObject obj)
		{
			socket.Emit(SEND_MESSAGE_TO, obj);
		}

		private void SocketGeneratedRoomId(SocketIOEvent obj)
		{
			string code = obj.data["id"].str;
			Debug.Log("Code : " + code);
			OnId?.Invoke(code);
		}

		private void SocketUserLeave(SocketIOEvent obj)
		{
			string userName = obj.data["username"].str;
			string id = obj.data["id"].str;
			Debug.Log("UserLeave : "+ userName + "\n" + id);
			OnUserLeave?.Invoke(userName, id);
		}

		private void SocketUserJoin(SocketIOEvent obj)
		{
			string userName = obj.data.GetField("username").str;
			string id = obj.data.GetField("id").str;
			Debug.Log("UserJoin : "+ userName + "\n" + id);
			OnUserJoin?.Invoke(userName, id);
		}

		protected override void SocketMessage(SocketIOEvent obj)
		{
			Debug.Log("Recived (host) : "+obj.data);
			OnMessage?.Invoke(obj.data["id"].str, Message.FromObject(obj.data[MESSAGE]));
		}
	}
}