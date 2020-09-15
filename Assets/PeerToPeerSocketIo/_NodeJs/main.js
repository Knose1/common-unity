const host = require("./src/host.js");
const client = require("./src/client.js");
const express = require('express');
const shortid = require('shortid');
const socketIO = require('socket.io')

const PORT = process.env.PORT || 3000;
const INDEX = '/index.html';

const server = express()
  .use((req, res) => res.sendFile(INDEX, { root: __dirname+"/docs" }))
  .listen(PORT, () => console.log(`Listening on ${PORT}`));

var io = socketIO(server, {
	transports: ['websocket'],
});


io.on('connection', function(socket){
	host.init(io, socket);
	client.init(io, socket);
});