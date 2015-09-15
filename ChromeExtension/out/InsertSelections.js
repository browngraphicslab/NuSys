console.log("ADDED");


chrome.storage.local.get(null, function (data) {
    console.log(data);
    allData = data;
    delete allData["curr"];

    console.info(data);
    console.log();
    //	setData(data);
    //showInsertion(data);
    sortByTime(data);
});
function sortByTime(obj){
    var json = JSON.stringify(obj);
    console.log(json);
    var sortedJson = sortResults(obj['selections'], "urlGroup", true);
    var newJson = {};
    console.log(sortedJson);
    var hash = {};
    $(sortedJson).each(function (indx, value) {
        var groupId = value["urlGroup"];
        if (hash[groupId] == null) {
            hash[groupId] = [value];
        }
        else {
            var list = hash[groupId];
            list = list.push(value);
        }
        
    });
    console.log(hash);

    showInsertion(hash);

}

function sortResults(json, prop, asc) {
  
    json = json.sort(function (a, b) {
        if (asc) return (a[prop] > b[prop]) ? 1 : ((a[prop] < b[prop]) ? -1 : 0);
        else return (b[prop] > a[prop]) ? 1 : ((b[prop] < a[prop]) ? -1 : 0);
    });

    return json;
}
function injectScript(tab) {
    console.log("injectin!!!!!");
    chrome.tabs.executeScript({ file: 'jquery.js' });
    
    chrome.tabs.executeScript({ file: 'NuSysChromeExtension.js' });
}

$("#reset").click(function () {
    console.log("DDDdd")
    chrome.storage.local.clear();
    $("#container").empty();
});

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
    $.each(data, function (index, val) {
        console.log(val);

        var title = document.createElement("h3");
        $(title).append("<span class='title'>" + val[0]["title"]);
        
        $(title).append("<span class='url'>" + val[0]["url"] + "</span>");
        $(title).append("<button class='toRemove button' type='button'>Remove</button>");
        $(title).append("<button class='pastPage button' type='button'>Open</button>");
        // $(title).append("<span>" + data[val]["date"] + "</span>");

        $(title).find(".pastPage").click(function () {
            chrome.tabs.create({ 'url': val[0]["url"] }, injectScript);
            sendMessage(val);
        });

        var selections = document.createElement("div");
        $.each(val, function (indx, v) {
            console.log(v);
            var content = v["_content"];
            var res = content.split('//');
            var newVal = "";
            for (var i = 0; i < res.length; i++) {
                newVal += res[i];
                if (i < res.length - 1 && res[i][res[i].length - 1] != ":") {
                    newVal += "https://";
                }
            }
            $(selections).append("<div style = 'clear:both'>" + newVal + "</div>");
        });
    //    $("#container").append(date);
        $("#container").append(title);
        $("#container").append(selections);

        //$(title).find(".toRemove").click(function () {
        //    $(title).remove();
        //    $(selections).remove();
        //    delete data[val];
        //    console.log(data);
        //    chrome.storage.local.remove(val);
        //});
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
