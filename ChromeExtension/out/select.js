
var	html = document.documentElement;
var isRunning = false;
var injected = false;
var addition = null;
var port = null;
var id = null;

chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
    chrome.tabs.sendMessage(tabs[0].id, { msg: "checkInjection" }, function (response) {

        //check whether javascript files were already injected into tab by sending message from select.js to main.js 
        
        if (response) {
            //retreive selected HTML segments if there is a response
            injected = true;            
            $("#cmn-toggle-1").prop('checked', response.toggleState);
            chrome.storage.local.get(response.objectId.toString(), function (result) {
                console.log(result);
                id = response.objectId.toString();
                console.log(id);
                console.log(result[id]);
                addition = result[id].selections;
                if (addition != null) {
                    addToSelection(addition);
                }
            });
        }
        else {
            //inject javascript files if there is no response 
            $("#cmn-toggle-1").prop('checked', false);
            port = chrome.runtime.connect({ name: "content" });
            $("#selected").empty();
            chrome.storage.local.set({ 'curr': [] });
            chrome.tabs.executeScript({ file: 'jquery.js' });
            chrome.tabs.executeScript({ file: 'javascriptUtil.js' });
            chrome.tabs.executeScript({ file: 'NuSysChromeExtension.js' });
        }
    });
});

$("#cmn-toggle-1").change(function () {

 //   chrome.extension.getBackgroundPage().isEnabled = !chrome.extension.getBackgroundPage().isEnabled;
    chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
        //tells javascript in tab that there was a change in toggle value
        chrome.tabs.sendMessage(tabs[0].id, {
            toggleState: $("#cmn-toggle-1").is(':checked')
        }, function (response) {
            console.log("request for toggle change sent");
        })
    });

});

$(".btnclose").click(function () {
    window.close();
});

$("#btnSend").click(function () {
    chrome.storage.local.get('curr', function (result) {
        chrome.extension.getBackgroundPage().port.postMessage(JSON.stringify(result.curr));
    });
});

$("#view").click(function () {
    chrome.tabs.create({ 'url': chrome.extension.getURL('AllSelections.html') });
});


$("#clear").click(function () {
    $("#selected").remove();
    chrome.storage.local.remove(id);
});

function addToSelection(array) {
    console.log($("selected").data);
    $.each(array, function (index, value) {
        var res = value.split('//');
        var newVal = "";
        for (var i = 0; i < res.length; i++) {

            newVal += res[i];
            if (i < res.length - 1 && res[i][res[i].length - 1] != ":") {
                newVal += "https://";
            }
        }
        $("#selected").append("<hr>");
        $("#selected").append("<div style='clear: both'>" + newVal + "</div>");

    });

}

var localPort;