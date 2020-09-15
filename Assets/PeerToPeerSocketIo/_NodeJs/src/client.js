const Room = require('./Room.js');
const Utils = require('./utils.js');

/**
 * 
 * @param {SocketIO.Server} io 
 * @param {SocketIO.Socket} socket 
 */
exports.init = (io, socket) => 
{
	socket.on('client', function(data){
		
		let log = Utils.dataToString(data);
		Utils.logSocket(socket,'client '+log);

		/**
		 * @typedef JoinData
		 * @property {string} username
		 * @property {string} room
		 */

		/**
		 * @type {JoinData}
		 */
		data;
		//let data = TryParseJson(socket, rawData);

		if (!data)
		{
			console.warn("Data is null or undefined");
			destroy();
			return;
		}
		if (!data.username && !data.room)
		{
			const USERNAME_AND_ROOM_NOT_SPECIFIED_ERROR = {message : "Username and Room not specified"};
			Utils.logSocket(socket,'client '+USERNAME_AND_ROOM_NOT_SPECIFIED_ERROR.message);
		
			socket.emit("infoError", USERNAME_AND_ROOM_NOT_SPECIFIED_ERROR);
			destroy();
			return;
		}
		if (!data.username)
		{
			const USERNAME_NOT_SPECIFIED_ERROR = {message : "Username not specified"};
			Utils.logSocket(socket,'client '+USERNAME_NOT_SPECIFIED_ERROR.message);
			
			socket.emit("infoError", USERNAME_NOT_SPECIFIED_ERROR);
			destroy();
			return;
		}
		if (!data.room)
		{
			const ROOM_NOT_SPECIFIED_ERROR = {message : "Room not specified"};
			Utils.logSocket(socket,'client '+ROOM_NOT_SPECIFIED_ERROR.message);
			
			socket.emit("infoError", ROOM_NOT_SPECIFIED_ERROR);
			destroy();
			return;
		}

		/**
		 * @type {string}
		 */
		var lRoomId = data.room;
		let currentRoom = Room.getRoomByName(lRoomId);
		if (currentRoom == null)
		{
			const ROOM_DONT_EXIST_ERROR = {message : "Room doesn't exist"};
			Utils.logSocket(socket,'client '+ROOM_DONT_EXIST_ERROR.message);
			
			socket.emit("infoError", ROOM_DONT_EXIST_ERROR);
			destroy();
			return;
		}
		
		if (!currentRoom.CanAdd())
		{
			const ROOM_DONT_EXIST_ERROR = {message : "Room doesn't exist"};
			Utils.logSocket(socket,'client '+ROOM_DONT_EXIST_ERROR.message);
			
			socket.emit("infoError", ROOM_DONT_EXIST_ERROR);
			destroy();
			return;
		}

		let userName = socket.nickname = data.username;
		socket.join(lRoomId);

		

		//The client send a message to the host
		socket.on("sendMessage", (rawDat) => 
		{
			let log = "";
			if (typeof(rawDat) == "object") {
				log = JSON.stringify(rawDat);
			} else {
				log = rawDat;
			}

			Utils.logSocket(socket,'client '+'sendMessage '+log);
			
			let host = getRoomHostByName(lRoomId);
			if (host == null) 
			{
				console.warn("Room ["+lRoomId+"] Host socket is null");
				return;
			}
			socket.to(host.emit("message", {id: socket.id, message: rawDat.message}));
		});

		socket.on("disconnect", userLeave);
		socket.on("partyEnd", destroy);


		function userLeave() {
			Utils.logSocket(socket,'client '+'disconnect or partyEnd');
			
			//User just left
			socket.to(lRoomId).emit("userLeave", {username:data.username, id:socket.id});
			
			destroy();
		}

		function destroy() {
			socket.disconnect(true); //true or false ?
		}
		
	});
};