
var	html = document.documentElement;
var isRunning = false;
var injected = false;
var addition = null;
var port = null;
var id = null;

//$('#cmn-toggle-1').prop('checked', chrome.extension.getBackgroundPage().isEnabled);

//chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
//    console.log("sending: " + chrome.extension.getBackgroundPage().isEnabled)
//    chrome.tabs.sendMessage(tabs[0].id, { toggleState: chrome.extension.getBackgroundPage().isEnabled }, function (response) {
//        console.log("request for toggle change sent");
//    })
//})

$(".btnclose").click(function () {
    window.close();
});
chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
    chrome.tabs.sendMessage(tabs[0].id, { msg: "checkInjection" }, function (response) {

        ///check whether javascript files were already injected by sending message from select.js to main.js 
        if (response) {
            console.log("Already there");
            injected = true;
            console.log(response);
            
            $("#cmn-toggle-1").prop('checked', response.toggleState);
            console.log("!!!!!!!!!!!!11111111" + response.objectId);

            chrome.storage.local.get(response.objectId.toString(), function (result) {
                console.log(result);
                id = response.objectId.toString();
                console.log(id);
                console.log(result[id]);
                addition = result[id].selections;
               // console.log(result.curr);
                if (addition != null) {
                    addToSelection(addition);
                }
            });
          //  $("#cmn-toggle-1").prop('checked', response.toggleState);
        }
        else {
            console.log("woooo");
            $("#cmn-toggle-1").prop('checked', false);
            port = chrome.runtime.connect({ name: "content" });
            $("#selected").empty();
            chrome.storage.local.set({ 'curr': [] });
            chrome.tabs.executeScript({ file: 'jquery.js' });
            chrome.tabs.executeScript({ file: 'NuSysChromeExtension.js' });
        }
    });
});

//chrome.storage.local.get('curr', function (result) {
//    addition = result.curr;
//    console.log(result.curr);
//    if (addition != null) {
//        addToSelection(result.curr);
//    }
//});


//$("#cmn-toggle-1").change(function () {

//   chrome.extension.getBackgroundPage().isEnabled = !chrome.extension.getBackgroundPage().isEnabled;
//    chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
//        console.log("sending: " + chrome.extension.getBackgroundPage().isEnabled)
//        chrome.tabs.sendMessage(tabs[0].id, { toggleState: chrome.extension.getBackgroundPage().isEnabled }, function (response) {
//            console.log("request for toggle change sent");
//        })
//    });

//});


$("#cmn-toggle-1").change(function () {

 //   chrome.extension.getBackgroundPage().isEnabled = !chrome.extension.getBackgroundPage().isEnabled;
    chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
        console.log("sending: " + $("#cmn-toggle-1").is(':checked'));
        chrome.tabs.sendMessage(tabs[0].id, {
            toggleState: $("#cmn-toggle-1").is(':checked')
        }, function (response) {
            console.log("request for toggle change sent");
        })
    });

});

$("#btnSend").click(function () {
    chrome.storage.local.get('curr', function (result) {
        console.log(addition);
        console.log(JSON.stringify(result.curr));
        chrome.extension.getBackgroundPage().port.postMessage(JSON.stringify(result.curr));
    });
});

$("#view").click(function () {
    chrome.tabs.create({ 'url': chrome.extension.getURL('AllSelections.html') });

});

$("#clear").click(function () {
    $("#selected").remove();
    chrome.storage.local.remove(id);
    //chrome.storage.local.clear();

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