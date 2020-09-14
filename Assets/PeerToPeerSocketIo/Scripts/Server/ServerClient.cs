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

		public string Username { get => _username; set => _username = value; }
		public string Room { get => _room; set => _room = value; }

		private string _username;
		private string _room;

		protected override void SocketConnect(SocketIOEvent obj)
		{
			base.SocketConnect(obj);
			JSONObject connect = new JSONObject();
			connect.AddField("username", _username);
			connect.AddField("room", _room);
			socket.Emit("client", connect);
		}

		protected override void SocketMessage(SocketIOEvent obj)
		{
            Debug.Log("Recived (client) : " + obj.data);
			OnMessage?.Invoke(Message.FromObject(obj.data[MESSAGE]));
		}
	}
}