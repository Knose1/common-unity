/**
 * If it's an object, stringify it
 * @param {any} data 
 * @returns {string | number | boolean}
 */
function dataToString(data) 
{
	if (typeof(data) == "object") {
		log = JSON.stringify(data);
	} else {
		log = data;
	}
}

/**
 * 
 * @param {SocketIO.Socket} socket 
 * @param {string} log 
 */
function logSocket(socket, log) 
{
	console.log("<"+socket.id+">"+": "+log);
}

/**
 * 
 * @param {SocketIO.Socket} socket 
 * @param {string} rawData 
 */
function tryParseJson(socket, rawData) {
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

exports.dataToString = dataToString;
exports.logSocket = logSocket;
exports.tryParseJson = tryParseJson;