using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Com.GitHub.Knose1.PeerToPeerSocketIo.Server
{
	[Serializable]
	public class Player
	{
		[NonSerialized]  protected string _id;
		public string Id => _id;

		[SerializeField] protected string _userName;
		public string UserName => _userName;

		[NonSerialized]  protected bool _hasLeftTheGame = false;
		public bool HasLeftTheGame 
		{
			get => _hasLeftTheGame;
			set => _hasLeftTheGame = value;
		}

		public Player(string id, string userName)
		{
			this._id = id;
			this._userName = userName;
		}

		public static bool operator ==(Player p1, Player p2) => p1._id == p2._id;
		public static bool operator !=(Player p1, Player p2) => p1._id != p2._id;
		public static implicit operator bool(Player p) => p != null && !p._hasLeftTheGame;
		public static implicit operator string(Player p) => p._id;

		public override bool Equals(object obj) => obj is Player player && _id == player._id && Id == player.Id;
		public override int GetHashCode()
		{
			var hashCode = -1496434976;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_id);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
			return hashCode;
		}
	}

	public static class PlayersHelper
	{
		public static int GetPlayerIndexBySocketId(this List<Player> list, string playerId)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i].Id == playerId) return i;
			}
			return -1;
		}

		public static Player GetPlayerBySocketId(this List<Player> list, string playerId)
		{
			int index = list.GetPlayerIndexBySocketId(playerId);
			if (index == -1) return null;

			return list[index];
		}

		public static void RemovePlayerBySocketId(this List<Player> list, string socketId)
		{
			int index = list.GetPlayerIndexBySocketId(socketId);
			if (index == -1)
			{
				Debug.LogWarning("Player \"" + socketId + "\" not in the list");
				return;
			}
			list.RemoveAt(index);
		}
	}
}
