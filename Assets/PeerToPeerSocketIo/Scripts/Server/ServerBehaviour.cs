using SocketIO;
using System;
using UnityEngine;

namespace Com.GitHub.Knose1.PeerToPeerSocketIo.Server
{
	public abstract class ServerBehaviour : MonoBehaviour
	{
		public enum MessageType
		{
			Start,		//Start la partie					//Player
			Info,	    //Afficher les infos sur mobile	    //string
			Downgrade,	//Mise à pieds	                    //void
			RevealRank,	//Afficher les infos sur mobile	    //Dictionnary<string, int>
			VotePrompt,	//Il faut voter pour la démocration //Dictionnary<string, string>
			VoteResult	//Un mobile a voté					//Dictionnary<string, int>
		}

		[Serializable]
		public struct Message
		{
			[SerializeField] private string _raw;
			[SerializeField] private MessageType _type;
			public string Raw => _raw;
			public MessageType Type => _type;
			
			public Message(string raw, MessageType type)
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
				return new Message(DecodePseudoSafeString(obj.GetField("r").str), (MessageType)obj.GetField("t").n);
			}

			const string PSEUDO_SAFE_STRING_REPLACE_QUOTE = "(%1)";
			/*const string PSEUDO_SAFE_STRING_REPLACE_OPEN_BRACKET = "(%2)";
			const string PSEUDO_SAFE_STRING_REPLACE_CLOSE_BRACKET = "(%3)";
			const string PSEUDO_SAFE_STRING_REPLACE_COMA = "(%4)";
			const string PSEUDO_SAFE_STRING_REPLACE_DOUBLE_DOT = "(%5)";*/
			private static string EncodePseudoSafeString(string jsonMsg)
			{
				return jsonMsg
					.Replace("\"", PSEUDO_SAFE_STRING_REPLACE_QUOTE);
				/*.Replace("{", PSEUDO_SAFE_STRING_REPLACE_OPEN_BRACKET)
				.Replace("}", PSEUDO_SAFE_STRING_REPLACE_CLOSE_BRACKET)
				.Replace(",", PSEUDO_SAFE_STRING_REPLACE_COMA)
				.Replace(":", PSEUDO_SAFE_STRING_REPLACE_DOUBLE_DOT);*/
			}

			private static string DecodePseudoSafeString(string jsonMsg)
			{
				return jsonMsg
					.Replace(PSEUDO_SAFE_STRING_REPLACE_QUOTE, "\"");
				/*.Replace(PSEUDO_SAFE_STRING_REPLACE_OPEN_BRACKET, "{")
				.Replace(PSEUDO_SAFE_STRING_REPLACE_CLOSE_BRACKET, "}")
				.Replace(PSEUDO_SAFE_STRING_REPLACE_COMA, ",")
				.Replace(PSEUDO_SAFE_STRING_REPLACE_DOUBLE_DOT, ":");*/
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