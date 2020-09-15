const Room = require('./Room.js');
const Utils = require('./utils.js');

const CLIENT = 'client';
/**
 * 
 * @param {SocketIO.Server} io 
 * @param {SocketIO.Socket} socket 
 */
exports.init = (io, socket) => 
{
	socket.on(CLIENT, function(initData){
		
		if (Room.isInRoom(socket)) 
		{
			Utils.logSocket(socket, CLIENT+' '+"socket already has a room");
			//bro u're in a room, whut u want from mi
			return;
		}

		let log = Utils.dataToString(initData);
		Utils.logSocket(socket,CLIENT+' '+log);

		/**
		 * @typedef JoinData
		 * @property {string} username
		 * @property {string} room
		 */

		/**
		 * @type {JoinData}
		 */
		initData;

		//CHECKING DATA OBJECT
		if (!initData)
		{
			console.warn("Data is null or undefined");
			destroy();
			return;
		}
		if (!initData.username && !initData.room)
		{
			const USERNAME_AND_ROOM_NOT_SPECIFIED_ERROR = {message : "Username and Room not specified"};
			Utils.logSocket(socket,CLIENT+' '+USERNAME_AND_ROOM_NOT_SPECIFIED_ERROR.message);
		
			socket.emit("infoError", USERNAME_AND_ROOM_NOT_SPECIFIED_ERROR);
			destroy();
			return;
		}
		if (!initData.username)
		{
			const USERNAME_NOT_SPECIFIED_ERROR = {message : "Username not specified"};
			Utils.logSocket(socket,CLIENT+' '+USERNAME_NOT_SPECIFIED_ERROR.message);
			
			socket.emit("infoError", USERNAME_NOT_SPECIFIED_ERROR);
			destroy();
			return;
		}
		if (!initData.room)
		{
			const ROOM_NOT_SPECIFIED_ERROR = {message : "Room not specified"};
			Utils.logSocket(socket,CLIENT+' '+ROOM_NOT_SPECIFIED_ERROR.message);
			
			socket.emit("infoError", ROOM_NOT_SPECIFIED_ERROR);
			destroy();
			return;
		}

		
		//CHECKING SERVER DATAS
		/**
		 * @type {string}
		 */
		var lRoomId = initData.room;
		let lRoom = Room.getRoomByName(lRoomId);
		if (lRoom == null)
		{
			const ROOM_DONT_EXIST_ERROR = {message : "Room doesn't exist"};
			Utils.logSocket(socket,CLIENT+' '+ROOM_DONT_EXIST_ERROR.message);
			
			socket.emit("infoError", ROOM_DONT_EXIST_ERROR);
			destroy();
			return;
		}
		
		let userName = socket.username = initData.username;
		if (lRoom.isUsernameInRoom(userName)) 
		{
			const ROOM_FULL_ERROR = {message : "Username already taken"};
			Utils.logSocket(socket,CLIENT+' '+ROOM_FULL_ERROR.message);
			
			socket.emit("infoError", ROOM_FULL_ERROR);
			destroy();
			return;
		}

		if (!lRoom.canAdd())
		{
			const ROOM_FULL_ERROR = {message : "Room is full"};
			Utils.logSocket(socket,CLIENT+' '+ROOM_FULL_ERROR.message);
			
			socket.emit("infoError", ROOM_FULL_ERROR);
			destroy();
			return;
		}

		lRoom.addUser(socket); //JOIN THE ROOM
		
		//The client send a message to the host
		socket.on("sendMessage", (data) => 
		{
			let log = Utils.dataToString(data);
			Utils.logSocket(socket,CLIENT+' '+'sendMessage '+log);
			
			if (!initData)
			{
				console.warn("Data is null or undefined");
				return;
			}

			let host = getRoomHostByName(lRoomId);
			if (host == null) 
			{
				console.warn("Room ["+lRoomId+"] Host socket is null");
				return;
			}
			socket.to(host.emit("message", {id: socket.id, message: data.message}));
		});

		socket.on("disconnect", userLeave);
		socket.on("partyEnd", destroy);


		function userLeave() {
			Utils.logSocket(socket,CLIENT+' '+'disconnect or partyEnd');
			destroy();
		}

		function destroy() {
			if (lRoom) lRoom.removeUser(socket);
			socket.disconnect(true); //true or false ?
		}
		
	});
};