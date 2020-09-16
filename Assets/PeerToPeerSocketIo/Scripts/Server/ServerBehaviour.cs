using SocketIO;
using System;
using System.Collections.Generic;
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
			public string Raw => _raw;
			
			[SerializeField] private int _type;
			public int Type => _type;

			private const string RAW = "r";
			private const string TYPE = "t";
			const string PSEUDO_SAFE_STRING_REPLACE_QUOTE = "(%1)";

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
				toReturn.AddField(RAW, EncodePseudoSafeString(msg.Raw));
				toReturn.AddField(TYPE, (int)msg._type);
				return toReturn;
			}
			
			public static Message FromObject(JSONObject obj)
			{
				return new Message(DecodePseudoSafeString(obj.GetField(RAW).str), (int)obj.GetField(TYPE).n);
			}

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
		public event Action<string> OnSocketError;
		public event Action OnSocketDisconnect;

		protected const string SEND_MESSAGE = "sendMessage";
		protected const string MESSAGE = "message";
		protected const string CONNECT = "connect";
		protected const string ERROR = "error";
		protected const string INFO_ERROR = "infoError";
		protected const string DISCONNECT = "disconnect";
		protected const string PARTY_END = "partyEnd";
		protected const string KICK = "kick";
		protected const string ID = "id";
		[SerializeField] protected SocketIOComponent socket;

		public bool Connected => socket.IsConnected;

		//Maybe a List<Action<SocketIOHandler>> for the handelers ?
		private Dictionary<string, Action<SocketIOEvent>> handelers = new Dictionary<string, Action<SocketIOEvent>>();
		private List<SocketIOEvent> toDo = new List<SocketIOEvent>();

		private void Awake()
		{
			lock (handelers)
			{
				handelers = new Dictionary<string, Action<SocketIOEvent>>();
			}
		}

		protected virtual void Start()
		{
			if (!socket) socket = FindObjectOfType<SocketIOComponent>();

			SetHandeler(CONNECT, SocketConnect);
			SetHandeler(ERROR, SocketError);
			SetHandeler(MESSAGE, SocketMessage);
			SetHandeler(INFO_ERROR, SocketInfoError);
			SetHandeler(DISCONNECT, SocketDisconnect);
			SetHandeler(PARTY_END, SocketPartyEnd);
		}

		protected virtual void OnDestroy()
		{
			RemoveHandeler(CONNECT);
			RemoveHandeler(ERROR);
			RemoveHandeler(MESSAGE);
			RemoveHandeler(INFO_ERROR);
			RemoveHandeler(DISCONNECT);
			RemoveHandeler(PARTY_END);
		}

		protected virtual void Update()
		{
			lock (toDo)
			{
				for (int i = toDo.Count - 1; i >= 0; i--)
				{
					SocketIOEvent evt = toDo[i];
					handelers[evt.name]?.Invoke(evt);
				}
				toDo = new List<SocketIOEvent>();
			}
		}

		private void OnAny(SocketIOEvent obj) 
		{
			lock (toDo)
			{
				toDo.Add(obj);
			}
		}

		protected void SetHandeler(string eventName, Action<SocketIOEvent> handeler)
		{
			socket.On(eventName, OnAny);
			handelers.Add(eventName, handeler);
		}

		protected void RemoveHandeler(string eventName)
		{
			socket.Off(eventName, OnAny);
			handelers.Remove(eventName);
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
			Debug.Log("Connect");
			OnSocketOpen?.Invoke();
		}

		private void SocketPartyEnd(SocketIOEvent obj)
		{
			socket.Emit(PARTY_END);
		}

		protected virtual void SocketDisconnect(SocketIOEvent obj)
		{
			Debug.Log("Disconnect");
			OnSocketDisconnect?.Invoke();
		}

		protected virtual void SocketError(SocketIOEvent obj)
		{
			string message = (obj as ErrorSocketIOEvent).error.Message;
			Debug.Log("Error : " + message);
			OnSocketError?.Invoke(message);
		}

		protected void SocketInfoError(SocketIOEvent obj)
		{
			string message = obj.data[MESSAGE].str;
			Debug.Log("Error : " + message);
			OnSocketError?.Invoke(message);
		}

		
	}
}