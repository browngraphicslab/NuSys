
var _bg = chrome.extension.getBackgroundPage();

$("#toggle").prop("checked", _bg.isActive);

chrome.runtime.onMessage.addListener(function(message, sender, bla) {
    console.log("got message: " + message);
    $('body').append($("div"));
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

$("#toggle").change(function () {

    _bg.toggleActive();

    //   chrome.extension.getBackgroundPage().isEnabled = !chrome.extension.getBackgroundPage().isEnabled;
    chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
        //tells javascript in tab that there was a change in toggle value
        chrome.tabs.sendMessage(tabs[0].id, {
            toggleState: $("#toggle").is(':checked')
        }, function (response) {
            console.log("request for toggle change sent");
        });
    });

});



/*

function addToSelection(array) {
    console.log($("selected").data);
    $.each(array, function (index, value) {
        var res = value._content.split('//')
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
*/