var port;
var localPort;
var selectedContent = new Array();
var isOpen;
var _isOpen = false;
var _isActive = false;


chrome.extension.onMessage.addListener(function(request, sender, response) {

    if (request.msg == "set_active")
        setActive(request.data);

    if (request.msg == "query_active")
        response(_isActive);
});

chrome.browserAction.onClicked.addListener(function() {
    if (!_isOpen) {
        initAllTabs();
        _isOpen = true;
    }
    else {
        msgAllTabs({ msg: "hide_menu" });
        setActive(false);
        _isOpen = false;
    }
});

chrome.tabs.onUpdated.addListener(function(tabId, changeInfo, tab) {
    if (_isOpen && changeInfo.status == "complete") {
        console.log("WOOOOOOO");
        initTab(tabId);
    }
});

function initAllTabs() {
    chrome.tabs.query({}, function(tabs) {
        tabs.forEach(function(tab) {
            initTab(tab.id);
        });
    });
}

function initTab(tabId) {
    chrome.tabs.sendMessage(tabId, { msg: "check_injection" }, function (response) {

        if (!response) {
            console.log("injecting scripts in : " + tabId);
            chrome.tabs.executeScript(tabId, { file: "jquery.js" }, function (result) {
                if (chrome.runtime.lastError) { // or if (!result)
                    return;
                }
            });
            chrome.tabs.executeScript(tabId, { file: "javascriptUtil.js" }, function (result) {
                if (chrome.runtime.lastError) { // or if (!result)
                    return;
                }
            });
            chrome.tabs.executeScript(tabId, { file: "NuSysChromeExtension.js" }, function (result) {
                if (chrome.runtime.lastError) { // or if (!result)
                    return;
                }
                $.get(chrome.extension.getURL("menu.html"), function (data) {
                    chrome.tabs.sendMessage(tabId, { msg: "build_menu", data: data }, function () {
                        chrome.tabs.sendMessage(tabId, { msg: "show_menu" });
                        if (_isActive)
                            chrome.tabs.sendMessage(tabId, { msg: "enable_selections" });
                    });
                });

            });
        } else {
            chrome.tabs.sendMessage(tabId, { msg: "show_menu" });
        }
    });
}

function msgAllTabs(message) {
    chrome.tabs.query({}, function(tabs) {
        for (var i = 0; i < tabs.length; ++i) {
            chrome.tabs.sendMessage(tabs[i].id, message);
        }
    });
}

function setActive(active) {
    console.log("setActive to :" + active);
    _isActive = active;

    // change icon based on current state
    if (_isActive) {
        chrome.browserAction.setIcon({ path: { 19: "assets/icon_active.png", 38: "assets/icon_active.png" } });
        msgAllTabs({msg: "enable_selections"});
    } else {
        chrome.browserAction.setIcon({ path: { 19: "assets/icon.png", 38: "assets/icon.png" } });
        msgAllTabs({ msg: "disable_selections" });
    }
}

function isActive() {
    return _isActive;
}

function startConnection() {
    port = chrome.runtime.connectNative('com.browngraphicslab.chromenusysintermediate');
}

function log(msg, response) {
    console.log(msg);
}