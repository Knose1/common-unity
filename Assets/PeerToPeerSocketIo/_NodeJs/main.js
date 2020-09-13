const shortid = require('shortid');
const { json } = require('express');
 
var io = require('socket.io')({
	transports: ['websocket'],
});
io.attach(4567);

/**
 * @type Array<Room>
 */
let rooms = [];

io.on('connection', function(socket){
	/**
	 * 
	 * @param {SocketIO.Socket} socket 
	 * @param {string} log 
	 */
	function logSocket(socket, log) 
	{
		console.log("<"+socket.id+">"+": "+log);
	}

	socket.on('host', function(){
		logSocket(socket,'host');

		if (getRoomIndexBySocket(socket) != -1) 
		{
			logSocket(socket,'host '+"socket already has a room");
			//bro u're in a room, whut u want from mi
			return;
		}

		function generate(number) 
		{
			return shortid.generate().slice(0, number);
		}

		//Get a unique room name
		let lRoom = generate(5);
		while (getRoomIndexByName(lRoom) != -1) lRoom = generate(5);
		
		//Add room in the list
		rooms.push(new Room(lRoom, socket));
		socket.join(lRoom);
		socket.emit("roomIdGenerated", {id:lRoom});

		//The host broadcast a message
		socket.on("sendMessage", (data) => 
		{
			let log = "";
			if (typeof(data) == "object") {
				log = JSON.stringify(data);
			} else {
				log = data;
			}

			logSocket(socket,'host '+'sendMessage '+log);
			

			socket.to(lRoom).emit("message", data);
		});

		socket.on("sendMessageTo", (data) => {
			
			/**
			 * @typedef SendMsgToData
			 * @property {string} id
			 * @property {object} message
			 */

			/**
			 * 
			 * @type {SendMsgToData} 
			 */
			data;

			let log = "";
			if (typeof(data) == "object") {
				log = JSON.stringify(data);
			} else {
				log = data;
			}

			logSocket(socket,'host '+'sendMessageTo '+log);
			
			if (!data)
			{
				console.warn("Data is null or undefined");
				destroy();
				return;
			}
			if (!data.id && !data.message)
			{
				const ID_AND_MESSAGE_NOT_SPECIFIED_ERROR = {message : "Id and message not specified"};
				logSocket(socket,'host '+'sendMessageTo '+ID_AND_MESSAGE_NOT_SPECIFIED_ERROR.message);

				socket.emit("infoError", ID_AND_MESSAGE_NOT_SPECIFIED_ERROR);
				destroy();
				return;
			}
			if (!data.id)
			{
				const ID_NOT_SPECIFIED_ERROR = {message : "Id not specified"};
				logSocket(socket,'host '+'sendMessageTo '+ID_NOT_SPECIFIED_ERROR.message);
				
				socket.emit("infoError", ID_NOT_SPECIFIED_ERROR);
				destroy();
				return;
			}
			if (!data.message)
			{
				const MESSAGE_NOT_SPECIFIED_ERROR = {message : "Message not specified"};
				logSocket(socket,'host '+'sendMessageTo '+MESSAGE_NOT_SPECIFIED_ERROR.message);


				socket.emit("infoError", MESSAGE_NOT_SPECIFIED_ERROR);
				destroy();
				return;
			}

			socket.to(data.id).emit("message", {message:data.message});
		});

		socket.on("partyEnd", destroy);
		socket.on("disconnect", destroy);

		function destroy() {
			logSocket(socket,'host '+'partyEnd or disconnect');

			//Leave
			socket.leave(lRoom);

			//Host just left
			socket.to(lRoom).emit("partyEnd");

			socket.disconnect(true); //true or false ?

			rooms.splice(getRoomIndexByName(lRoom), 1);
		}
	});

	socket.on('client', function(data){
		
		let log = "";
		if (typeof(data) == "object") {
			log = JSON.stringify(data);
		} else {
			log = data;
		}
		logSocket(socket,'client '+log);

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
			logSocket(socket,'client '+USERNAME_AND_ROOM_NOT_SPECIFIED_ERROR.message);
		
			socket.emit("infoError", USERNAME_AND_ROOM_NOT_SPECIFIED_ERROR);
			destroy();
			return;
		}
		if (!data.username)
		{
			const USERNAME_NOT_SPECIFIED_ERROR = {message : "Username not specified"};
			logSocket(socket,'client '+USERNAME_NOT_SPECIFIED_ERROR.message);
			
			socket.emit("infoError", USERNAME_NOT_SPECIFIED_ERROR);
			destroy();
			return;
		}
		if (!data.room)
		{
			const ROOM_NOT_SPECIFIED_ERROR = {message : "Room not specified"};
			logSocket(socket,'client '+ROOM_NOT_SPECIFIED_ERROR.message);
			
			socket.emit("infoError", ROOM_NOT_SPECIFIED_ERROR);
			destroy();
			return;
		}

		/**
		 * @type {string}
		 */
		var lRoom = data.room;

		if (getRoomIndexByName(lRoom) == -1)
		{
			const ROOM_DONT_EXIST_ERROR = {message : "Room doesn't exist"};
			logSocket(socket,'client '+ROOM_DONT_EXIST_ERROR.message);
			
			socket.emit("infoError", ROOM_DONT_EXIST_ERROR);
			destroy();
			return;
		}

		socket.username = data.username;

		socket.to(lRoom).emit("userJoin", {username:data.username, id:socket.id});
		socket.join(lRoom);
		

		//The client send a message to the host
		socket.on("sendMessage", (rawDat) => 
		{
			let log = "";
			if (typeof(rawDat) == "object") {
				log = JSON.stringify(rawDat);
			} else {
				log = rawDat;
			}

			logSocket(socket,'client '+'sendMessage '+log);
			
			let host = getRoomHostByName(lRoom);
			if (host == null) 
			{
				console.warn("Room ["+lRoom+"] Host socket is null");
				return;
			}
			socket.to(host.emit("message", {id: socket.id, message: rawDat.message}));
		});

		socket.on("disconnect", userLeave);
		socket.on("partyEnd", destroy);


		function userLeave() {
			logSocket(socket,'client '+'disconnect or partyEnd');
			
			//User just left
			socket.to(lRoom).emit("userLeave", {username:data.username, id:socket.id});
			
			destroy();
		}

		function destroy() {
			socket.disconnect(true); //true or false ?
		}
		
	});
});



