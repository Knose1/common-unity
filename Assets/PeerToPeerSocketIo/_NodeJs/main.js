const { json } = require('express');
const host = require("./src/host.js");
const client = require("./src/client.js");

var io = require('socket.io')({
	transports: ['websocket'],
});
io.attach(4567);


/**
 * List of all events :
 * @namespace PeerToPeerServer
 */

io.on('connection', function(socket){
	host.init(io, socket);
	client.init(io, socket);
});