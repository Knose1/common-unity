using SocketIO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.GitHub.Knose1.PeerToPeerSocketIo.Server
{
	public class ServerClient : ServerBehaviour
	{
		public delegate void OnMessageDelegate(Message message);
		public event OnMessageDelegate OnMessage;

		private string _username;
		public string Username { get => _username; set => _username = value; }

		private string _room;
		public string Room { get => _room; set => _room = value; }

		protected const string USERNAME = "username";
		protected const string ROOM = "room";
		protected const string CLIENT = "client";


		protected override void SocketConnect(SocketIOEvent obj)
		{
			base.SocketConnect(obj);
			JSONObject connect = new JSONObject();
			connect.AddField(USERNAME, _username);
			connect.AddField(ROOM, _room);
			socket.Emit(CLIENT, connect);
		}

		protected override void SocketMessage(SocketIOEvent obj)
		{
            Debug.Log("Recived (client) : " + obj.data);
			OnMessage?.Invoke(Message.FromObject(obj.data[MESSAGE]));
		}
	}
}