class Room 
{
	/**
	 * 
	 * @param {string} name 
	 * @param {SocketIO.Socket} host 
	 */
	constructor(name, host) {
		this.name = name;
		this.host = host;
	}
}

/**
 * 
 * @param {SocketIO.Socket} socket 
 * @param {string} rawData 
 */
function TryParseJson(socket, rawData) {
	try {
		var data = JSON.parse(rawData);
	}
	catch(e) {
		console.log(rawData);
		console.log(e);
		const PARSE_ERROR = {message : 'JSON Parse error'};
		socket.emit("infoError", PARSE_ERROR);
		return null;
	}
	return data;
}

/**
 * 
 * @param {string} name 
 * @returns {number}
 */
function getRoomIndexByName(name)
{
	let index = -1;
	for (let i = rooms.length - 1; i >= 0; i--) {
		let lElement = rooms[i]
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
 * @param {SocketIO.Socket} socket
 * @returns {number}
 */
function getRoomIndexBySocket(socket)
{
	let index = -1;
	for (let i = rooms.length - 1; i >= 0; i--) {
		let lElement = rooms[i]
		if (lElement.host == socket) 
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
function getRoomHostByName(name)
{
	/**
	 * @type {SocketIO.Socket}
	 */
	let socket = null;
	for (let i = rooms.length - 1; i >= 0; i--) {
		let lElement = rooms[i]
		if (lElement.name == name) 
		{
			socket = lElement.host;
			break;
		}
	}

	return socket;
}