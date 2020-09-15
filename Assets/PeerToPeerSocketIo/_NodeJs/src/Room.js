const shortid = require('shortid');

/**
 * @property {Array<Room>} list
 */
class Room 
{
	/**
	 * @type {Array<Room>}
	 */
	static list = [];
	/**
	 * 
	 * @param {SocketIO.Socket} host 
	 */
	constructor(host, minCapacity, maxCapacity) {
		//Get a unique room name
		let lRoomId = lGenerate(5);
		function lGenerate(number) 
		{
			return shortid.generate().slice(0, number);
		}

		while (Room.getRoomIndexByName(lRoomId) != -1) lRoomId = lGenerate(5);
		
		/**
		 * The room's name, it's also its code to connect
		 * @type {string}
		 */
		this.name = lRoomId;

		/**
		 * 
		 */
		this.host = host;
		this.minCapacity = minCapacity;
		this.maxCapacity = maxCapacity;
		
		/**
		 * @type {Array<SocketIO.Socket>}
		 */
		this.users = [];

		Room.list.push(this);//Add room to the list

		host.join(lRoomId);
		host.emit("roomIdGenerated", {id:lRoomId, minCapacity: minCapacity, maxCapacity: maxCapacity});
	}

	destroy() 
	{
		//Leave
		this.host.leave(this.name);
	
		//Host just left
		this.host.to(this.name).emit("partyEnd");
	
		Room.list.splice(Room.list.indexOf(this), 1);
	}

	/**
	 * Return true if a user can be added to the room
	 * @returns {boolean}
	 */
	canAdd() 
	{
		return this.users.length <= this.maxCapacity;
	}

	/**
	 * 
	 * @param {string} username 
	 */
	isUsernameInRoom(username) 
	{
		return this.users.find( (v) => v.username == username);
	}

	/**
	 * 
	 * @param {SocketIO.Socket} socket 
	 */
	addUser(socket)
	{
		socket.join(this.name);
		this.users.push(socket);
		socket.to(this.name).emit("userJoin", {username:socket.username, id:socket.id});
	}

	/**
	 * 
	 * @param {SocketIO.Socket} socket 
	 */
	removeUser(socket)
	{
		let lIndex = this.users.indexOf(socket);
		this.users.splice(lIndex, 1);
		//User just left
		socket.to(this.name).emit("userLeave", {username:socket.username, id:socket.id});
	}
	
	/**
	 * @param {string} socketId
	 * @return {SocketIO.Socket} socket 
	 */
	getUserBySocket(socketId) 
	{
		return this.users.find( (s) => s.id == socketId);
	}

	/**
	 * 
	 * @param {string} name 
	 * @returns {Room}
	 */
	static getRoomByName(name)
	{
		let index = Room.getRoomIndexByName(name);
		return index == -1 ? null : Room.list[index];
	}

	/**
	 * 
	 * @param {string} name 
	 * @returns {number}
	 */
	static getRoomIndexByName(name)
	{
		let index = -1;
		for (let i = Room.list.length - 1; i >= 0; i--) {
			let lElement = Room.list[i]
			if (lElement.name == name)
			{
				index = i;
				break;
			}
		}

		return index;
	}

	/**
	 * 
	 * @param {SocketIO.Socket} host
	 * @returns {number}
	 */
	static getRoomIndexBySocket(host)
	{
		let index = -1;
		for (let i = Room.list.length - 1; i >= 0; i--) {
			let lElement = Room.list[i]
			if (lElement.host == host) 
			{
				index = i;
				break;
			}
		}

		return index;
	}

	/**
	 * 
	 * @param {string} name
	 * @returns {SocketIO.Socket}
	 */
	static getRoomHostByName(name)
	{
		/**
		 * @type {SocketIO.Socket}
		 */
		let socket = null;
		for (let i = Room.list.length - 1; i >= 0; i--) {
			let lElement = Room.list[i]
			if (lElement.name == name) 
			{
				socket = lElement.host;
				break;
			}
		}

		return socket;
	}

	/**
	 * 
	 * @param {SocketIO.Socket} socket 
	 */
	static isInRoom(socket) 
	{
		for (let i = Room.list.length - 1; i >= 0; i--) {
			let lRoom = Room.list[i];
			if (lRoom.host == socket || lRoom.users.indexOf(socket) != -1) 
				return true;
		}
		return false;
	}
}

module.exports = Room;