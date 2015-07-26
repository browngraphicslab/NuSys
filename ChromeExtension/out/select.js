
var	html = document.documentElement;
var isRunning = false;
var injected = false;
var addition = null;

chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
    chrome.tabs.sendMessage(tabs[0].id, { msg: "checkInjection" }, function (response) {

        ///check whether javascript files were already injected by sending message from select.js to main.js 
        if (response) {
            console.log("Already there");
            document.getElementById("cmn-toggle-1").checked = response.toggleState;
            injected = true;
        }
        else {
            console.log("wooohoo");
            $("#selected").empty();
            chrome.storage.local.set({ 'curr': [] });
            document.getElementById("cmn-toggle-1").checked = true;
            chrome.tabs.executeScript({ file: 'jquery.js' });
            chrome.tabs.executeScript({ file: 'NuSysChromeExtension.js' });
        }
    });
});

chrome.storage.local.get('curr', function (result) {
    addition = result.curr;
    console.log(result.curr);
    if (addition != null) {
        addToSelection(result.curr);
    }
});


$("#cmn-toggle-1").change(function () {
    chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
        chrome.tabs.sendMessage(tabs[0].id, { togglechanged: $("#cmn-toggle-1").is(':checked') }, function (response) {
            console.log("request for toggle change sent");
        })
    })
});

$("#view").click(function () {
    chrome.tabs.create({ 'url': chrome.extension.getURL('AllSelections.html') });

});

$("#clear").click(function () {
    chrome.storage.local.clear();

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
    console.log($('#selected').children());

}



var localPort;