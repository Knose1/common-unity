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
		protected const string ROOM_ID_GENERATED = "roomIdGenerated";
		protected const string USER_JOIN = "userJoin";
		protected const string USER_LEAVE = "userLeave";
		protected const string HOST = "host";
		protected const string MIN_CAPACITY = "minCapacity";
		protected const string MAX_CAPACITY = "maxCapacity";

		private const int DEFAULT_MIN_CAPACITY = 1;
		private const int DEFAULT_MAX_CAPACITY = 6;

		/// <summary>
		/// Triggered when the connection code is recived
		/// </summary>
		public event Action<string> OnCode;

		protected string _code = default;
		public string Code => _code;

		protected List<Player> _players = new List<Player>();
		public List<Player> Players => _players;

		[SerializeField] uint minCapacity = DEFAULT_MIN_CAPACITY;
		[SerializeField] uint maxCapacity = DEFAULT_MAX_CAPACITY;

		protected override void OnEnable()
		{
			base.OnEnable();
			SetHandeler(ROOM_ID_GENERATED, SocketGeneratedRoomId);
			SetHandeler(USER_JOIN, SocketUserJoin);
			SetHandeler(USER_LEAVE, SocketUserLeave);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
		}

		protected override void SocketConnect(SocketIOEvent obj)
		{
			base.SocketConnect(obj);
			_players = new List<Player>();

			JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
			json.AddField(MIN_CAPACITY, minCapacity);
			json.AddField(MAX_CAPACITY, maxCapacity);
			socket.Emit(HOST);
		}

		public void Kick(string playerId)
		{
			JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
			json.AddField(ID, playerId);

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
			json.AddField(ID, playerId);

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
			_code = obj.data[ID].str;
			Debug.Log("Code : " + _code);
			OnCode?.Invoke(_code);
		}

		private void SocketUserJoin(SocketIOEvent obj)
		{
			string userName = obj.data.GetField("username").str;
			string id = obj.data.GetField(ID).str;
			Debug.Log("UserJoin : "+ userName + "\n" + id);

			Player p = new Player(id, userName);
			_players.Add(p);
			OnUserJoin?.Invoke(p, this);
		}

		private void SocketUserLeave(SocketIOEvent obj)
		{
			string userName = obj.data["username"].str;
			string id = obj.data[ID].str;
			Debug.Log("UserLeave : " + userName + "\n" + id);

			Player p = _players.GetPlayerBySocketId(id);
			OnUserLeave?.Invoke(p, this);
		}

		protected override void SocketMessage(SocketIOEvent obj)
		{
			Debug.Log("Recived (host) : "+obj.data);
			Player p = _players.GetPlayerBySocketId(obj.data[ID].str);
			OnMessage?.Invoke(p, Message.FromObject(obj.data[MESSAGE]), this);
		}
	}
}