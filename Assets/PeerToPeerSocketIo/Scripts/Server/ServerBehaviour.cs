using SocketIO;
using System;
using UnityEngine;

namespace Com.GitHub.Knose1.PeerToPeerSocketIo.Server
{
	public abstract class ServerBehaviour : MonoBehaviour
	{
		[Serializable]
		///<summary>
		/// A message to be
		/// Warning
		///</summary>
		public struct Message
		{
			[SerializeField] private string _raw;
			[SerializeField] private int _type;
			public string Raw => _raw;
			public int Type => _type;

			public Message(object obj, int type) : this(JsonUtility.ToJson(obj), type)
			{
			}
			public Message(string raw, int type)
			{
				this._raw = raw;
				this._type = type;
			}

			public T Parse<T>()
			{
				return JsonUtility.FromJson<T>(_raw);
			}

			public static JSONObject ToObject(Message msg)
			{
				JSONObject toReturn = new JSONObject();
				toReturn.AddField("r", EncodePseudoSafeString(msg.Raw));
				toReturn.AddField("t", (int)msg._type);
				return toReturn;
			}
			
			public static Message FromObject(JSONObject obj)
			{
				return new Message(DecodePseudoSafeString(obj.GetField("r").str), (int)obj.GetField("t").n);
			}

			const string PSEUDO_SAFE_STRING_REPLACE_QUOTE = "(%1)";
			private static string EncodePseudoSafeString(string jsonMsg)
			{
				return jsonMsg
					.Replace("\"", PSEUDO_SAFE_STRING_REPLACE_QUOTE);
			}

			private static string DecodePseudoSafeString(string jsonMsg)
			{
				return jsonMsg
					.Replace(PSEUDO_SAFE_STRING_REPLACE_QUOTE, "\"");
			}
		}

		public event Action OnSocketOpen;
		public event Action OnSocketError;
		public event Action OnSocketDisconnect;

		protected const string SEND_MESSAGE = "sendMessage";
		protected const string MESSAGE = "message";
		private const string CONNECT = "connect";
		private const string ERROR = "error";
		private const string INFO_ERROR = "infoError";
		private const string DISCONNECT = "disconnect";
		private const string PARTY_END = "partyEnd";
		protected const string KICK = "Kick";
		[SerializeField] protected SocketIOComponent socket;

		protected virtual void Start()
		{
			if (!socket) socket = FindObjectOfType<SocketIOComponent>();

			socket.On(CONNECT, SocketConnect);
			socket.On(ERROR, SocketError);
			socket.On(MESSAGE, SocketMessage);
			socket.On(INFO_ERROR, SocketInfoError);
			socket.On(DISCONNECT, SocketDisconnect);
			socket.On(PARTY_END, SocketPartyEnd);
		}


		public void Connect()
		{
			socket.Connect();
		}

		/// <summary>
		/// On the client : Send message to host
		/// On the host : Broadcast message
		/// </summary>
		/// <param name="msg"></param>
		public void SendMessageAll(Message msg)
		{
			JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
			JSONObject msgObj = Message.ToObject(msg);
			Debug.Log("Send "+ msgObj);
			json.AddField(MESSAGE, msgObj);
			SendMessageAll(json);
		}

		/// <summary>
		/// On the client : Send message to host
		/// On the host : Broadcast message
		/// </summary>
		/// <param name="obj"></param>
		/// <remarks>
		/// You can pass the methode to public if you need it
		/// </remarks>
		protected virtual void SendMessageAll(JSONObject obj)
		{
			socket.Emit(SEND_MESSAGE, obj);
		}

		protected abstract void SocketMessage(SocketIOEvent obj);

		protected virtual void SocketConnect(SocketIOEvent obj)
		{
			OnSocketOpen?.Invoke();
			Debug.Log("Connect");
		}

		private void SocketPartyEnd(SocketIOEvent obj)
		{
			socket.Emit(PARTY_END);
		}

		protected virtual void SocketDisconnect(SocketIOEvent obj)
		{
			OnSocketDisconnect?.Invoke();
			Debug.Log("Disconnect");
		}

		protected virtual void SocketError(SocketIOEvent obj)
		{
			OnSocketError?.Invoke();
			Debug.Log("Error : " + (obj as ErrorSocketIOEvent).error.Message);
		}

		protected void SocketInfoError(SocketIOEvent obj)
		{
			OnSocketError?.Invoke();
			Debug.Log("Error : " + obj.data[MESSAGE].str);
		}

		
	}
}