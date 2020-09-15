using SocketIO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.GitHub.Knose1.PeerToPeerSocketIo.Server
{
	public class ServerHost : ServerBehaviour
	{
		public delegate void OnMessageDelegate(Player p, Message message, ServerHost host);
		public event OnMessageDelegate OnMessage;

		public delegate void OnUserJoinLeaveDelegate(Player p, ServerHost host);
		public event OnUserJoinLeaveDelegate OnUserJoin;
		public event OnUserJoinLeaveDelegate OnUserLeave;
		
		protected const string SEND_MESSAGE_TO = "sendMessageTo";

		/// <summary>
		/// Triggered when the connection code is recived
		/// </summary>
		public event Action<string> OnCode;

		protected string _code = default;
		public string Code => _code;

		protected List<Player> _players = new List<Player>();
		public List<Player> Players => _players;

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
			_players = new List<Player>();
			socket.Emit("host");
		}

		public void Kick(string playerId)
		{
			JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
			json.AddField("id", playerId);

			socket.Emit(KICK, json);
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
			_code = obj.data["id"].str;
			Debug.Log("Code : " + _code);
			OnCode?.Invoke(_code);
		}

		private void SocketUserJoin(SocketIOEvent obj)
		{
			string userName = obj.data.GetField("username").str;
			string id = obj.data.GetField("id").str;
			Debug.Log("UserJoin : "+ userName + "\n" + id);

			Player p = _players.GetPlayerBySocketId(id);
			if (p)

			OnUserJoin?.Invoke(p, this);
		}

		private void SocketUserLeave(SocketIOEvent obj)
		{
			string userName = obj.data["username"].str;
			string id = obj.data["id"].str;
			Debug.Log("UserLeave : " + userName + "\n" + id);

			Player p = _players.GetPlayerBySocketId(id);

			if (p)
			{
				p.HasLeftTheGame = true;
				_players.RemovePlayerBySocketId(p);
			}

			OnUserLeave?.Invoke(p, this);
		}

		protected override void SocketMessage(SocketIOEvent obj)
		{
			Debug.Log("Recived (host) : "+obj.data);
			Player p = _players.GetPlayerBySocketId(obj.data["id"].str);
			OnMessage?.Invoke(p, Message.FromObject(obj.data[MESSAGE]), this);
		}
	}
}