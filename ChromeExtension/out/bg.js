var port;
var localPort;
var selectedContent = new Array();

var isEnabled = false;

chrome.runtime.onStartup.addListener(startConnection());

port.onDisconnect.addListener(function() {
	console.debug("disconnected");
});
chrome.runtime.onConnect.addListener(function(mesPort){
	localPort = mesPort;
	console.debug("local messaging connected");

	localPort.onMessage.addListener(function(msg) {
		port.postMessage(msg);
		console.debug("sent to nusys");
	})
});

function startConnection() {
	port = chrome.runtime.connectNative('com.browngraphicslab.chromenusysintermediate');
}

