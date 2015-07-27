console.log("ADDED");


chrome.storage.local.get(null, function (data) {
    console.log(data);
    allData = data;
    delete allData["curr"];

    console.info(data);
    console.log();
    //	setData(data);
    showInsertion(data);
});

function injectScript(tab) {
    chrome.tabs.executeScript({ file: 'jquery.js' });
    chrome.tabs.executeScript({ file: 'HashMap.js' });
    chrome.tabs.executeScript({ file: 'NuSysChromeExtension.js' });
}

function sendMessage(key) {
    chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
        chrome.tabs.sendMessage(tabs[0].id, { pastPage: key }, function (response) {
            // console.log(response.farewell);\
            console.log("MESSAGE SENT!!!!");
            console.log(tabs);
            console.log(response);
            if (!response) {
                sendMessage(key);
            }
            else {
                console.log("MESSAGE RECEIVED From PASTPAGE");
            }
        });
    });
}

function showInsertion(data) {
    $.each(Object.keys(data).reverse(), function (index, val) {
        var title = document.createElement("h3");
        $(title).append("<span class='title'>" + data[val]["title"]);
        $(title).append("<span class='url'>" + data[val]["url"] + "</span>");
        $(title).append("<button class='pastPage' type='button'>Open</button>");
       // $(title).append("<span>" + data[val]["date"] + "</span>");
        $(title).find(".pastPage").click(function () {
            chrome.tabs.create({ 'url': data[val]["url"] }, injectScript);
            sendMessage(val);
        });
        var selections = document.createElement("div");
        $.each(data[val]["selections"], function (indx, v) {
            var res = v.split('//');
            var newVal = "";
            for (var i = 0; i < res.length; i++) {
                newVal += res[i];
                if (i < res.length - 1 && res[i][res[i].length - 1] != ":") {
                    newVal += "https://";
                }
            }
            $(selections).append("<div style = 'clear:both'>" + newVal + "</div>");
        });
        $("#container").append(title);
        $("#container").append(selections);
    });

    $("#container").accordion({active: false,  collapsible: true});
}


function setData(data) {

    $.each(allUrls, function (index, url) {
        console.log(url);
        var div = document.createElement("div");
        div.setAttribute("id", index);
        document.body.appendChild(div);
        $("#" + index).append("<p style='font-size:160%' >" + url + "</p>");
        console.log(allData[url]);
        $.each(allData[url], function (indx, val) {
            var res = val.split('//');
            var newVal = "";
            for (var i = 0; i < res.length; i++) {
                newVal += res[i];
                if (i < res.length - 1 && res[i][res[i].length - 1] != ":") {
                    newVal += "https://";
                }
            }
            $("#" + index).append("<div style = 'clear:both'>" + newVal + "</div>");
        })

        $("#" + index).append("<hr>");
        document.getElementById("container").appendChild(div);

    })
}
