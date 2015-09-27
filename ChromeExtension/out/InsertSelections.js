console.log("ADDED");


chrome.storage.local.get(null, function (data) {
    console.log(data);
    allData = data;
    delete allData["curr"];
    //	setData(data);
    //showInsertion(data);
    if (allData["selections"] != null) {
        sortByTime(allData);
    }   
});

$('#filter').change(function () {
    switch ($("#filter").val()) {
        case "chrono":
            alert("chrono");
            break;
        case "key":
            
            break;
        case "comment":
            break;
    }
});

$('#submit').click(function () {
    changeFilter();

});

function changeFilter() {
    switch ($("#filter").val()) {
        case "comment":
            alert($("#searchInput").val());
            break;
    }
}


function sortByTime(obj){
    var json = JSON.stringify(obj);
    var sortedJson = sortResults(obj['selections'], "urlGroup", false);
    var newJson = {};
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
    showInsertion(hash);
}

function sortResults(json, prop, asc) {
  
    sortedJson = json.sort(function (a, b) {
        if (asc) return (a[prop] > b[prop]) ? 1 : ((a[prop] < b[prop]) ? -1 : 0);
        else return (b[prop] > a[prop]) ? 1 : ((b[prop] < a[prop]) ? -1 : 0);
    });
    return sortedJson;
}
function injectScript(tab) {
    console.log("injectin!!!!!");
    chrome.tabs.executeScript({ file: 'jquery.js' });
    chrome.tabs.executeScript({ file: 'NuSysChromeExtension.js' });
}

$("#reset").click(function () {
    chrome.storage.local.clear();
    $("#container").empty();
});

function sendMessage(key) {
    chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
        chrome.tabs.sendMessage(tabs[0].id, { pastPage: key }, function (response) {
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
    console.log(data);
    $.each(data, function (index, val) {
        console.log(val);

        var title = document.createElement("h3");
        $(title).append("<span class='title'>" + val[0]["title"]);
        
        $(title).append("<span class='url'>" + val[0]["url"] + "</span>");
        $(title).append("<button class='toRemove button' type='button'>Remove</button>");
        $(title).append("<button class='pastPage button' type='button'>Open</button>");
       
        $(title).find(".pastPage").click(function () {
            chrome.tabs.create({ 'url': val[0]["url"] }, injectScript);
            sendMessage(val);
        });

        var selections = document.createElement("div");
        $.each(val, function (indx, v) {
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
        $("#container").append(title);
        $("#container").append(selections);

        $(title).find(".toRemove").click(function () {
            $(title).remove();
            $(selections).remove();
            chrome.storage.local.get(null, (data) =>{
                console.log(data);
                var newSelections = [];
                $(data["selections"]).each((indx, elem) =>{
                    console.log(val[0]["urlGroup"] == elem["urlGroup"]);
                    if (val[0]["urlGroup"]!= elem["urlGroup"]) {
                        newSelections.push(elem);
                    }
                });
                data["selections"] = newSelections;
                chrome.storage.local.set(data);
            });
        });
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
