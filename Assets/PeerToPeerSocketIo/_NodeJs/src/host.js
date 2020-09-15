const Room = require('./Room.js');
const Utils = require('./utils.js');

const DEFAULT_MIN_CAPACITY = 1;
const DEFAULT_MAX_CAPACITY = 6;

const HOST = 'host';
/**
 * 
 * @namespace Host
 */

/**
 * @memberof Host
 * @param {SocketIO.Server} io 
 * @param {SocketIO.Socket} socket 
 */
exports.init = (io, socket) => 
{
	socket.on(HOST, function(data){

		
		if (data == undefined || data == null) data = {};

		Utils.logSocket(socket, HOST);
		
		if (Room.isInRoom(socket)) 
		{
			Utils.logSocket(socket, HOST+' '+"socket already has a room");
			//bro u're in a room, whut u want from mi
			return;
		}
	
		//Create room
		let lRoom = new Room(socket, data.minCapacity || DEFAULT_MIN_CAPACITY, data.maxCapacity || DEFAULT_MAX_CAPACITY);
		
		//The host broadcast a message
		socket.on("sendMessage", (data) => 
		{
			let log = "";
			Utils.logSocket(socket,HOST+' '+'sendMessage '+log);
			socket.to(lRoom.name).emit("message", data);
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
	
			let log = Utils.dataToString(data);
			Utils.logSocket(socket,HOST+' '+'sendMessageTo '+log);
			
			if (!data)
			{
				console.warn("Data is null or undefined");
				return;
			}
			if (!data.id && !data.message)
			{
				const ID_AND_MESSAGE_NOT_SPECIFIED_ERROR = {message : "Id and message not specified"};
				Utils.logSocket(socket,HOST+' '+'sendMessageTo '+ID_AND_MESSAGE_NOT_SPECIFIED_ERROR.message);
	
				socket.emit("infoError", ID_AND_MESSAGE_NOT_SPECIFIED_ERROR);
				return;
			}
			if (!data.id)
			{
				const ID_NOT_SPECIFIED_ERROR = {message : "Id not specified"};
				Utils.logSocket(socket,HOST+' '+'sendMessageTo '+ID_NOT_SPECIFIED_ERROR.message);
				
				socket.emit("infoError", ID_NOT_SPECIFIED_ERROR);
				return;
			}
			
			let lClientSocket = lRoom.getUserBySocket(data.id);
			if (!lClientSocket) 
			{
				const WRONG_ID_ERROR = {message : "Wrong Id"};
				Utils.logSocket(socket,HOST+' '+'sendMessageTo '+WRONG_ID_ERROR.message);
				
				socket.emit("infoError", WRONG_ID_ERROR);
				return;
			}

			if (!data.message)
			{
				const MESSAGE_NOT_SPECIFIED_ERROR = {message : "Message not specified"};
				Utils.logSocket(socket,HOST+' '+'sendMessageTo '+MESSAGE_NOT_SPECIFIED_ERROR.message);
	
	
				socket.emit("infoError", MESSAGE_NOT_SPECIFIED_ERROR);
				return;
			}
	
			socket.to(data.id).emit("message", {message:data.message});
		});
	
		socket.on("kick", (data) => {
			
			/**
			 * @typedef KickData
			 * @property {string} id
			 */
	
			/**
			 * 
			 * @type {KickData} 
			 */
			data;
	
			let log = Utils.dataToString(data);
	
			Utils.logSocket(socket,HOST+' '+'kick '+log);
			
			if (!data)
			{
				console.warn("Data is null or undefined");
				return;
			}
			if (!data.id)
			{
				const ID_NOT_SPECIFIED_ERROR = {message : "Id not specified"};
				Utils.logSocket(socket,HOST+' '+'kick '+ID_NOT_SPECIFIED_ERROR.message);
				
				socket.emit("infoError", ID_NOT_SPECIFIED_ERROR);
				return;
			}
	
			let lClientSocket = lRoom.getUserBySocket(data.id);
			if (!lClientSocket) 
			{
				const WRONG_ID_ERROR = {message : "Wrong Id"};
				Utils.logSocket(socket,HOST+' '+'kick '+WRONG_ID_ERROR.message);
				
				socket.emit("infoError", WRONG_ID_ERROR);
				return;
			}

			lRoom.removeUser(lClientSocket);
			lClientSocket.disconnect(true);
		});
	
		socket.on("partyEnd", destroy);
		socket.on("disconnect", destroy);
	
		function destroy() {
			Utils.logSocket(socket,HOST+' '+'partyEnd or disconnect');
			lRoom.destroy();
			socket.disconnect(true); //true or false ?
		}
	});
};