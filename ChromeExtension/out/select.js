
var	html = document.documentElement;
var isRunning = false;

if(!isRunning) {
    chrome.tabs.executeScript({ file: 'NuSysChromeExtension.js' });
    isRunning = true;


    document.getElementById("btnSend").addEventListener("click", function(){
        console.log(chrome.extension.getBackgroundPage().port);
        console.log(selections)
    });
}

var localPort;